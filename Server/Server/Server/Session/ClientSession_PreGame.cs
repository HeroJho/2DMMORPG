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
                            PosX = playerDb.PosX,
                            PosY = playerDb.PosY,
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
                // 플레이어 정보
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = playerInfo.PosX;
                MyPlayer.Info.PosInfo.PosY = playerInfo.PosY;
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;

                
                // 아이템 정보
                S_ItemList itemListPacket = new S_ItemList();
                // 스킬 정보
                S_SkillPoint skillPointPacket = new S_SkillPoint();

                using (AppDbContext db = new AppDbContext())
                {
                    // Loading Item
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
                            info.MergeFrom(item.ItemInfo);
                            itemListPacket.Items.Add(info);
                        }
                    }


                    // Loading Quest
                    List<QuestDb> quests = db.Quests
                        .Where(q => q.OwnerDbId == MyPlayer.PlayerDbId)
                        .ToList();

                    foreach (QuestDb questDb in quests)
                    {
                        Quest quest = Quest.MakeQuest(questDb);
                        if (quest == null)
                            continue;

                        switch (quest.QuestState)
                        {
                            case QuestState.Proceed:
                                {
                                    MyPlayer.Quest.Quests.Add(quest.QuestId, quest);
                                }
                                break;
                            case QuestState.Cancomplete:
                                {
                                    MyPlayer.Quest.Quests.Add(quest.QuestId, quest);
                                    MyPlayer.Quest.CanCompleteQuests.Add(quest.QuestId, quest);
                                }
                                break;
                            case QuestState.Complete:
                                {
                                    MyPlayer.Quest.CompletedQuests.Add(quest.QuestId, quest);
                                }
                                break;
                        }
                                               
                    }


                    // Loading Obstacle
                    foreach (ObstacleData obstacleData in DataManager.ObstacleDict.Values)
                    {
                        Quest tempQuest = null;
                        if (MyPlayer.Quest.CompletedQuests.TryGetValue(obstacleData.despawnConditionQuestId, out tempQuest))
                            continue;

                        Obstacle obstacle = Obstacle.MakeObstacle(obstacleData);
                        if (obstacle == null)
                            continue;

                        MyPlayer.Obstacle.Add(obstacle);
                    }


                    // Loading SkillInfo
                    SkillDb skills = db.Skills
                        .Where(s => s.OwnerDbId == MyPlayer.PlayerDbId)
                        .FirstOrDefault();

                    MyPlayer.Skill.SkillDbId = skills.SkillDbId;
                    ConvertIntStringData convertData = new ConvertIntStringData();
                    convertData = skills.ConvertStringToInt();
                    MyPlayer.Skill.SkillPoint = skills.SkillPoints;
                    MyPlayer.Skill.SkillTree.SkillPoints = convertData.SkillPoints;

                    skillPointPacket.Points = MyPlayer.Skill.SkillPoint;
                    foreach (int key in MyPlayer.Skill.SkillTree.SkillPoints.Keys)
                    {
                        SkillInfo skillInfo = new SkillInfo();
                        skillInfo.SkillId = key;
                        skillInfo.Point = MyPlayer.Skill.SkillTree.SkillPoints[key];
                        skillPointPacket.SkillInfos.Add(skillInfo);
                    }

                }

                // 퀘스트 같은 경우는 EnterGame할 때 보내줌
                Send(itemListPacket);
                Send(skillPointPacket);
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
                        JobClassType = (int)JobClassType.None,
                        StatPoints = 0,
                        AccountDbId = AccountDbId // 저장해둔 AccountDbId 사용
                    };
                    {
                        db.Players.Add(newPlayerDb);
                        bool success = db.SaveChangesEx();
                        if (success == false)
                            return; // TODO
                    }

                    //스킬 기본설정
                    ConvertIntStringData convertData = new ConvertIntStringData();
                    {
                        convertData.SkillPoints.Add(1, 1);
                        convertData.SkillPoints.Add(2, 1);
                        convertData.SkillPoints.Add(3, 1);
                    }
                    SkillDb newSkillDb = new SkillDb();
                    {
                        newSkillDb.OwnerDbId = newPlayerDb.PlayerDbId;
                        newSkillDb.SkillPoints = 0;
                        newSkillDb.ConvertIntToString(convertData);
                    }
                    {
                        db.Skills.Add(newSkillDb);
                        bool success = db.SaveChangesEx();
                        if (success == false)
                            return; // TODO
                    }

                    // 메모리 추가
                    LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                    {
                        PlayerDbId = newPlayerDb.PlayerDbId,
                        Name = createPacket.Name,
                        PosX = -50,
                        PosY = -75,
                        StatInfo = new StatInfo()
                        {
                            Level = stat.Level,
                            Hp = stat.Hp,
                            MaxHp = stat.MaxHp,
                            Mp = stat.Mp,
                            MaxMp = stat.MaxMp,
                            Attack = stat.Attack,
                            Speed = stat.Speed,
                            TotalExp = 0,
                            JobClassType = (int)JobClassType.None,
                            StatPoints = 0,
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
