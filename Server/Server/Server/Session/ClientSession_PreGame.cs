using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                // AccountName을 UniqueId로 사용
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

                if (findAccount != null)
                {
                    // AccountDbId 메모리에 기억
                    AccountDbId = findAccount.AccountDbId;

                    S_Login loginOkPacket = new S_Login() { LoginOk = 1 };
                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name = playerDb.PlayerName,
                            StatInfo = new StatInfo()
                            {                               
                                Level = playerDb.Level,
                                TotalExp = playerDb.TotalExp,
                                Hp = playerDb.Hp,
                                MaxHp = playerDb.MaxHp,
                                Mp = playerDb.Mp,
                                MaxMp = playerDb.MaxMp,
                                Attack = playerDb.Attack,
                                Speed = playerDb.Speed
                            }
                        };

                        // 메모리에 들고 있는다
                        LobbyPlayers.Add(lobbyPlayer);

                        // 패킷에 넣어준다
                        loginOkPacket.Players.Add(lobbyPlayer);
                    }

                    Send(loginOkPacket);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return; // TODO

                    // AccountDbId 메모리에 기억
                    AccountDbId = newAccount.AccountDbId;

                    S_Login loginOkPacket = new S_Login() { LoginOk = 1};
                    Send(loginOkPacket);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterPacket)
        {
            // TODO : 이런 저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterPacket.Name);
            if (playerInfo == null)
                return;

            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;

                S_ItemList itemListPacket = new S_ItemList();

                using (AppDbContext db = new AppDbContext())
                {
                    List<ItemDb> items = db.Items
                        .Where(i => i.OwnerDbId == MyPlayer.PlayerDbId)
                        .ToList();

                    foreach(ItemDb itemDb in items)
                    {
                        Item item = Item.MakeItem(itemDb);
                        if (item != null)
                        {
                            MyPlayer.Inven.Add(item);

                            ItemInfo info = new ItemInfo();
                            info.MergeFrom(item.Info);
                            itemListPacket.Items.Add(info);
                        }
                    }
                }

                Send(itemListPacket);
            }

            ServerState = PlayerServerState.ServerStateGame;

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.EnterGame, MyPlayer);
            });

        }

        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            // TODO : 이런 저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb findPlayer = db.Players
                    .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

                if(findPlayer != null)
                {
                    // 이름이 겹친다 => 빈 걸 보내서 클라에서 처리하게 함
                    Send(new S_CreatePlayer());
                }
                else 
                {
                    // 1레벨 스탯 정보 추출
                    StatInfo stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);

                    // DB에 플레이어 만들어 줌
                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        Level = stat.Level,
                        Hp = stat.Hp,
                        Mp = stat.Mp,
                        MaxHp = stat.MaxHp,
                        MaxMp = stat.MaxMp,
                        Attack = stat.Attack,
                        Speed = stat.Speed,
                        TotalExp = 0,
                        AccountDbId = AccountDbId // 저장해둔 AccountDbId 사용
                    };

                    db.Players.Add(newPlayerDb);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return; // TODO

                    // 메모리 추가
                    LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                    {
                        PlayerDbId = newPlayerDb.PlayerDbId,
                        Name = createPacket.Name,
                        StatInfo = new StatInfo()
                        {
                            Level = stat.Level,
                            Hp = stat.Hp,
                            MaxHp = stat.MaxHp,
                            Mp = stat.Mp,
                            MaxMp = stat.MaxMp,
                            Attack = stat.Attack,
                            Speed = stat.Speed,
                            TotalExp = 0
                        }
                    };

                    LobbyPlayers.Add(lobbyPlayer);

                    // 클라에 전송
                    S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
                    newPlayer.Player.MergeFrom(lobbyPlayer);

                    Send(newPlayer);
                }
            }
        }
    }
}
