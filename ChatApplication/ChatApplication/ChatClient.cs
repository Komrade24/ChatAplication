using ChatServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace ChatClient
{
    class Client
    {
        private TcpClient client;
        public string AdressIP { get; set; }
        public string Uesrname { get; set; }
        public void Connect()
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(AdressIP), 8888); 
            StartClient();
        }

        private async void StartClient()
        {
            try
            {
                NetworkStream clientStream = client.GetStream();
                byte[] message = new byte[4096];
                int bytesRead;

                while (true)
                {
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
                    Console.WriteLine("Received from server: " + receivedMessage);

                    // Добавим сообщение в локальный чат-бокс
                    // Предполагается, что в вашем приложении есть TextBox для отображения сообщений от сервера
                    // Например, ServerMessagesTextBox.Text += receivedMessage + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Client error: " + ex.Message);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                NetworkStream clientStream = client.GetStream();
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                clientStream.Write(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message: " + ex.Message);
            }
        }
    }
}