using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Summoning : CreatureObject
    {
        public int Damage { get; set; }
        public int Duration { get; set; }
        public int Radian { get; set; }
        protected Skill _skillData = null;
        protected int _skillLevel;

        public Summoning()
        {
            ObjectType = GameObjectType.Summoning;
        }

        public virtual void Init(Skill skillData, int point)
        {
            _skillData = skillData;
            _skillLevel = point;

            Damage = skillData.skillPointInfos[point].damage;
            Duration = Environment.TickCount + skillData.summoning.summoningPointInfos[point].duration * 1000;
            Radian = skillData.summoning.summoningPointInfos[point].radian;
        }

        public override void Update()
        {

        }
    }
}
