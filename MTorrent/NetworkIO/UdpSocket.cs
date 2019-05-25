// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Torrent.NetworkIO
{
    public class UdpSocket
    {
        private readonly Socket _udpSocket;
        private readonly IPEndPoint _localEndpoint;


        public UdpSocket(int port)
        {
            _localEndpoint = new IPEndPoint(IPAddress.IPv6Any, port);
            _udpSocket = GetSocket();
        }

        private static Socket GetSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork | AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            return socket;
        }

        public void StartTest()
        {
            for (int i = 1; i < 100; i++)
            {
                if (i == 2)
                    Thread.Sleep(200);
                else
                    Thread.Sleep(10);

                int c = i;
                Task.Run(() => { TestCore(c); });
            }
            TestCore(100);
        }
        private void TestCore(int i)
        {
            try
            {
                int msg = 1;
                byte[] buffer = new byte[1024];
                var endpoint = new IPEndPoint(IPAddress.IPv6Loopback, _localEndpoint.Port);

                _udpSocket.SendTo(buffer, endpoint);
                Thread.Sleep(500);

                while (true)
                {
                    int size = Encoding.UTF8.GetBytes($"Sender {i,3}, msg {msg++,3}", buffer);
                    //Thread.Sleep(1);
                    _udpSocket.SendTo(
                        buffer,
                        size,
                        SocketFlags.None,
                        endpoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Listen()
        {
            _udpSocket.Bind(_localEndpoint);

            var saea = new SocketAsyncEventArgs()
            {
                SocketFlags = SocketFlags.Peek,
                UserToken = _udpSocket,
                RemoteEndPoint = _localEndpoint
            };
            saea.SetBuffer(new byte[1024]);

            saea.Completed += OnPeekCompleted;

            _udpSocket.ReceiveFromAsync(saea);
        }

        private void OnPeekCompleted(object _, SocketAsyncEventArgs saea)
        {
            var error = saea.SocketError;
            Console.WriteLine($"{DateTime.Now.Second,2} Return: {error}");

            if (error == SocketError.Success)
            {
                Socket clientSocket = GetSocket();

                var csaea = new SocketAsyncEventArgs()
                {
                    UserToken = clientSocket,
                    RemoteEndPoint = saea.RemoteEndPoint
                };
                csaea.SetBuffer(new byte[1024]);

                csaea.Completed += OnConnectCompleted;

                clientSocket.Bind(saea.RemoteEndPoint);
                if (!clientSocket.ConnectAsync(csaea))
                    clientSocket.ReceiveAsync(csaea);

                (saea.UserToken as Socket).ReceiveFromAsync(saea);
            }
        }
        private void OnConnectCompleted(object _, SocketAsyncEventArgs saea)
        {
            Console.WriteLine("Connected: " + saea.RemoteEndPoint);
            saea.Completed -= OnConnectCompleted;
            saea.Completed += OnReceiveCompleted;
            (saea.UserToken as Socket).ReceiveAsync(saea);
        }
        private void OnReceiveCompleted(object _, SocketAsyncEventArgs saea)
        {
            OnMessageReceived(saea);

            Socket socket = saea.UserToken as Socket;

            while (!socket.ReceiveAsync(saea))
                OnMessageReceived(saea);
        }

        long cnt = 0;
        long total = 0;
        private void OnMessageReceived(SocketAsyncEventArgs saea)
        {
            long count = Interlocked.Increment(ref cnt);
            long bytes = Interlocked.Add(ref total, saea.BytesTransferred);
            if (count % 100000 == 0)
            {
                Console.WriteLine($"Messages {count}, {bytes >> 10} KB");
            }
        }
    }
}
