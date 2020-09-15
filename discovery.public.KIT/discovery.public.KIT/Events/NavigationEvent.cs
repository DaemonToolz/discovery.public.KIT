using System;
using Windows.ApplicationModel.Appointments;

namespace discovery.KIT.Models
{
    public class NavigationEventArgs<T> : EventArgs 
    {
        public T Data;
        public object AdditionalPayload;
        public NavigationEvent NavigationEvent;

    }

 
    public enum DataSourceEventType
    {
        Insert,
        Update,
        Delete
    }

    public enum NavigationEvent
    {
        CreateOrUpdate,
        Summary,
        Welcome,
        Delete,
        LogIn,
        Disconnect,
        Back
    }


}
