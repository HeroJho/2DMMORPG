﻿using Google.Protobuf;
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
        public const int VisionCells = 5; // TODO : config로 관리

        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, Item> _Items = new Dictionary<int, Item>();

        Dictionary<int, Npc> _npc = new Dictionary<int, Npc>();

        public Zone[,] Zones { get; private set; }
        public Zone[,] ItemZones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId, int zoneCells)
        {
            Map.LoadMap(mapId);

            // Zone
            ZoneCells = zoneCells; // 10
            
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

            // TEMP
            for (int i = 0; i < 3; i++)
            {
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Init(1);
                EnterGame(monster);
            }

            // 맵 생성되고 Zone나눴으면 바닥 아이템 뿌림
            // Npc 메모리에 생성 > Player입장 시 패킷 보내서 생성
            LoadInitData();
        }

        public void Update()
        {
            Flush();            
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
                                             
                        player.RefreshAdditionanlStat();

                        Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                        GetZone(player.CellPos).Players.Add(player);

                        // 내 정보 전송
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.Info;
                        player.Session.Send(enterPacket);

                        player.Vision.AllSpawn();
                        player.Vision.Update();
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
                case GameObjectType.Item:
                    {
                        Item item = (Item)gameObject;
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

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdiacentZones(pos);

            foreach(Player p in zones.SelectMany(z => z.Players))
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

                    zones.Add(zone);
                }
            }

            return zones.ToList();
        }

        public void LoadInitData()
        {
            // 아이템 필드에 뿌리기
            List<ItemDb> items = null;
            using (AppDbContext db = new AppDbContext())
            {
                items = db.Items
                    .Where(i => i.Owner == null).ToList();
                
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

           
        }


        public void HandleAcceptQuest(Player player, C_AddQuest questPacket)
        {
            if (player == null && player.Room == null)
                return;

            // 받을 수 있는 퀘스트 인가?
            // 퀘스트 조건 확인 하기
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(questPacket.QuestId, out questData);
            if (questData == null)
                return;
            // 클리어한 퀘스트이냐
            Quest clearQuest = null;
            if (player.Quest.CompletedQuests.TryGetValue(questPacket.QuestId, out clearQuest) == true)
                return;
            // 퀘스트 레벨이 되냐
            if (player.Stat.Level < questData.condition.level)
                return;
            // 클리어 퀘스트를 충족하냐
            foreach (int questId in questData.condition.completeQuests)
            {
                Quest clear = null;
                if (player.Quest.CompletedQuests.TryGetValue(questId, out clear) == false)
                    return;
            }


            Quest quest = Quest.MakeQuest(questPacket.QuestId);

            player.Quest.AcceptQuest(quest);

            // 클라 통보
            S_AddQuest addQuestPacket = new S_AddQuest();
            addQuestPacket.QuestId = questPacket.QuestId;
            addQuestPacket.NpcId = questData.npcId;
            player.Session.Send(addQuestPacket);
        }

        public void HandleTryCompleteQuest(Player player, C_TryCompleteQuest questPacket)
        {
            if (player == null && player.Room == null)
                return;

            if (!player.Quest.CompleteQuest(questPacket.QuestId))
                return;

            // 클라 통보
            S_CompleteQuest completeQuestPacket = new S_CompleteQuest();
            completeQuestPacket.QuestId = questPacket.QuestId;
            completeQuestPacket.NpcId = questPacket.NpcId;
            player.Session.Send(completeQuestPacket);
        }

    }
}
