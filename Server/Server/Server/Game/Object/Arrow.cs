using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Arrow : Projectile
    {
        public CreatureObject Owner { get; set; } // 주인

        IJob _job = null;
        int _rangeCount = 1;
        public override void Update()
        {
            if (Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Speed);
            _job = Room.PushAfter(tick, Update);
                        
            if (_rangeCount > Range)
            {
                _job.Cancel = true;
                Room.Push(Room.LeaveGame, Id);
                return;
            }
            _rangeCount++;

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
                if (go as CreatureObject != null)
                {
                    CreatureObject target = (CreatureObject)go;

                    if (target != null)
                    {
                        target.OnDamaged(this, Damage + Owner.TotalAttack);
                        target.Condition.SlowSpeed(2, 5);
                    }
                }

                _job.Cancel = true;
                Room.Push(Room.LeaveGame, Id);
            }

        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
