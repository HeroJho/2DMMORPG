﻿using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Summoning : CreatureObject
    {
        //public ConditionInfo ConditionInfo { get; set; } = new ConditionInfo();
        public int Damage { get; set; }
        public int Range { get; set; }
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

            Speed = skillData.projectile.projectilePointInfos[point].speed;
            Damage = skillData.skillPointInfos[point].damage;
            Range = skillData.projectile.projectilePointInfos[point].range;

        }

        public override void Update()
        {

        }
    }
}
