using System;
using Windows.ApplicationModel.Appointments;

namespace discovery.KIT.Models
{
    public class NavigationEventArgs<T> : EventArgs 
    {
        public T Data;
        public NavigationEvent NavigationEvent;
    }

    public enum NavigationEvent
    {
        CreateOrUpdate,
        Summary,
        Welcome,
        LogIn,
        QueryFilters,
        ImportExport,
        P2P,
        Back
    }

    public class DataUpdatedEventArgs<T> : EventArgs
    {
        public T Data;
    }


}
