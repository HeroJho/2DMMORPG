using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectCompleted;
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnect(args);

                //TEMP
                Thread.Sleep(10);
            }

        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            try
            {
                bool pending = socket.ConnectAsync(args);
                if (pending == false)
                    OnConnectCompleted(null, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        void OnConnectCompleted(object obj, SocketAsyncEventArgs args)
        {

            try
            {
                if (args.SocketError == SocketError.Success)
                {
                    Session session = _sessionFactory.Invoke();
                    session.Start(args.ConnectSocket);
                    session.OnConnected(args.RemoteEndPoint);
                }
                else
                {
                    Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
