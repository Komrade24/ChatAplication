using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class Server
    {
        private TcpListener listener;
        private Thread listenThread;
        private bool isListening;
        private List<TcpClient> connectedClients = new List<TcpClient>();
        public string AddAdressServerIP { get; set; }
        public Server()
        {
            listener = new TcpListener(IPAddress.Any, 8888);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            isListening = false;
        }

        public async void Start()
        {
            if (!isListening)
            {
                listener = new TcpListener(IPAddress.Parse(AddAdressServerIP), 8888);
                listener.Start();
                isListening = true;

                await Task.Run(async () =>
                {
                    while (true)
                    {
                        TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                        HandleClientCommunicationAsync(tcpClient);
                    }
                });
            }
        }

        private async void HandleClientCommunicationAsync(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = await clientStream.ReadAsync(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string receivedMessage = Encoding.UTF8.GetString(message, 0, bytesRead);
                Console.WriteLine("Received: " + receivedMessage);

                // Отправка сообщения всем клиентам, кроме отправителя
                BroadcastMessage(receivedMessage, tcpClient);
            }

            tcpClient.Close();
        }

        private async void BroadcastMessage(string message, TcpClient excludeClient)
        {
            foreach (TcpClient connectedClient in connectedClients)
            {
                if (connectedClient != excludeClient)
                {
                    NetworkStream clientStream = connectedClient.GetStream();
                    byte[] broadcastBytes = Encoding.UTF8.GetBytes(message);
                    await clientStream.WriteAsync(broadcastBytes, 0, broadcastBytes.Length);
                }
            }
        }
        private async void ListenForClients()
        {
            listener.Start();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunicationAsync));
                clientThread.Start(client);
            }
        }
    }
}