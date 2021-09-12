using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class SkillBase
    {
        private bool _canUseSKill_01 = true;
        private bool _canUseSKill_02 = true;
        private bool _canUseSKill_03 = true;
        private Player _player;

        public SkillBase(Player player)
        {
            _player = player;
        }

        public bool StartCheckCooltime(int templateId)
        {
            Skill skillData = null;
            DataManager.SkillDict.TryGetValue(templateId, out skillData);

            float cooldown = skillData.cooldown;

            switch (templateId)
            {
                case 1:
                    {
                        if (!_canUseSKill_01)
                            return false;

                        _canUseSKill_01 = false;
                    }
                    break;
                case 2:
                    {
                        if (!_canUseSKill_02)
                            return false;

                        _canUseSKill_02 = false;
                    }
                    break;
                case 3:
                    {
                        if (!_canUseSKill_03)
                            return false;

                        _canUseSKill_03 = false;                        
                    }
                    break;
                
            }

            _player.Room.PushAfter((int)(cooldown * 1000), ResetCooltime, templateId);
            return true;
        }

        public void ResetCooltime(int templateId)
        {
            switch (templateId)
            {
                case 1:
                    {
                        _canUseSKill_01 = true;
                    }
                    break;
                case 2:
                    {
                        _canUseSKill_02 = true;
                    }
                    break;
                case 3:
                    {
                        _canUseSKill_03 = true;
                    }
                    break;

            }

            S_ManageSkill manageSkillPacket = new S_ManageSkill();
            manageSkillPacket.TemplateId = templateId;
            _player.Session.Send(manageSkillPacket);
        }
    }
}
