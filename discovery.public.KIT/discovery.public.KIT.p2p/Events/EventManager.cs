using System;

namespace discovery.KIT.netmq.events
{
    public class EventManager
    {
        public static event EventHandler<CommandEventArgs<object>> CommandHandler;

        public virtual void OnCommandReceived(CommandEventArgs<object> e)
        {
            CommandHandler?.Invoke(this, e);
        }

        public static event EventHandler<DiscoveryEventArgs> DiscoveryHandler;

        public virtual void OnDiscoveryReceived(DiscoveryEventArgs e)
        {
            DiscoveryHandler?.Invoke(this, e);
        }

    }
}
