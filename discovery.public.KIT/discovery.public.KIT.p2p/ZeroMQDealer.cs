using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using discovery.KIT.netmq.events;
using discovery.KIT.p2p.Models;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace discovery.KIT.p2p
{
    public class ZeroMQDealer : IDisposable
    {
        private volatile EventManager _eventManager = new EventManager();

        private readonly DealerSocket _dealer;
        private readonly NetMQPoller _shardPoller;
        private readonly NetMQQueue<NetMQMessage> _outputQueue;
        private readonly ConcurrentQueue<CommandEventArgs<object>> _receivedMessages;
        
        public DateTimeOffset LastSentMessage { get; private set; }

        public ConcurrentQueue<CommandEventArgs<object>> Received => _receivedMessages;
        public NetMQQueue<NetMQMessage> OutputQueue => _outputQueue;

        private readonly string _remoteChannel;
        private readonly string _remoteAddress;
        private readonly int _remotePort;
        private readonly int _localPort;

        private readonly NetMQCertificate _dealerCertificate;

        public ZeroMQDealer(int listPort, string remoteAddress, int remotePort, DiscoveryFrame discoveryFrame )
        {
            _dealerCertificate = new NetMQCertificate();

            _receivedMessages = new ConcurrentQueue<CommandEventArgs<object>>();
            _dealer = new DealerSocket($"tcp://*:{_localPort = listPort}");
            _dealer.Options.CurveServerKey = discoveryFrame.Certificate;
            _dealer.Options.CurveCertificate = _dealerCertificate;

            _dealer.Options.Identity = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());
            _dealer.Options.HeartbeatInterval = new TimeSpan(0, 0, 0, 0, 100);
            _dealer.ReceiveReady += RouterOnReceiveReady;

            _outputQueue = new NetMQQueue<NetMQMessage>();
            _outputQueue.ReceiveReady += MessageQueue_ReceiveReady;

            _shardPoller = new NetMQPoller { _dealer, _outputQueue };
            try
            {
                _dealer.Connect($"tcp://{_remoteAddress = remoteAddress}:{_remotePort = remotePort}");
                _shardPoller.RunAsync(Guid.NewGuid().ToString(), true);
            } catch(Exception e)
            {
                _ = e;
            }
        }


        private void DealerOnSendReady(object sender, NetMQSocketEventArgs e)
        {
            Console.Error.WriteLineAsync("Dealer Ready");
        }

        public void Restart()
        {
            Stop();
            _shardPoller.RunAsync(Guid.NewGuid().ToString(), true);
        }

        public void Stop()
        {
            _shardPoller.Stop();
        }

        private void RouterOnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var clientMessage = new NetMQMessage();
            if (!e.Socket.TryReceiveMultipartMessage(ref clientMessage)) return;
            try
            {
                if (clientMessage.FrameCount == 2)
                {
                    Console.Error.WriteLine("Signal Received");
                    return;
                }
                var clientAddress = clientMessage[0].ConvertToString();
                var clientOriginalMessage = JsonConvert.DeserializeObject(clientMessage[1].ConvertToString());
                var messageAt = JsonConvert.DeserializeObject<CommunicationMetadata>(clientMessage[2].ConvertToString());
                
                _eventManager.OnCommandReceived(new CommandEventArgs<object>() {
                    Data = new Message() {
                        Address = clientAddress,
                        Payload = clientOriginalMessage,
                        Metadata = messageAt
                    }
                }); 
                e.Socket.SignalOK();
            } catch(Exception ex)
            {
                e.Socket.SignalError();
                Console.Error.WriteLine(ex);
            }
        }

        public void SendEmptyFrames()
        {
            var messageToServer = new NetMQMessage();
            messageToServer.Append(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString());
            _outputQueue.Enqueue(messageToServer);
        }

        public void SendMessage(object message)
        {
            var messageToServer = new NetMQMessage();
            messageToServer.Append(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString());
            messageToServer.Append(JsonConvert.SerializeObject(message));
            
            _outputQueue.Enqueue(messageToServer);
        }

        private void MessageQueue_ReceiveReady(object sender, NetMQQueueEventArgs<NetMQMessage> e)
        {
            try
            {
                if (!e.Queue.TryDequeue(out var messageToServer, TimeSpan.FromMilliseconds(50))) return;
                if (_dealer.TrySendMultipartMessage(messageToServer))
                {
                    LastSentMessage = DateTimeOffset.UtcNow;
                }
            } catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            Stop();
            _dealer.Disconnect($"tcp://{_remoteAddress}:{_remotePort}");
            _shardPoller.Dispose();
            _outputQueue.Dispose();
            _dealer.Dispose();
        }
    }
}
