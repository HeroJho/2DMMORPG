using Server.Data;
using Server.DB;
using ServerCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
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

		static void StartServerInfoTask()
        {
			var t = new System.Timers.Timer();
			t.AutoReset = true; // 반복 
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
			{
				using(SharedDbContext shared = new SharedDbContext())
                {
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if(serverDb != null)
                    {
						serverDb.IpAddress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangeEx();
                    }
                    else
                    {
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							IpAddress = Program.IpAddress,
							Port = Program.Port,
							BusyScore = SessionManager.Instance.GetBusyScore()
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangeEx();
                    }
                }

			});
			t.Interval = 10 * 1000; // 10초마다 한번
			t.Start();
        }

		// TODO : Configure에서 관리하기
		public static string Name { get; } = "스카니아";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameLogic.Instance.Add();
			GameLogic.Instance.Init();

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			StartServerInfoTask();

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
