using System;
using System.Windows;

#if WINDOWS_PHONE
using System.Windows.Threading;
#else
using Windows.UI.Core;
#endif

namespace KhanViewer.Models
{
    public static class UIThread
    {
#if !WINDOWS_PHONE
        private static readonly CoreDispatcher Dispatcher;

        static UIThread()
        {
            // Store a reference to the current Dispatcher once per application
            Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        /// <summary>
        ///   Invokes the given action on the UI thread - if the current thread is the UI thread this will just invoke the action directly on
        ///   the current thread so it can be safely called without the calling method being aware of which thread it is on.
        /// </summary>
        public static void Invoke(Action action)
        {
            if (Dispatcher.HasThreadAccess)
                action.Invoke();
            else
                Dispatcher.Invoke(CoreDispatcherPriority.Normal, (s,a) => action(), null, null);
        }
#else
        private static readonly Dispatcher Dispatcher;

        static UIThread()
        {
            // Store a reference to the current Dispatcher once per application
            Dispatcher = Deployment.Current.Dispatcher;
        }

        /// <summary>
        ///   Invokes the given action on the UI thread - if the current thread is the UI thread this will just invoke the action directly on
        ///   the current thread so it can be safely called without the calling method being aware of which thread it is on.
        /// </summary>
        public static void Invoke(Action action)
        {
            if (Dispatcher.CheckAccess())
                action.Invoke();
            else
                Dispatcher.BeginInvoke(action);
        }
#endif
    }
}
