using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Party
    {
        //  ID, Player
        public Player LeaderPlayer { get; set; }
        public Dictionary<int, Player> PartyList = new Dictionary<int, Player>();

        public Party(Player player)
        {
            LeaderPlayer = player;
        }

        public bool AddPlayer(Player player)
        {
            // 이미 가입한 플레이어냐
            if (PartyList.ContainsKey(player.Id))
                return false;

            PartyList.Add(player.Id, player);
            SendPartyInfo();
            return true;
        }

        public Player FindPlayerById(int id)
        {
            Player player = null;
            if (PartyList.TryGetValue(id, out player) == false)
                return null;

            return player;
        }
        
        public void RemovePlayer(int playerId)
        {
            Player tempPlayer = null;
            if (PartyList.Remove(playerId, out tempPlayer) == false)
                return;

            tempPlayer.Communication.Party = null;

            // 만약 파티장 이였다면 다음 사람으로 바꿔줌
            if (LeaderPlayer.Id == playerId)
            {
                foreach (Player player in PartyList.Values)
                    LeaderPlayer = player;
            }

            // 파티에서 삭제가 됐다면 어쨋든 파티원 전부 사라지는 패킷 전송
            S_PartyList partyListPacket = new S_PartyList();
            tempPlayer.Session.Send(partyListPacket);
            SendPartyInfo();
        }

        public void SendPartyInfo()
        {
            if (PartyList.Count <= 0)
                return;

            S_PartyList partyListPacket = new S_PartyList();
            partyListPacket.LeaderPlayer = new ObjectInfo(LeaderPlayer.Info);
            // 버퍼에 따른 맥스치를 보내줘야 함 > 해당 버프를 사용시 이 함수 실행
            partyListPacket.LeaderPlayer.StatInfo.MaxHp = LeaderPlayer.TotalMaxHp;
            partyListPacket.LeaderPlayer.StatInfo.MaxMp = LeaderPlayer.TotalMaxMp;
            foreach (Player player in PartyList.Values)
            {
                ObjectInfo obj = new ObjectInfo(player.Info);
                obj.StatInfo.MaxHp = player.TotalMaxHp;
                obj.StatInfo.MaxMp = player.TotalMaxMp;
                partyListPacket.PlayerInfos.Add(obj);
            }

            foreach (Player player in PartyList.Values)
            {
                player.Session.Send(partyListPacket);
            }
        }

    }
}
