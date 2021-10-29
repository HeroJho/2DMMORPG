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

            Console.WriteLine("Party List: ");
            S_PartyList partyListPacket = new S_PartyList();
            foreach (Player player in PartyList.Values)
            {
                partyListPacket.PlayerInfos.Add(player.Info);
            }

            foreach (Player player in PartyList.Values)
            {
                player.Session.Send(partyListPacket);
            }
        }

    }
}
