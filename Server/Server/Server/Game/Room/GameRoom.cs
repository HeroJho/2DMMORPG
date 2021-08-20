﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
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

        public Zone[,] Zones { get; private set; }
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

            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }

            // TEMP
            for (int i = 0; i < 3; i++)
            {
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Init(1);
                EnterGame(monster);
            }
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
            }

            // 타인한테 정보 전송
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

        public Player FindPlayer(Func<GameObject, bool> condition)
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

    }
}