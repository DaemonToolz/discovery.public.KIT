using System;
using discovery.KIT.Models;

namespace discovery.KIT.Events
{
    public class EventManager {
        public static event EventHandler<NavigationEventArgs<object>> Handler;

        public virtual void OnNavigationEvent(NavigationEventArgs<object> e)
        {
            Handler?.Invoke(this, e);
        }


        public static event EventHandler<DataUpdatedEventArgs<object>> DataUpdatedHandler;

        public virtual void OnDataUpdated(DataUpdatedEventArgs<object> e)
        {
            DataUpdatedHandler?.Invoke(this, e);
        }

    }


}
