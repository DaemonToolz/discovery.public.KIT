using System;
using System.Collections.Generic;
using System.Text;

namespace discovery.KIT.p2p.Models
{
    public struct Message
    {
        public string Channel;
        public string Address;
        public object Payload;
        public CommunicationMetadata Metadata;
    }

}
