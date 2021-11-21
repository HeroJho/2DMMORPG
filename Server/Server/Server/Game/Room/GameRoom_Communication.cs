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

        public void HandleInvitePlayerToParty(Player player, C_InvitePlayer invitePlayerPacket)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            Player playerInvited = room.FindPlayerById(invitePlayerPacket.PlayerId);

            // 초대한 플레이어가 맵에 없다?
            if (playerInvited == null)
                return;

            player.Communication.InvitePlayerToParty(playerInvited, player);
        }

        public void HandleQuitParty(Player player, C_QuitParty quitPartyPacket)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            // 자기 자신인가? >> 탈퇴
            if(quitPartyPacket.Id == player.Id)
            {
                player.Communication.RemoveParty();
                return;
            }

            // 파티장 인가? >> 강퇴
            if(player.Communication.KickParty(quitPartyPacket.Id) == false)
                player.SendMassage("당신은 파티장이 아닙니다!", false);

        }

        public void HandleChat(Player player, C_Chat chatPacket)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            string str = chatPacket.Str;
            if (str.Length <= 0)
                return;

            S_Chat schatPacket = new S_Chat();
            schatPacket.Id = player.Id;
            schatPacket.Str = str;
            room.Broadcast(player.CellPos, schatPacket);
        }

    }
}
