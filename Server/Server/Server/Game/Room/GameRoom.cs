using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class GameRoom : JobSerializer
    {
        public const int VisionCells = 11; // TODO : config로 관리

        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        public Dictionary<int, Player> PlayerList { get { return _players; }}
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, Summoning> _summonings = new Dictionary<int, Summoning>();
        Dictionary<int, Item> _Items = new Dictionary<int, Item>();

        Dictionary<int, Npc> _npc = new Dictionary<int, Npc>();

        SpawnManager _spawnManager;

        public Zone[,] Zones { get; private set; }
        public Zone[,] ItemZones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId, int zoneCells)
        {
            string spawnInfo = Map.LoadMap(mapId);

            // Zone
            
            ZoneCells = zoneCells;
            
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            Zones = new Zone[countY, countX];
            ItemZones = new Zone[countY, countX];

            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                    ItemZones[y, x] = new Zone(y, x);
                }
            }

            // 맵 생성되고 Zone나눴으면 바닥 아이템 뿌림
            // Npc 메모리에 생성 > Player입장 시 패킷 보내서 생성
            LoadInitData();

            _spawnManager = new SpawnManager(spawnInfo, this);
            _spawnManager.Init();
        }

        bool _isEmpty = false;
        IJob _checkEmptyJob = null;
        public void Update()
        {
            Flush();

            CheckEmptyRoom();
        }

        private void CheckEmptyRoom()
        {
            if (Map.MapId == 1)
                return;

            // 던전에 플레이어가 20초 이상 동안 없다면 방 삭제
            if (_players.Count <= 0)
                _isEmpty = true;
            else
                _isEmpty = false;

            if (_checkEmptyJob == null)
            {
                _checkEmptyJob = PushAfter(20000, () =>
                {
                    _checkEmptyJob = null;

                    if (this == null)
                        return;

                    if (_isEmpty == false)
                        return;

                    ChangeRoomAllPlayer();

                    // 방 제거
                    GameLogic.Instance.Push(() =>
                    {
                        GameLogic.Instance.Remove(RoomId);
                        Console.WriteLine("맵 제거!");
                    });
                });
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            switch (type)
            {
                case GameObjectType.Player:
                    {
                        Player player = (Player)gameObject;
                        _players.Add(player.Id, player);
                        player.Room = this;

                        // Npc 스폰
                        SpawnNpc(player);
                        SpawnObstacle(player);
                                             
                        player.RefreshAdditionanlStat();

                        Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                        GetZone(player.CellPos).Players.Add(player);

                        // 내 정보 전송
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.Info;

                        player.Session.Send(enterPacket);

                        player.Vision.AllSpawn();
                        player.Vision.Update();

                        // 파티가 있다면 파티정보 전송
                        if(player.Communication.Party != null)
                            player.Communication.SendPartyInfo();

                    }
                    break;
                case GameObjectType.Monster:
                    {
                        // TODO
                        Monster monster = (Monster)gameObject;
                        _monsters.Add(monster.Id, monster);
                        monster.Room = this;

                        GetZone(monster.CellPos).Monsters.Add(monster);

                        Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

                        monster.Update();
                    }
                    break;
                case GameObjectType.Projectile:
                    {
                        Projectile projectile = (Projectile)gameObject;
                        _projectiles.Add(projectile.Id, projectile);
                        projectile.Room = this;

                        GetZone(projectile.CellPos).Projectiles.Add(projectile);
                        projectile.Update();
                    }
                    break;
                case GameObjectType.Summoning:
                    {
                        Summoning summoning = (Summoning)gameObject;
                        _summonings.Add(summoning.Id, summoning);
                        summoning.Room = this;

                        GetZone(summoning.CellPos).Summonings.Add(summoning);
                        summoning.Update();
                    }
                    break;
                case GameObjectType.Item:
                    {
                        Item item = (Item)gameObject;
                        // 떨어진 아이템이 해당 룸이 아닐땐 패스
                        if (item.RoomId != RoomId)
                            return;

                        _Items.Add(item.Id, item);
                        item.Room = this;

                        GetZone(item.CellPos).Items.Add(item);
                    }                    
                    break;
            }

            // 게임 입장(죽고 다시 생성)할 때 주변에 자신 스폰 알림
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                Broadcast(gameObject.CellPos, spawnPacket);
            }
                        
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            Vector2Int cellPos;

            switch (type)
            {
                case GameObjectType.Player:
                    {
                        Player player = null;
                        if (_players.Remove(objectId, out player) == false)
                            return;

                        cellPos = player.CellPos;
                        GetZone(player.CellPos).Players.Remove(player);

                        player.OnLeaveGame();
                        Map.ApplyLeave(player);
                        player.Room = null;

                        // 본인한테 정보 전송
                        S_LeaveGame leavePacket = new S_LeaveGame();
                        player.Session.Send(leavePacket);
                    }
                    break;
                case GameObjectType.Monster:
                    {
                        Monster monster = null;
                        if (_monsters.Remove(objectId, out monster) == false)
                            return;

                        cellPos = monster.CellPos;
                        GetZone(monster.CellPos).Monsters.Remove(monster);
                        Map.ApplyLeave(monster);
                        monster.Room = null;
                    }
                    break;
                case GameObjectType.Projectile:
                    {
                        Projectile projectile = null;
                        if (_projectiles.Remove(objectId, out projectile) == false)
                            return;

                        cellPos = projectile.CellPos;
                        Map.ApplyLeave(projectile);
                        projectile.Room = null;
                    }
                    break;
                case GameObjectType.Summoning:
                    {
                        Summoning summoning = null;
                        if (_summonings.Remove(objectId, out summoning) == false)
                            return;

                        cellPos = summoning.CellPos;
                        Map.ApplyLeave(summoning);
                        summoning.Room = null;
                    }
                    break;
                case GameObjectType.Item:
                    {
                        Item item = null;
                        if (_Items.Remove(objectId, out item) == false)
                            return;

                        cellPos = item.CellPos;
                        Map.ApplyLeave(item);
                        item.Room = null;
                    }
                    break;
                default:
                    return;

            }

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                Broadcast(cellPos, despawnPacket);
            }
        }

        public void HandleSkillPoint(Player player, C_ChangeSkillPoint changeSkillPointPacket)
        {
            if (player == null || player.Room == null)
                return;
            GameRoom room = player.Room;

            if (!player.Skill.IncreaseSkillLevel(changeSkillPointPacket.SkillInfo.SkillId))
                return;

            // S_SkillPoint 패킷 전송
            S_SkillPoint skillPointPacket = new S_SkillPoint();
            skillPointPacket.Points = player.Skill.SkillPoint;
            skillPointPacket.SkillInfos.Add(new SkillInfo()
            {
                SkillId = changeSkillPointPacket.SkillInfo.SkillId,
                Point = changeSkillPointPacket.SkillInfo.Point + 1
            });

            player.Session.Send(skillPointPacket);
        }

        public void HandleStatPoint(Player player, C_ChangeStatPoint changeStatPointPacket)
        {
            if (player == null || player.Room == null)
                return;
            GameRoom room = player.Room;

            // 포인트 확인
            if (player.Stat.StatPoints <= 0)
                return;

            // 스텟당 오르는 능력치 계수
            switch (changeStatPointPacket.Stat)
            {
                case 1:
                    player.Stat.MaxHp += 10;
                    break;
                case 2:
                    player.Stat.MaxMp += 10;
                    break;
                case 3:
                    player.Stat.Str += 1;
                    break;
                case 4:
                    player.Stat.Int += 1;
                    break;
                default:
                    break;
            }

            // 포인트 감소
            player.Stat.StatPoints--;

            // S_StatPoint 패킷 전송
            player.UpdateClientStat(); 
            player.UpdateHpMpStat();
        }

        public void HandleClassUp(Player player, C_ClassUp classUpPacket)
        {
            if (player == null || player.Room == null)
                return;

            if (player.Stat.CanUpClass == false)
                return;
            // 초보자가 아니라면 return > 이 패킷은 1차 전직때만 사용됨
            if (player.JobClassType != JobClassType.None)
                return;

            // 전직 진행
            switch (classUpPacket.ClassType)
            {
                case JobClassType.Warrior:
                    player.JobClassType = JobClassType.Warrior;
                    break;
                case JobClassType.Hunter:
                    break;
                case JobClassType.Mage:
                    player.JobClassType = JobClassType.Mage;
                    break;
                default:
                    break;
            }

            // 스킬 업글
            player.Skill.ClassSkillUp(classUpPacket.ClassType);

            // 업글 불가
            player.Stat.CanUpClass = false;

            // 직업, 직업업글 여부 갱신
            player.UpdateClientStat();
        }

        public void HandleTryDungun ( Player player, C_TryGetInDungun tryDungunPacket)
        {
            if (player == null || player.Room == null)
                return;

            // 던전 데이터를 뽑는다
            DungunData dungunData = null;
            if (DataManager.DungunDict.TryGetValue(tryDungunPacket.Id, out dungunData) == false)
                return;

            S_TryGetInDungun stryDungunPacket = new S_TryGetInDungun();


            // 파티 정보를 뽑는다
            Party party = player.Communication.Party;
            List<Player> partys = new List<Player>();
            if (player.Communication.Party != null)
            {
                // 리더인지 확인
                if (player != party.LeaderPlayer)
                {
                    player.SendMassage("당신은 파티장이 아닙니다!", false);
                    return;
                }    

                foreach (Player partyer in party.PartyList.Values)
                {
                    if (partyer == null || partyer.Room == null)
                        return;

                    // 파티원중에 레벨이 안되는 사람이 있다 >> 레벨 안됨 메세지 전송
                    if(partyer.Stat.Level < dungunData.limitLevel)
                    {
                        player.SendMassage("입장 조건이 안됩니다!", false);
                        return;
                    }

                    partys.Add(partyer);
                }
            }
            else // 파티가 없다면 파티가 있어야 한다는 메세지 전송
            {
                player.SendMassage("꼭 파티가 있어야 합니다!", false);
                return;
            }

            // 맵을 먼저 만든다
            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Add();
                room.Init(dungunData.mapId, 50);

                // 파티원들을 전부 씬만 전환
                foreach (Player partyer in partys)
                {
                    partyer.Session.TempRoomId = room.RoomId;
                    partyer.IsChangedRoom = true;

                    // 아무이상 없다면 일단
                    // 먼저 방을 나감 >> 씬전환 후 Clear실행 방지
                    partyer.Room.LeaveGame(partyer.Id);

                    stryDungunPacket.Ok = 0;
                    partyer.Session.Send(stryDungunPacket);
                }
            });

            // 씬 전환 되면 GetIn패킷으로 방 변경
            // 씬 전환 되고 Spawn패킷을 실행하기 위함
        }

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdiacentZones(pos);

            foreach (Player p in zones.SelectMany(z => z.Players))
            {
                int dx = p.CellPos.x - pos.x;
                int dy = p.CellPos.y - pos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells)
                    continue;
                if (Math.Abs(dy) > GameRoom.VisionCells)
                    continue;

                p.Session.Send(packet);
            }            
        }

        public Player FindPlayer(Func<Player, bool> condition)
        {
            foreach(Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public Player FindPlayerById(int id)
        {
            Player player = null;

            if (_players.TryGetValue(id, out player) == false)
                return null;

            return player;
        }

        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            if (x < 0 || x >= Zones.GetLength(1))
                return null;
            if (y < 0 || y >= Zones.GetLength(0))
                return null;

            return Zones[y, x];
        }

        public List<Zone> GetAdiacentZones(Vector2Int cellPos, int cells = VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int[] delta = new int[2] { -cells, +cells };
            foreach (int dy in delta)
            {
                foreach (int dx in delta)
                {
                    int y = cellPos.y + dy;
                    int x = cellPos.x + dx;
                    Zone zone = GetZone(new Vector2Int(x, y));

                    // 범위에 벗어나면 zone null반환
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                }
            }

            return zones.ToList();
        }

        public void ChangeRoomAllPlayer()
        {
            S_TryGetInDungun stryDungunPacket = new S_TryGetInDungun();
            List<Player> players = _players.Values.ToList();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Session.TempRoomId = 1;
                players[i].IsChangedRoom = true;

                // 아무이상 없다면 일단
                // 먼저 방을 나감 >> 씬전환 후 Clear실행 방지
                LeaveGame(players[i].Id);

                stryDungunPacket.Ok = 0;
                players[i].Session.Send(stryDungunPacket);
            }
        }

        public void ChangeRoomPlayer(Player player)
        {
            if (_players.ContainsKey(player.Id) == false)
                return;

            S_TryGetInDungun stryDungunPacket = new S_TryGetInDungun();

            player.Session.TempRoomId = 1;
            player.IsChangedRoom = true;

            // 아무이상 없다면 일단
            // 먼저 방을 나감 >> 씬전환 후 Clear실행 방지
            LeaveGame(player.Id);

            stryDungunPacket.Ok = 0;
            player.Session.Send(stryDungunPacket);
        }

        public void LoadInitData()
        {
            // 아이템 필드에 뿌리기
            List<ItemDb> items = null;
            using (AppDbContext db = new AppDbContext())
            {
                items = db.Items
                    .Where(i => i.Owner == null)
                    .Where(i => i.RoomId == RoomId).ToList();

            }

            if (items == null)
                return;

            foreach (ItemDb itemDb in items)
            {
                Item newItem = Item.MakeItem(itemDb);
                newItem.Id = ObjectManager.Instance.GenerateId(newItem.ObjectType);
                Map.DropItemToMap(newItem.CellPos, newItem);

                EnterGame(newItem);
            }



            // NPC 메모리에 넣기
            foreach (NpcData npcData in DataManager.NpcDict.Values)
            {
                // 해당 룸의 Npc가 아니면 패쓰
                if (npcData.mapId != RoomId)
                    continue;

                // 따로 Npc패킷을 만들어서 tampId랑해서 보내기
                Npc npc = new Npc();
                npc.Init(npcData.id);
                npc.Room = this;
                _npc.Add(npcData.id, npc);
                Map.ApplyMove(npc, npc.CellPos, checkObjects: false, collision: true);
            }


        }

        void SpawnNpc(Player player)
        {
            if (player == null || player.Room == null)
                return;

            S_SpawnNpc spawnNpcPacket = new S_SpawnNpc();
            spawnNpcPacket.QuestInfo = new QuestInfo();

            foreach (Npc npc in _npc.Values)
            {
                // 따로 Npc패킷을 만들어서 tampId랑해서 보내기
                spawnNpcPacket.NpcInfos.Add(npc.Info);
            }

            // 퀘스트 동기화 데이터
            foreach (Quest quest in player.Quest.Quests.Values)
            {
                spawnNpcPacket.QuestInfo.Quests.Add(quest.QuestId);
            }
            foreach (Quest quest in player.Quest.CanCompleteQuests.Values)
            {
                spawnNpcPacket.QuestInfo.CanCompleteQuests.Add(quest.QuestId);
            }
            foreach (Quest quest in player.Quest.CompletedQuests.Values)
            {
                spawnNpcPacket.QuestInfo.CompletedQuests.Add(quest.QuestId);
            }

            player.Session.Send(spawnNpcPacket);

            // 접속하고 몇마리 잡았는지 갱신(퀘스트 진행 갱신)
            foreach (Quest quest in player.Quest.Quests.Values)
            {
                player.Quest.RefreshQuest(quest);
            }

        }

        void SpawnObstacle(Player player)
        {
            if (player == null || player.Room == null)
                return;

            S_SpawnObstacle spawnObstaclePacket = new S_SpawnObstacle();
            foreach (int key in player.Obstacle.Obstacles.Keys)
            {
                spawnObstaclePacket.TemplateId.Add(key);
            }

            player.Session.Send(spawnObstaclePacket);
        }

        public void SendAllPlayerMassage(string str, bool isGreen, bool isCount = false)
        {
            foreach (Player player in _players.Values)
                player.SendMassage(str, isGreen, isCount);

        }

    }
}
