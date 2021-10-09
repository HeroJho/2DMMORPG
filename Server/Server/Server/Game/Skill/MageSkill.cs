using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class MageSkill : BaseSkill
    {
        
        public MageSkill(Player player) : base(player)
        {

        }

        public override void FirstAddSkill()
        {
            // 전직할 시 추가되는 스킬 정보
            SkillPoints.Add(2001, 1);
            SkillPoints.Add(2002, 1);

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
                case SkillType.SkillProjectile:
                    {
                        if (skillData.id != 2001)
                            break;

                        IceBall iceBall = ObjectManager.Instance.Add<IceBall>();
                        if (iceBall == null)
                            return;

                        iceBall.Owner = _player;
                        iceBall.Info.TemplateId = skillData.id;

                        iceBall.PosInfo.State = CreatureState.Moving;
                        iceBall.PosInfo.MoveDir = _player.PosInfo.MoveDir;
                        iceBall.PosInfo.PosX = _player.PosInfo.PosX;
                        iceBall.PosInfo.PosY = _player.PosInfo.PosY;
                        iceBall.Init(skillData, point);

                        _player.Room.EnterGame(iceBall);
                    }
                    break;
                case SkillType.SkillExplosion:
                    {
                        if (skillData.id != 2002)
                            break;

                        HashSet<GameObject> objects = _player.Room.Map.LoopByCircle(_player.CellPos, skillData.explosion.explosionPointInfos[point].radian);

                        foreach (GameObject obj in objects)
                        {
                            if (obj as CreatureObject == null)
                                continue;

                            CreatureObject co = (CreatureObject)obj;
                            co.OnDamaged(_player, skillData.skillPointInfos[point].damage);
                        }
                    }
                    break;
                case SkillType.SkillSummoning:
                    {
                        if (skillData.id != 2003)
                            break;

                        IceBall iceBall = ObjectManager.Instance.Add<IceBall>();
                        if (iceBall == null)
                            return;

                        iceBall.Owner = _player;
                        iceBall.Info.TemplateId = skillData.id;

                        iceBall.PosInfo.State = CreatureState.Moving;
                        iceBall.PosInfo.MoveDir = _player.PosInfo.MoveDir;
                        iceBall.PosInfo.PosX = _player.PosInfo.PosX;
                        iceBall.PosInfo.PosY = _player.PosInfo.PosY;
                        iceBall.Init(skillData, point);

                        _player.Room.EnterGame(iceBall);
                    }
                    break;

            }
        }

    }
}
