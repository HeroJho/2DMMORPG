using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        // [UNUSED(1)][TYPE(7)][ID(24)]
        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.Id, gameObject as Player);
                }
            }

            return gameObject;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24 | (_counter++));
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if(objectType == GameObjectType.Player)
                    return _players.Remove(objectId);
            }

            return false;
        }

        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if(objectType == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.TryGetValue(objectId, out player))
                        return player;
                }

                return null;
            }
        }
    }
}

