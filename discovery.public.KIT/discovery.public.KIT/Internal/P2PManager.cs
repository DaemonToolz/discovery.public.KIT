using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using discovery.KIT.Events;
using discovery.KIT.Models;
using discovery.KIT.p2p;
using discovery.KIT.p2p.Models;

namespace discovery.KIT.Internal
{
    public static class P2PManager
    {
        private static ZeroMQRouter _router;
        private static ZeroMQDealer _connection;
        private static ZeroMQBeacon _beacon;
        private static EventManager _manager;


        private static readonly List<DiscoveryFrame> _discoveredSystems = new List<DiscoveryFrame>();

        public static List<DiscoveryFrame> DiscoveredSystems => _discoveredSystems;
        public static void Init()
        {
            StartAsNode();

            _beacon = new ZeroMQBeacon(10510, 10500, _router.PublicCertificate);
            _manager = new EventManager();
            _beacon.Subscribe();
            _beacon.PublicizeMe();
        }

        static P2PManager(){
          

            netmq.events.EventManager.DiscoveryHandler += (sender, args) =>
            {
                var found = _discoveredSystems.FindIndex(data => data.Address == args.Data.Address);
                if (found >= 0)
                {
                    if (_discoveredSystems[found].Certificate == args.Data.Certificate) return;
                    _discoveredSystems.RemoveAt(found);
                    _discoveredSystems.Add(args.Data);
                }
                else
                {
                    _discoveredSystems.Add(args.Data);
                }

                _manager.OnDataUpdated(new DataUpdatedEventArgs<object>());
            };
        }

        public static void PeerToSystem(DiscoveryFrame data)
        {
            try
            {
                DisconnectFromPeer();
                _connection = new ZeroMQDealer(10520, data.Host, data.RouterPort, data);
                Thread.Sleep(50);
                _connection?.SendEmptyFrames();
                _beacon.PrivatizeMe();
                _beacon.Stop();
            }
            catch
            {
                DisconnectFromPeer();
                _beacon.Subscribe();
                _beacon.PublicizeMe();
            }
        }

        public static void DisconnectFromPeer()
        {
            try
            {
                _connection?.Stop();
                _connection?.Dispose();

            }
            catch
            {

            }
            finally
            {
                _beacon.Subscribe();
                _beacon.PublicizeMe();
            }
        }

        public static void StartAsNode()
        {
            StopNode();
            _router = new ZeroMQRouter(10500);
        }

        public static void StopNode()
        {
            _router?.Stop();
        }

        public static void SendData(object data)
        {
            _connection?.SendMessage(data);
            Thread.Sleep(50);
            _connection?.SendEmptyFrames();
        }

    }
}
