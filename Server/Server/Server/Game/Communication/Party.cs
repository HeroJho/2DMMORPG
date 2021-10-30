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

            return true;
        }
        
        public void RemovePlayer(int playerId)
        {

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
