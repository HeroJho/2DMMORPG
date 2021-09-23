using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class SkillCoolTimeManager
    {
        Dictionary<int, Skill> _coolTimeList = new Dictionary<int, Skill>();
        private Player _player;

        public SkillCoolTimeManager(Player player)
        {
            _player = player;
        }

        public bool StartCheckCooltime(int templateId, float cooldown)
        {
            if (_player == null || _player.Room == null)
                return false;

            Skill skillData = null;
            DataManager.SkillDict.TryGetValue(templateId, out skillData);

            if (_coolTimeList.ContainsKey(templateId))
                return false;

            _coolTimeList.Add(templateId, skillData);

            _player.Room.PushAfter((int)(cooldown * 1000), ResetCooltime, templateId);
            return true;
        }

        public void ResetCooltime(int templateId)
        {
            if (_player == null || _player.Room == null)
                return;

            _coolTimeList.Remove(templateId);

            S_ManageSkill manageSkillPacket = new S_ManageSkill();
            manageSkillPacket.TemplateId = templateId;
            _player.Session.Send(manageSkillPacket);
        }
    }
}
