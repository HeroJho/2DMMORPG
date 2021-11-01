using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class HealZone : Summoning
    {
        public CreatureObject Owner { get; set; } // 주인

        IJob _job = null;
        public override void Update()
        {
            if (Owner == null || Room == null)
                return;

            // 0.5초마다 체크
            int tick = (int)(1000 / 500);
            _job = Room.PushAfter(tick, Update);


            if (Duration > Environment.TickCount) // 지속 시간일 때
            {
                HashSet<Player> objects = Room.Map.LoopByCircle<Player>(CellPos, Radian, true);
               
                foreach (Player co in objects)
                {
                    if((Owner as Player).Communication.Party != null && (Owner as Player).Communication.Party.FindPlayerById(co.Id) != null)
                        co.Condition.Healing(_skillData, _skillLevel, Owner);
                }
            }
            else // 지속시간이 끝낫을 때
            {
                _job.Cancel = true;
                Room.Push(Room.LeaveGame, Id);
                return;
            }
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
