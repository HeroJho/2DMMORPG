using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // 서버에서 좌표 이동
            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            // 다른 좌표로 이동 가능한지 체크
            // 상태만 바껴도 패킷을 보내기 때문에 한번 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }

            // 이동 가능하다. 상태만 변경
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;

            // _map에 좌표를 갱신
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            // 다른 플레이어한테도 알려준다
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            Broadcast(player.CellPos, resMovePacket);

        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;

            // 스킬 사용 가능 여부 체크
            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle) // 멈춘 상태에서
                return;

            // 마나 여부 확인
            if (player.Mp < skillData.mp)
                return;

            // 클라 애니메이션 실행
            info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillData.id;
            Broadcast(player.CellPos, skill);

            // 쿨타임 확인 or 쿨타임 체크
            if (!player.Skill.StartCheckCooltime(skillData.id))
            {
                Console.WriteLine("FailedSkill");
                return;
            }


            // 스킬을 사용했으니 Mp 깎음
            player.UseMp(skillData.mp);

            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        // 데미지 판정
                        Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                        GameObject go = Map.Find(skillPos);

                        if (go as CreatureObject == null)
                            return;

                        CreatureObject target = (CreatureObject)go;

                        if (target != null)
                        {
                            target.OnDamaged(player, skillData.damage + player.TotalAttack);
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;

                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = player.PosInfo.PosX;
                        arrow.PosInfo.PosY = player.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;

                        EnterGame(arrow);
                    }
                    break;
                case SkillType.SkillExplosion:
                    {
                        HashSet<GameObject> objects = Map.LoopByCircle(player.CellPos, skillData.explosion.radian);

                        foreach (GameObject obj in objects)
                        {
                            if (obj as CreatureObject == null)
                                continue;

                            CreatureObject co = (CreatureObject)obj;
                            co.OnDamaged(player, skillData.damage);
                        }
                    }
                    break;
            }

        }

        public void HandleRespawn(Player player)
        {
            if (player == null || player.Room == null)
                return;

            player.Respawn();
        }
    }
}
