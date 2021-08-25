using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Arrow : Projectile
    {
        public CreatureObject Owner { get; set; } // 주인

        public override void Update()
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);

            Vector2Int destPos = GetFrontCellPos(); // 내 앞 좌표
            if (Room.Map.ApplyMove(this, destPos, collision: false))
            {
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePacket);
            }
            else
            {
                GameObject go = Room.Map.Find(destPos);
                if (go as CreatureObject == null)
                    return;

                CreatureObject target = (CreatureObject)go;

                if (target != null)
                {
                    target.OnDamaged(this, Data.damage + Owner.TotalAttack);
                }

                Room.Push(Room.LeaveGame, Id);
            }

        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
