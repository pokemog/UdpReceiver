using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPReceiver
{
    class Program
    {
        private readonly static UdpClient _udpClient = new UdpClient(6000);
        //private readonly Action<int> m_consumer = new Action<int>();
        private static AsyncProducerConsumerQueue<string> _asyncProducerConsumerQueue;
        private static readonly IPEndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        static void Main(string[] args)
        {
            _asyncProducerConsumerQueue = new AsyncProducerConsumerQueue<string>(ProcessUdpData);
            Console.WriteLine("Starting Udp listening");
            ReceivePacketsAsync();
            Console.Read();
        }

        private static void ReceivePacketsAsync()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var dataReceived = await _udpClient.ReceiveAsync();
                    if (dataReceived == null)
                    {

                    }
                    var data = Encoding.UTF8.GetString(dataReceived.Buffer);
                    var queueSize = await _asyncProducerConsumerQueue.GetQueueSize();
                    Console.WriteLine($"Data received: {data} and adding to queue with size {queueSize}");
                    await _asyncProducerConsumerQueue.ProduceAsync(data);
                }
            });

        }

        private static void ProcessUdpData(string udpData)
        {
            Task.Run(async () =>
            {
                Console.WriteLine($"Taking {udpData} from queue and processing");
                Thread.Sleep(100);
            });
        }
    }


}
