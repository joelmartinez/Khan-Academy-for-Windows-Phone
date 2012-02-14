using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using System.Web;

namespace KhanProxy.Services
{
    public sealed class KhanHtmlParser
    {
        private HtmlDocument htmlDocument;
        private Func<HtmlDocument> lazy;

        #region Constructors

        /// <summary>A lazy evaluation method so we can defer the creation of the HTML document
        /// in case things are cached.</summary>
        public KhanHtmlParser(Func<HtmlDocument> lazyAction)
        {
            if (lazyAction == null) throw new ArgumentNullException("lazyAction");

            this.lazy = lazyAction;
        }

        /// <summary>An already initialized html document</summary>
        public KhanHtmlParser(HtmlDocument doc)
        {
            if (doc == null) throw new ArgumentNullException("doc");

            this.htmlDocument = doc;
        }

        #endregion

        private HtmlDocument html
        {
            get
            {
                if (this.htmlDocument == null) this.htmlDocument = lazy();

                return this.htmlDocument;
            }
        }

        public static KhanHtmlParser Create()
        {
            bool fromWeb = true;
            HtmlDocument htmlDoc;

            if (fromWeb)
            {
                return new KhanHtmlParser(() =>
                {
                    var khanHomeUri = "http://khanacademy.com".AsUri();
                    HtmlAgilityPack.HtmlWeb web = new HtmlWeb() { UserAgent = "Khan Spider for Windows Phone 7 viewer app" };
                    web.AutoDetectEncoding = true;
                    return web.Load(khanHomeUri.ToString());
                });
            }
            else
            {
                // just use the local version
                return new KhanHtmlParser(() =>
                {
                    htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    // no http context in a wcf service? ok, hardcoding path for now. 
                    // change path for your local env for now
                    // string filePath = HttpContext.Current.Server.MapPath("~/Services/local.html"); ;
                    string filePath = @"C:\Users\joel\Documents\Visual Studio 2010\Projects\KhanSvn2\KhanProxy\Services\local.htm";
                    string html = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    htmlDoc.LoadHtml(html);
                    return htmlDoc;
                });
            }
        }

        #region Public Methods

        public KhanCategory[] GetCategories()
        {
            return CacheHelper.Get("categories",
                () =>
                {
                    return Parse(
                            "http://khanacademy.com".AsUri(),
                            "//h2[@class='playlist-heading']",
                            n => new KhanCategory { Name = n.InnerText })
                            .ToArray();
                },
                    DateTime.Now.AddHours(1));
        }

        public KhanVideo[] GetVideos()
        {
            return CacheHelper.Get("videos",
                () =>
                {
                    return Parse(
                            "http://www.khanacademy.org".AsUri(),
                            "//h2[@class='playlist-heading']|//a[@class='video-link']",
                            VidNav,
                            n => new KhanVideo
                            {
                                Category = n.Attributes["category"].Value,
                                Uri = Pathify(n.Attributes["href"].Value),
                                Name = n.InnerText
                            })
                            .ToArray();
                },
                    DateTime.Now.AddHours(1));
        }

        private string Pathify(string p)
        {
            if (p.StartsWith("http")) return p;
            if (!p.StartsWith("/")) p = "/" + p;
            return "http://www.khanacademy.org" + p;
        }

        #endregion

        #region Private Methods

        private IEnumerable<T> Parse<T>(Uri khanHomeUri, string xpath, Func<HtmlNode, T> action)
        {
            return Parse<T>(khanHomeUri, xpath,
                DefaultNav,
                action);
        }

        private IEnumerable<T> DefaultNav<T>(HtmlNodeCollection html, Func<HtmlNode, T> action)
        {
            foreach (var item in html) yield return action(item);
        }

        private IEnumerable<T> VidNav<T>(HtmlNodeCollection html, Func<HtmlNode, T> action)
        {
            string cat = string.Empty;
            foreach (var item in html)
            {
                var category = item.InnerText;
                if (cat != category && item.Name != "a")
                {
                    cat = category;
                    continue;
                }
                var catattribute = item.Attributes["href"].Clone();
                catattribute.Name = "category";
                catattribute.Value = cat;
                item.Attributes.Add(catattribute);
                yield return action(item);
            }
        }

        private IEnumerable<T> Parse<T>(Uri khanHomeUri, string xpath, Func<HtmlNodeCollection, Func<HtmlNode, T>, IEnumerable<T>> navigate, Func<HtmlNode, T> action)
        {
            // TODO: Cache, WebRequest

            // There are various options, set as needed
            html.OptionFixNestedTags = true;

            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (html.ParseErrors != null && html.ParseErrors.Count() > 0)
            {
                // Handle any parse errors as required

            }
            else
            {

                if (html.DocumentNode != null)
                {
                    var nodes = html.DocumentNode.SelectNodes(xpath);

                    return navigate(nodes, action);

                }
            }
            return new T[0];
        }

        #endregion
    }
}