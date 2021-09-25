﻿using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class SkillManager
    {
        private SkillCoolTimeManager _coolTimeManager;
        private Player _player;
        public BaseSkill SkillTree { get; private set; }
        public int SkillPoint { get; set; }

        public SkillManager(Player player)
        {
            _player = player;
            _coolTimeManager = new SkillCoolTimeManager(player);

            // TEMP
            SkillPoint = 10;

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


        public bool StartCheckCooltime(int templateId, float cooldown)
        {
            return _coolTimeManager.StartCheckCooltime(templateId, cooldown);
        }

    }
}