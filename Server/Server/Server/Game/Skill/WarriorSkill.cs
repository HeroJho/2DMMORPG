﻿using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class WarriorSkill : BaseSkill
    {

        public WarriorSkill(Player player) : base(player)
        {

        }

        public override void FirstAddSkill()
        {
            // 전직할 시 추가되는 스킬 정보
            //SkillPoints.Add(2001, 1);

            S_SkillPoint skillPointPacket = new S_SkillPoint();

            skillPointPacket.Points = _player.Skill.SkillPoint;
            foreach (int key in _player.Skill.SkillTree.SkillPoints.Keys)
            {
                SkillInfo skillInfo = new SkillInfo();
                skillInfo.SkillId = key;
                skillInfo.Point = _player.Skill.SkillTree.SkillPoints[key];
                skillPointPacket.SkillInfos.Add(skillInfo);
            }

            _player.Session.Send(skillPointPacket);
        }

        public override void SkillInfo(Skill skillData, int point)
        {
            base.SkillInfo(skillData, point);

            switch (skillData.skillType)
            {
                


            }
        }

    }
}