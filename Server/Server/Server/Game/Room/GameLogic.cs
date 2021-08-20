using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();

        object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            foreach(GameRoom room in _rooms.Values)
            {
                room.Update();
            }
        }

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapId, 10);


            // 원자성! 룸ID가 겹칠일은 없다!
            gameRoom.RoomId = _roomId;
            _rooms.Add(_roomId, gameRoom);
            _roomId++;


            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            return _rooms.Remove(roomId);            
        }

        public GameRoom Find(int roomId)
        {
            GameRoom room = null;
            if (_rooms.TryGetValue(roomId, out room))
                return room;
            return null;
        }
    }
}
