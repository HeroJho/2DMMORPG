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
            SkillPoints.Add(2003, 1);

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

                        HashSet<CreatureObject> objects = _player.Room.Map.LoopByCircle<CreatureObject>(_player.CellPos, skillData.explosion.explosionPointInfos[point].radian);

                        foreach (CreatureObject co in objects)
                            co.OnDamaged(_player, skillData.skillPointInfos[point].damage);

                    }
                    break;
                case SkillType.SkillSummoning:
                    {
                        if (skillData.id != 2003)
                            break;

                        PoisonSmoke poisonSmoke = ObjectManager.Instance.Add<PoisonSmoke>();
                        if (poisonSmoke == null)
                            return;

                        poisonSmoke.Owner = _player;
                        poisonSmoke.Info.TemplateId = skillData.id;

                        poisonSmoke.PosInfo.State = CreatureState.Idle;
                        poisonSmoke.PosInfo.MoveDir = _player.PosInfo.MoveDir;
                        poisonSmoke.PosInfo.PosX = _player.PosInfo.PosX;
                        poisonSmoke.PosInfo.PosY = _player.PosInfo.PosY;
                        poisonSmoke.Stat.Level = point; // 클라 크기조절 용
                        poisonSmoke.Init(skillData, point);

                        // 스킬시전 시간 후에 생성
                        _player.Room.PushAfter(1500 , ()=> 
                        {
                            if (_player == null || _player.Room == null)
                                return;

                            _player.Room.EnterGame(poisonSmoke);
                        });
                    }
                    break;

            }
        }

    }
}
