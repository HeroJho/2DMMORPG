using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Spawner
    {
        private List<Monster> _monsters = new List<Monster>();
        private List<Monster> _deadMonsters = new List<Monster>();

        GameRoom _room;
        Random _rand = new Random();

        private int _monsterId;
        private Vector2Int _spawnPos;
        private int _maxCount;
        private int _genDelay;

        private int _rangth;
        public int MinX { get; private set; }
        public int MaxX { get; private set; }
        public int MinY { get; private set; }
        public int MaxY { get; private set; }


        public Spawner(string monsterId, string x, string y, GameRoom room)
        {
            if (room == null)
                return;

            _room = room;

            _monsterId = int.Parse(monsterId);
            _spawnPos = new Vector2Int(int.Parse(x), int.Parse(y));
            _maxCount = 3;
            _genDelay = 5000;
            _rangth = 4;

            MinX = _spawnPos.x - _rangth;
            MaxX = _spawnPos.x + _rangth;
            MinY = _spawnPos.y - _rangth;
            MaxY = _spawnPos.y + _rangth;

            Init();
        }

        private void Init()
        {
            for (int i = 0; i < _maxCount; i++)
            {
                Vector2Int? randPos = GetPos();
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Init(_monsterId, this, randPos.Value);

                _room.EnterGame(monster);

                _monsters.Add(monster);
            }

        }

        public void Update()
        {
            for (int i = 0; i < _deadMonsters.Count; i++)
            {
                Monster monster = _deadMonsters[i];

                Vector2Int? randPos = GetPos();
                if (randPos == null)
                    continue;

                _deadMonsters.Remove(monster);
                monster.ReSpawn(randPos.Value, _room);
            }

            _room.PushAfter(_genDelay, Update);
        }

        private Vector2Int? GetPos()
        {
            int radX = _rand.Next(MinX, MaxX);
            int radY = _rand.Next(MinY, MaxY);
            Vector2Int? randPos = new Vector2Int(radX, radY);

            int tryCount = 0; // 10번만 시도
            while (!_room.Map.CanGo(randPos.Value, checkObject: true))
            {
                tryCount++;
                radX = _rand.Next(MinX, MaxX);
                radY = _rand.Next(MinY, MaxY);
                randPos = new Vector2Int(radX, radY);

                if (tryCount > 10)
                    return null;
            }

            return randPos;
        }

        public void Dead(Monster monster)
        {
            _deadMonsters.Add(monster);
        }

    }


    public class SpawnManager
    {
        GameRoom _room;
        private List<Spawner> _spawners = new List<Spawner>();

        public SpawnManager(string spawnInfo, GameRoom room)
        {
            if (room == null)
                return;
            _room = room;

            string[] infos = spawnInfo.Split("\r\n");
           
            if(infos.Length <= 0)
                return;

            foreach (string info in infos)
            {
                if (info.Length <= 2)
                    return;

                string[] splitedInfos = info.Split("_");
                Spawner spawner = new Spawner(splitedInfos[0], splitedInfos[1], splitedInfos[2], room);
                _spawners.Add(spawner);
            }

        }

        public void Init()
        {
            if (_spawners.Count <= 0)
                return;

            foreach (Spawner spawner in _spawners)
            {
                spawner.Update();
            }
        }

    }

}
