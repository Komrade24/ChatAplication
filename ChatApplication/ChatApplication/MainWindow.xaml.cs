using ChatClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApplication
{
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private List<TcpClient> connectedClients = new List<TcpClient>();
        private Client client;

        public MainWindow()
        {
            InitializeComponent();
        }
        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            Username.Text = null;
            ServerIP.Text = null;
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                connectedClients.Add(client);
                _ = Dispatcher.Invoke(() => HandleClientCommunication(client));
            }
        }
        private async void ConnectServer_Click(object sender, RoutedEventArgs e)
        {
            client = new Client();
            client.Uesrname = Username.Text;
            client.AdressIP = ServerIP.Text;
            Username.Text = null;
            ServerIP.Text = null;

            await Task.Run(() =>
            {
                if (client != null)
                {
                    Dispatcher.Invoke(() => client.Connect());
                }
            });
        }

        private async Task HandleClientCommunication(TcpClient client)
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
                    Console.WriteLine("Received: " + receivedMessage);

                    // Выводим сообщение в TextBlock
                    Dispatcher.Invoke(() =>
                    {
                        ChatBox.Text += receivedMessage + Environment.NewLine;
                    });

                    // Отправка сообщения всем клиентам, кроме отправителя
                    await BroadcastMessageAsync(receivedMessage, client);
                }
            }
            finally
            {
                connectedClients.Remove(client);
                client.Close();
            }
        }

        private async Task BroadcastMessageAsync(string message, TcpClient excludeClient)
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

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            MessageTextBox.Text = null;
            foreach (TcpClient client in connectedClients)
            {
                NetworkStream clientStream = client.GetStream();
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                //ChatBox.Text = Encoding.UTF8.GetString(messageBytes);
                clientStream.Write(messageBytes, 0, messageBytes.Length);
            }
        }

    }
}