using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        object _lock = new object();
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        int _sessionId = 0;

        public int GetBusyScore()
        {
            int count = 0;

            lock (_lock)
            {
                count = _sessions.Count;
            }
            
            return count / 100;
        }

        public List<ClientSession> GetSessions()
        {
            List<ClientSession> sessions = new List<ClientSession>();

            lock (_lock)
            {
                sessions = _sessions.Values.ToList();
            }

            return sessions;
        }

        public ClientSession Generate()
        {
            lock (_lock)
            {// 원자성! 룸ID가 겹칠일은 없다!
                int sessionId = ++_sessionId;

                ClientSession session = new ClientSession();
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int sessionId)
        {
            lock (_lock)
            {
                ClientSession session = null;
                if (_sessions.TryGetValue(sessionId, out session))
                    return session;
                return null;
            }
        }

        public bool Remove(ClientSession session)
        {
            lock (_lock)
            {
                return _sessions.Remove(session.SessionId);
            }
        }
    }
}

