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

        public int TempRoomId;

        public void HandleLogin(C_Login loginPacket)
        {
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            LobbyPlayers.Clear();

            using (AppDbContext db = new AppDbContext())
            {
                // AccountName은 ID
                // 패킷 UniqueId는 로그인 Db에서 가져온 ID임
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId)
                    .FirstOrDefault();

                S_Login loginOkPacket = new S_Login() { LoginOk = 1 };

                // 만약 해당 서버에서 계정ID에 맞는 Account가 없다
                if (findAccount == null)
                {
                    // Account를 만들어주고 빈 캐릭 선택창을 띄운다
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return; // TODO

                    // AccountDbId 메모리에 기억
                    AccountDbId = newAccount.AccountDbId;

                    // 캐릭터 리스트 없는 빈 패킷 전송 >> 선택창만 띄움
                    Send(loginOkPacket);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                    return;
                }
                else // 해당 서버에 Account가 존재한다 >> Account의 Players를 사용해서 캐릭 정보 전송
                {
                    // AccountDbId 메모리에 기억
                    AccountDbId = findAccount.AccountDbId;

                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name = playerDb.PlayerName,
                            Gold = playerDb.Gold,
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
                                Defence = playerDb.Defence,
                                Str = playerDb.Str,
                                Int = playerDb.Int,
                                Speed = playerDb.Speed,
                                StatPoints = playerDb.StatPoints,

                                JobClassType = (JobClassType)playerDb.JobClassType,
                                CanUpClass = playerDb.CanUpClass

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
                MyPlayer.Info.Gold = playerInfo.Gold;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = playerInfo.PosX;
                MyPlayer.Info.PosInfo.PosY = playerInfo.PosY;
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;
                // ClassType에 따라 Tree클래스 설정
                MyPlayer.Skill.SetSkillTree();

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
                        // TODO DB에 현재 위치한 맵저장을 넣어야함
                        if (obstacleData.mapId != 1)
                            continue;

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
                        PosX = -50,
                        PosY = -75,
                        Gold = 500,
                        Level = stat.Level,
                        Hp = stat.Hp,
                        Mp = stat.Mp,
                        MaxHp = stat.MaxHp,
                        MaxMp = stat.MaxMp,
                        Attack = stat.Attack,
                        Defence = stat.Defence,
                        Str = stat.Str,
                        Int = stat.Int,
                        Speed = stat.Speed,
                        TotalExp = 0,
                        JobClassType = (int)JobClassType.None,
                        CanUpClass = false,
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
                        convertData.SkillPoints.Add(1001, 1);
                        convertData.SkillPoints.Add(1002, 1);
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
                        Gold = newPlayerDb.Gold,
                        PosX = newPlayerDb.PosX,
                        PosY = newPlayerDb.PosY,
                        StatInfo = new StatInfo()
                        {
                            Level = stat.Level,
                            Hp = stat.Hp,
                            MaxHp = stat.MaxHp,
                            Mp = stat.Mp,
                            MaxMp = stat.MaxMp,
                            Attack = stat.Attack,
                            Defence = stat.Defence,
                            Str = stat.Str,
                            Int = stat.Int,
                            Speed = stat.Speed,
                            TotalExp = 0,
                            JobClassType = JobClassType.None,
                            CanUpClass = false,
                            StatPoints = 0,
                        }
                    };

                    LobbyPlayers.Add(lobbyPlayer);

                    // 클라에 전송
                    S_CreatePlayer newPlayer = new S_CreatePlayer();
                    foreach (LobbyPlayerInfo info in LobbyPlayers)
                        newPlayer.Players.Add(info);

                    Send(newPlayer);
                }
            }
        }

        public void HandleDeletePlayer(C_DeletePlayer deletePacket)
        {
            // TODO : 이런 저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb findPlayer = db.Players
                    .Where(p => p.PlayerName == deletePacket.Name).FirstOrDefault();

                if (findPlayer == null)
                {
                    // 닉네임이 없다? >> 일단 빈 걸 보내서 클라에서 처리하게 함
                    Send(new S_DeletePlayer());
                }
                else
                {
                    LobbyPlayerInfo lobbyPlayer = LobbyPlayers.Find((e) => e.Name == findPlayer.PlayerName);
                    if (lobbyPlayer == null)
                        return;

                    db.Players.Remove(findPlayer);
                    db.SaveChangesEx();

                    LobbyPlayers.Remove(lobbyPlayer);

                    // 클라에 전송
                    S_DeletePlayer deletePlayer = new S_DeletePlayer();
                    foreach (LobbyPlayerInfo info in LobbyPlayers)
                        deletePlayer.Players.Add(info);

                    Send(deletePlayer);
                }
            }
        }

        public void HandleChangeRoom()
        {
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

                foreach (ItemDb itemDb in items)
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
                    // 방이 다르면 패스
                    if (obstacleData.mapId != TempRoomId)
                        continue;

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


            // 초기 위치
            if(TempRoomId == 1)
                MyPlayer.CellPos = new Vector2Int(-33, -36);
            else
                MyPlayer.CellPos = new Vector2Int(25, -35);

            // 상태 Idle
            MyPlayer.State = CreatureState.Idle;

            // 파티방 입장
            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(TempRoomId);

                room.Push(room.EnterGame, MyPlayer);
            });
        }
    }
}
