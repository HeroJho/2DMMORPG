using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Communication
    {
        public Party Party { get; set; } = null;
        private Player _player = null;

        public Communication(Player player)
        {
            _player = player;
        }

        // 파티를 처음 건 유저가 실행
        public void MakeParty()
        {
            if(Party != null)
            {
                // TODO : 이미 가입한 파티가 있다 패킷 전달
                return;
            }

            // 내가 만들었으니 파티장은 나
            Party = new Party(_player);
            Party.AddPlayer(_player);
        }

        public void RemoveParty()
        {
            if (Party == null)
                return;

            Party.RemovePlayer(_player.Id);
        }

        public bool KickParty(int id)
        {
            if (Party == null)
                return false;
            if (_player.Id != Party.LeaderPlayer.Id)
                return false;

            Party.RemovePlayer(id);
            return true;
        }

        public void InvitePlayerToParty(Player player)
        {
            // 파티가 없다면 만들어서 초대
            if (Party == null)
                MakeParty();

            // 파티장이 아니라면 초대 불가능
            if (Party.LeaderPlayer != _player)
            {
                Console.WriteLine("You're not Leader!");
                return;
            }
            // 이미 가입한 파티가 있다
            if (player.Communication.Party != null)
            {
                Console.WriteLine("this Player aleady have a party!");
                return;
            }

            // 4인 이상이다
            if (Party.PartyList.Count >= 4)
            {
                Console.WriteLine("Pull Party!");
                return;
            }

            // 이미 가입한 플레이어다
            if (!Party.AddPlayer(player))
            {
                Console.WriteLine("This Player is already in Party!");
                return;
            }
                
            // 가입한 플레이어의 Party를 지금 Party로 저장
            player.Communication.Party = Party;
        }

        public void SendPartyInfo()
        {
            // 파티가 없다면 보내지 않음
            if (Party == null)
                return;

            Party.SendPartyInfo();
        }

       
        public void ResetCommunication()
        {
            // 종료하면 파티 탈퇴
            RemoveParty();
        }
    }
}
