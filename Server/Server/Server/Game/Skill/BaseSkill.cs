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
        public Dictionary<int, int> SkillPoints { get; private set; } = new Dictionary<int, int>();
        Player _player;

        public BaseSkill(Player player)
        {
            _player = player;

            // TEMP
            SkillPoints.Add(1, 1);
            SkillPoints.Add(2, 4);
            SkillPoints.Add(3, 1);
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

            // 스킬 사용 가능 여부 체크
            ObjectInfo info = _player.Info;
            if (info.PosInfo.State != CreatureState.Idle) // 멈춘 상태에서
                return;
            // 마나 여부 확인
            if (_player.Mp < skillData.skillPointInfos[point].mp)
                return;
            // 쿨타임 확인 or 쿨타임 체크
            if (!_player.Skill.StartCheckCooltime(skillData.id, skillData.skillPointInfos[point].cooldown))
            {
                Console.WriteLine($"{_player.PlayerDbId} Tryed to use Skill Skipped!");
                return;
            }

            // 클라 애니메이션 실행
            info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillData.id;
            room.Broadcast(_player.CellPos, skill);

            // 스킬을 사용했으니 Mp 깎음
            _player.UseMp(skillData.skillPointInfos[point].mp);

            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        // 데미지 판정
                        Vector2Int skillPos = _player.GetFrontCellPos(info.PosInfo.MoveDir);
                        GameObject go = room.Map.Find(skillPos);

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
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = _player;

                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = _player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = _player.PosInfo.PosX;
                        arrow.PosInfo.PosY = _player.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.projectilePointInfos[point].speed;
                        arrow.Damage = skillData.skillPointInfos[point].damage;

                        room.EnterGame(arrow);
                    }
                    break;
                case SkillType.SkillExplosion:
                    {
                        HashSet<GameObject> objects = room.Map.LoopByCircle(_player.CellPos, skillData.explosion.explosionPointInfos[point].radian);

                        foreach (GameObject obj in objects)
                        {
                            if (obj as CreatureObject == null)
                                continue;

                            CreatureObject co = (CreatureObject)obj;
                            co.OnDamaged(_player, skillData.skillPointInfos[point].damage);
                        }
                    }
                    break;
            }
        }

    }
}
