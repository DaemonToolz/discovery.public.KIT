using System;
using discovery.KIT.p2p.Models;

namespace discovery.KIT.netmq.events
{
    public class CommandEventArgs<T> : EventArgs
    {
        public T Data;
    }

    public class DiscoveryEventArgs : EventArgs
    {
        public DiscoveryFrame Data;
    }
}
