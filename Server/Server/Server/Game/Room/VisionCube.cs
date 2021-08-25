using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        // 내 시야각의 오브젝트를 반환하는 함수
        public HashSet<GameObject> GatherObjects()
        {
            if (Owner == null || Owner.Room == null)
                return null; // JobSerial 방식으로 언제든 null 가능!

            HashSet<GameObject> objects = new HashSet<GameObject>();

            Vector2Int cellPos = Owner.CellPos; // 현재위치

            // 내 시야각의 존들을 가져온다 > 1차적으로 걸러짐
            List<Zone> zones = Owner.Room.GetAdiacentZones(cellPos);

            // 시야각의 오브젝트만 가져옴 > 2차적으로 걸러짐
            foreach (Zone zone in zones)
            {
                foreach (Player player in zone.Players)
                {
                    int dx = player.CellPos.x - cellPos.x;
                    int dy = player.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(player);
                }
                foreach (Monster monster in zone.Monsters)
                {
                    int dx = monster.CellPos.x - cellPos.x;
                    int dy = monster.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(monster);
                }
                foreach (Projectile projectile in zone.Projectiles)
                {
                    int dx = projectile.CellPos.x - cellPos.x;
                    int dy = projectile.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(projectile);
                }
                foreach (Item item in zone.Items)
                {
                    int dx = item.CellPos.x - cellPos.x;
                    int dy = item.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(item);
                }
            }

            return objects;
        }

        public void AllSpawn()
        {
            HashSet<GameObject> spawnObject = GatherObjects();

            S_Spawn spawnPacket = new S_Spawn();

            foreach (GameObject gameObject in spawnObject)
            {
                // 패킷 복사 이유
                ObjectInfo info = new ObjectInfo();
                info.MergeFrom(gameObject.Info);
                spawnPacket.Objects.Add(info);
            }

            Owner.Session.Send(spawnPacket);
        }

        public IJob job = null;
        public void Update()
        {
            if (Owner == null || Owner.Room == null)
                return;

            HashSet<GameObject> currentObjects = GatherObjects();

            // 기존엔 없었는데 새로 생긴 애들 Spawn 처리
            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if (added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();

                foreach (GameObject gameObject in added)
                {
                    // 패킷 복사 이유
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(gameObject.Info);
                    spawnPacket.Objects.Add(info);
                }

                Owner.Session.Send(spawnPacket);
            }
            // 기존엔 있었는데 사라진 애들 Despawn 처리
            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if (removed.Count > 0)
            {
                S_Despawn despawnPacket = new S_Despawn();

                foreach (GameObject gameObject in removed)
                {
                    despawnPacket.ObjectIds.Add(gameObject.Id);
                }

                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;

            job =  Owner.Room.PushAfter(100, Update);
        }
    }
}
