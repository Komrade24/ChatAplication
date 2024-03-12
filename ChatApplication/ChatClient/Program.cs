using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    class Client
    {
        private TcpClient client;

        public void Connect()
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse("серверIP"), 8888);

            StartClient();
        }

        private void StartClient()
        {
            NetworkStream clientStream = client.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                string input = Console.ReadLine();
                message = Encoding.UTF8.GetBytes(input);
                clientStream.Write(message, 0, message.Length);
            }
        }
    }
}