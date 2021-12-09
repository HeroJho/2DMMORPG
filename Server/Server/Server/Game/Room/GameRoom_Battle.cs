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

            // 상태확인
            if (player.State == CreatureState.Dead &&
                player.State == CreatureState.Skill &&
                player.State == CreatureState.Stun &&
                player.State == CreatureState.Cutscene)
                return;

            // 스킬 방향전환을 위해 방향은 바뀌면 항상 변경
            if (info.PosInfo.MoveDir != movePosInfo.MoveDir)
                info.PosInfo.MoveDir = movePosInfo.MoveDir;

            // 상태만 바껴도 패킷을 보내기 때문에 한번 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {

                // 한턴에 이동거리가 2이상이다 > 핵 사용으로 간주 > 다시 원래자리로 콜빽
                Vector2Int destPos = new Vector2Int(movePosInfo.PosX, movePosInfo.PosY);
                int dist = (destPos - player.CellPos).cellDistFromZero;
                if (dist > 1)
                {
                    // 다른 플레이어한테도 알려준다
                    S_Move backMovePacket = new S_Move();
                    backMovePacket.ObjectId = player.Info.ObjectId;
                    backMovePacket.PosInfo = player.PosInfo;
                    backMovePacket.IncludingMe = true;
                    info.PosInfo.State = movePosInfo.State;
                    info.PosInfo.MoveDir = movePosInfo.MoveDir;

                    Broadcast(player.CellPos, backMovePacket);
                    return;
                }

                // 다른 좌표로 이동 가능한지 체크
                if (Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }

            // 이동 가능하다. 상태만 변경
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;


            // 다른 플레이어한테도 알려준다
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;
            resMovePacket.IncludingMe = false;

            Broadcast(player.CellPos, resMovePacket);

        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null || player.Room == null)
                return;

            // 스킬 사용 가능 여부 체크
            if (player.State != CreatureState.Idle) // 멈춘 상태에서
                return;

            player.Skill.UseSkill(skillPacket.Info.SkillId);

        }

        public void HandleRespawn(Player player)
        {
            if (player == null || player.Room == null)
                return;

            player.Respawn();
        }
    }
}
