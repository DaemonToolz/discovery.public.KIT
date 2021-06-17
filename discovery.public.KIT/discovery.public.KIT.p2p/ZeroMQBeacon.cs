using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using discovery.KIT.netmq.events;
using discovery.KIT.p2p.Models;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace discovery.KIT.p2p
{


    public class ZeroMQBeacon : IDisposable
    {
        private volatile EventManager _eventManager = new EventManager();

        private readonly NetMQBeacon _router;
        private readonly NetMQPoller _incomingPoller;
        private readonly TimeSpan _timeoutSpan = new TimeSpan(0, 0, 0, 5);
        private readonly DiscoveryFrame _whoAmI;
        public NetMQBeacon Router => _router;
        public ZeroMQBeacon(int port, int rPort, byte[] cert)
        {
            _whoAmI = new DiscoveryFrame()
            {
                RouterPort = rPort,
                Certificate = cert
            }; 
            _router = new NetMQBeacon();
            _router.Configure(port);
            _router.ReceiveReady += RouterOnReceiveReady;


            _incomingPoller = new NetMQPoller { _router };

            _incomingPoller.RunAsync(Guid.NewGuid().ToString(), true);

        }

        public void Restart()
        {
            Stop();
            _incomingPoller.RunAsync(Guid.NewGuid().ToString(), true);
        }

        public void Stop()
        {
            _incomingPoller.Stop();
        }

        private void RouterOnReceiveReady(object sender, NetMQBeaconEventArgs e)
        {
        
            try
            {
                if (!e.Beacon.TryReceive(_timeoutSpan, out var beaconMessage ))
                {
                    Console.Error.WriteLine("Empty message the message");
                    return;
                }
                var bindingMessage = JsonConvert.DeserializeObject<DiscoveryFrame>(beaconMessage.String);
                bindingMessage.Address = beaconMessage.PeerAddress;
                bindingMessage.Host = beaconMessage.PeerHost;
                try
                {
                    Task.Run(() =>
                    {
                        _eventManager.OnDiscoveryReceived(new DiscoveryEventArgs()
                        {
                            Data = bindingMessage
                        });
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

        }

        public void PublicizeMe()
        {
            _router.Publish(JsonConvert.SerializeObject(_whoAmI), new TimeSpan(0, 0, 1));
        }

        public void Subscribe(string filter = "")
        {
            _router.Subscribe(filter);
        }

        public void PrivatizeMe()
        {
            _router.Silence(); 
        }

        public void Dispose()
        {
            Stop();
            _incomingPoller.Dispose();
            _router.Dispose();
        }
    }
}
