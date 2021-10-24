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
            Console.WriteLine("Party List: ");
            foreach (Player player in PartyList.Values)
            {
                // TODO : 파티정보 패킷 전송
                //player.Session.Send();

                Console.WriteLine($"id : {player.Id}");
            }
        }

    }
}
