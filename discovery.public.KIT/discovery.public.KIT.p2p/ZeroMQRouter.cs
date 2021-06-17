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
    internal class ZeroMQHandler
    {
        public bool Authenticated;
        public bool MarkedReady;
        public string Address;
        public DateTimeOffset LastCall;

        public ZeroMQHandler(string addr = "")
        {
            Authenticated = false;
            MarkedReady = false;
            Address = addr;
        }
    }

    public class ZeroMQRouter : IDisposable {
        private volatile EventManager _eventManager = new EventManager();

        private readonly RouterSocket _router;
        private readonly NetMQPoller _incomingPoller;
        private readonly NetMQPoller _outgoingPoller;
        private readonly NetMQQueue<NetMQMessage> _outputQueue;
        private readonly ConcurrentDictionary<string, ZeroMQHandler> _ConnectedDealers = new ConcurrentDictionary<string, ZeroMQHandler>();
        private readonly NetMQCertificate _serverCertificate;
        public byte[] PublicCertificate => _serverCertificate.PublicKey;

        public RouterSocket Router => _router;
        public ZeroMQRouter(int port)
        {
            _serverCertificate = new NetMQCertificate();

            _router = new RouterSocket($"tcp://*:{port}");
            _router.Options.RouterMandatory = true;
            _router.Options.Identity = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());
            _router.Options.HeartbeatInterval = new TimeSpan(0, 0, 0, 0, 100);
            _router.ReceiveReady += RouterOnReceiveReady;
            _router.Options.CurveServer = true;
            _router.Options.CurveCertificate = _serverCertificate;
            
            _outputQueue = new NetMQQueue<NetMQMessage>();
            _outputQueue.ReceiveReady += MessageQueue_ReceiveReady;
            
            _incomingPoller = new NetMQPoller {_router };
            _outgoingPoller = new NetMQPoller { _outputQueue };

            _incomingPoller.RunAsync(Guid.NewGuid().ToString(), true);
            _outgoingPoller.RunAsync(Guid.NewGuid().ToString(), true);

        }



        public void Restart()
        {
            Stop();
            _incomingPoller.RunAsync(Guid.NewGuid().ToString(), true);
            _outgoingPoller.RunAsync(Guid.NewGuid().ToString(), true);
        }

        public void Stop()
        {
            _incomingPoller.Stop();
            _outgoingPoller.Stop();
        }

        private void RouterOnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
             var clientMessage = new NetMQMessage();
             
            try
            {
                if (!e.Socket.TryReceiveMultipartMessage(ref clientMessage))
                {
                    e.Socket.TrySignalError();
                    Console.Error.WriteLine("Empty message the message");
                    return;
                }
                var from = clientMessage[0].ConvertToString();
                try
                {
                    if (clientMessage.FrameCount == 2)
                    {
                        Console.Error.WriteLine("Signal Received");
                        return;
                    }
                    var clientAddress = clientMessage[1].ConvertToString();
                    MarkConnected(from, clientAddress);
                    UpdateCallTime(from);
                    var clientOriginalMessage = JsonConvert.DeserializeObject(clientMessage[2].ConvertToString());

                    Task.Run(() =>
                    {

                        _eventManager.OnCommandReceived(new CommandEventArgs<object>()
                        {
                            Data = new Message()
                            {
                                Channel = from,
                                Address = clientAddress,
                                Payload = clientOriginalMessage
                            }
                        });
                    });
                    e.Socket.TrySignalOK();
                }
                catch (Exception ex)
                {
                    e.Socket.TrySignalError();
                    Console.Error.WriteLine(ex);
                }
            } catch(Exception ex)
            {
                e.Socket.TrySignalError();
                Console.Error.WriteLine(ex);
            }

        }

        public void MarkConnected(string channel, string clientAddress)
        {
            if (_ConnectedDealers.Any(kv => kv.Key == channel)) return;
            _ConnectedDealers.TryAdd(channel, new ZeroMQHandler(clientAddress));
        }

        private void UpdateCallTime(string channel)
        {
            if (_ConnectedDealers.Any(kv => kv.Key == channel)) return;
            _ConnectedDealers[channel].LastCall = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset GetLastCall(string channel)
        {
            return _ConnectedDealers.Any(kv => kv.Key == channel) ? DateTimeOffset.MinValue : _ConnectedDealers[channel].LastCall;
        }

        public void MarkDisconnected(string channel)
        {
            _ConnectedDealers.TryRemove(channel, out var _);
        }


        public void Validate(string client)
        {
            _ConnectedDealers[client].Authenticated = true;
        }

        public List<string> GetChannels()
        {
            return _ConnectedDealers.Keys.ToList();
        }

        public void Broadcast(object message)
        {
            foreach (var key in _ConnectedDealers.Keys.Where(key => _ConnectedDealers[key] != null))
            {
                SendMessage(key, message);
            }
        }

        public bool IsAuthenticated(string ipAddress)
        {
            return _ConnectedDealers.Any(kv => kv.Value.Address == ipAddress) && _ConnectedDealers.First(kv => kv.Value.Address == ipAddress).Value.Authenticated;
        }

        public void SendMessage(string address, object message)
        {
            if (!_ConnectedDealers.Any(kv => kv.Key.Equals(address))) return;

            var messageToClient = new NetMQMessage();
            messageToClient.Append(address);
            messageToClient.Append(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString());
            messageToClient.Append(JsonConvert.SerializeObject(message));
            messageToClient.Append(JsonConvert.SerializeObject(new CommunicationMetadata
            {
                MessageStampedAt = DateTimeOffset.UtcNow
            }));
            _outputQueue.Enqueue(messageToClient);
        }
        private void MessageQueue_ReceiveReady(object sender, NetMQQueueEventArgs<NetMQMessage> e)
        {
            if (!e.Queue.TryDequeue(out var messageToServer, TimeSpan.FromMilliseconds(250))) return;
            try
            {
                _router.SendMultipartMessage(messageToServer);
            }
            catch (HostUnreachableException hue)
            {
                Console.Error.WriteLine(hue);
                MarkDisconnected(messageToServer.First.ConvertToString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            Stop();
            _incomingPoller.Dispose();
            _outgoingPoller.Dispose();
            _outputQueue.Dispose();
            _router.Dispose();
        }
    }
}
