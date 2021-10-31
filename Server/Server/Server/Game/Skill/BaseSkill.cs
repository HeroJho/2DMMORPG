using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class BaseSkill
    {
        // 스킬 ID, Point
        public Dictionary<int, int> SkillPoints { get; set; } = new Dictionary<int, int>();
        protected Player _player;

        public BaseSkill(Player player)
        {
            _player = player;

        }

        public virtual void FirstAddSkill()
        {

        }

        public virtual void UseSkill(int skillId)
        {
            GameRoom room = _player.Room;
            if (_player == null || room == null)
                return;

            Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillId, out skillData) == false)
                return;
            int point = 0;
            if (SkillPoints.TryGetValue(skillId, out point) == false)
                return;
            if (point == 0)
                return;
            point -= 1;

            // 마나 여부 확인
            if (_player.Mp < skillData.skillPointInfos[point].mp)
            {// 부족하다면 클라쪽 쿨타임에서 빼줌
                S_ManageSkill manageSkillPacket = new S_ManageSkill();
                manageSkillPacket.TemplateId = skillId;
                _player.Session.Send(manageSkillPacket);
                return;
            }
            // 쿨타임 확인 or 쿨타임 체크
            if (!_player.Skill.StartCheckCooltime(skillData.id, skillData.skillPointInfos[point].cooldown))
            {
                Console.WriteLine($"{_player.PlayerDbId} Tryed to use Skill Skipped!");
                return;
            }

            // 클라 애니메이션 실행
            _player.Info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = _player.Info.ObjectId;
            skill.Info.SkillId = skillData.id;
            skill.Info.Point = point;
            room.Broadcast(_player.CellPos, skill);

            // 스킬을 사용했으니 Mp 깎음
            _player.UseMp(skillData.skillPointInfos[point].mp);

            // 스킬 상세 구현
            SkillInfo(skillData, point);
        }

        public virtual void SkillInfo(Skill skillData, int point)
        {
            if (_player == null || _player.Room == null)
                return;

            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        if (skillData.id != 1001)
                            break;

                        // 데미지 판정
                        Vector2Int skillPos = _player.GetFrontCellPos(_player.Info.PosInfo.MoveDir);
                        GameObject go = _player.Room.Map.Find(skillPos);

                        if (go as CreatureObject == null)
                            return;

                        CreatureObject target = (CreatureObject)go;

                        if (target != null)
                        {
                            target.OnDamaged(_player, skillData.skillPointInfos[point].damage + _player.TotalAttack);
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        if (skillData.id != 1002)
                            break;

                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = _player;
                        arrow.Info.TemplateId = skillData.id;

                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = _player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = _player.PosInfo.PosX;
                        arrow.PosInfo.PosY = _player.PosInfo.PosY;
                        arrow.Init(skillData, point);

                        _player.Room.EnterGame(arrow);
                    }
                    break;

            }
        }

        public virtual bool IncreaseSkillLevel(int templateId)
        {
            // 해당 스킬이 있는 직업 인지
            int level = 0;
            if (!SkillPoints.TryGetValue(templateId, out level))
                return false;

            // 스킬 MaxLevel인지
            Skill skillData = null;
            if (!DataManager.SkillDict.TryGetValue(templateId, out skillData))
                return false;
            if (level >= skillData.skillPointInfos.Count)
                return false;

            // 스킬Level 상승
            SkillPoints[templateId] = level + 1;
            return true;
        }

        public void UseBuff(Skill skillData, int point)
        {
            if (_player == null || _player.Room == null)
                return;

            // 공유가능한 버프이다
            if (_player.Communication.Party != null && skillData.buff.canShare)
            {
                foreach (Player party in _player.Communication.Party.PartyList.Values)
                {
                    if (party == null || party.Room == null)
                        return;

                    switch (skillData.id)
                    {
                        case 2006:
                            party.Condition.MagicGuard(skillData, point);
                            break;
                        case 2007:
                            party.Condition.HyperBody(skillData, point);
                            break;
                        case 2008:
                            party.Condition.IronBody(skillData, point);
                            break;
                        default:
                            break;
                    }

                }
            }
            else
            {
                switch (skillData.id)
                {
                    case 2006:
                        _player.Condition.MagicGuard(skillData, point);
                        break;
                    case 2007:
                        _player.Condition.HyperBody(skillData, point);
                        break;
                    case 2008:
                        _player.Condition.IronBody(skillData, point);
                        break;
                    default:
                        break;
                }
            }

        }

    }
}
