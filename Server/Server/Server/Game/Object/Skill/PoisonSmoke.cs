using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class PoisonSmoke : Projectile
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
                        

        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
