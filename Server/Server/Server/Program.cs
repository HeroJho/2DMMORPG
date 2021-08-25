using Server.Data;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		
		static void GameLogicTask()
        {
            while (true)
            {
				GameLogic.Instance.Update();
				Thread.Sleep(0);
            }
        }

		static void NetworkTask()
        {
			while(true)
            {
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach (ClientSession session in sessions)
                {
					session.FlushSend();
                }

				Thread.Sleep(0);
            }
        }

		static void DbTask()
        {
			while (true)
			{
				DbTransaction.Instance.Flush();
				Thread.Sleep(0);
			}
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();


			GameLogic.Instance.Add(1);
			GameLogic.Instance.Init();


			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			// DbTask
			{
				Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
			}

			// NetworkTask
			{
				Thread t = new Thread(NetworkTask);
				t.Name = "Network Send";
				t.Start();
			}

			// GameLogicTask
			Thread.CurrentThread.Name = "GameLogic";
			GameLogicTask();
		}
	}
}
