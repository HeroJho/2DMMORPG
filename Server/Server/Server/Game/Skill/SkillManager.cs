﻿using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class SkillManager
    {
        public int SkillDbId { get; set; }
        private Player _player;
        private SkillCoolTimeManager _coolTimeManager;
        public BaseSkill SkillTree { get; private set; }
        public int SkillPoint { get; set; }

        public SkillManager(Player player)
        {
            _player = player;
            _coolTimeManager = new SkillCoolTimeManager(player);

            switch (player.JobClassType)
            {
                case JobClassType.None:
                    SkillTree = new BaseSkill(player);
                    break;
                case JobClassType.Warrior:
                    break;
                case JobClassType.Hunter:
                    break;
                case JobClassType.Mage:
                    break;
                default:
                    break;
            }
        }

        public void UseSkill(int skillId)
        {
            if (_player == null || _player.Room == null)
                return;

            SkillTree.UseSkill(skillId);
        }

        public bool IncreaseSkillLevel(int templateId)
        {
            // 스킬 포인트가 있는지
            if (SkillPoint <= 0)
                return false;

            // 스킬 업
            if (!SkillTree.IncreaseSkillLevel(templateId))
                return false;

            // 성공했으니 포인트 --
            SkillPoint--;
            
            return true;
        }

        public void GetSkillPoints(int points)
        {
            SkillPoint += points;

            // S_SkillPoint 패킷 전송
            S_SkillPoint skillPointPacket = new S_SkillPoint();
            skillPointPacket.Points = SkillPoint;
            _player.Session.Send(skillPointPacket);
        }

        public bool StartCheckCooltime(int templateId, float cooldown)
        {
            return _coolTimeManager.StartCheckCooltime(templateId, cooldown);
        }     

    }
}
