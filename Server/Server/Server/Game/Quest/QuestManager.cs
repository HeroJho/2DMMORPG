using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class QuestManager
    {
        public Dictionary<int, Quest> Quests { get; set; } = new Dictionary<int, Quest>();
        public Dictionary<int, Quest> CanCompleteQuests { get; set; } = new Dictionary<int, Quest>();
        public Dictionary<int, Quest> CompletedQuests { get; set; } = new Dictionary<int, Quest>();
        Player _player;

        public QuestManager(Player player)
        {
            _player = player;
        }

        public void AcceptQuest(Quest quest)
        {
            if (quest == null)
                return;

            // 똑같은 퀘스트 수락 방지
            Quest checkTemp = null;
            if (Quests.TryGetValue(quest.QuestId, out checkTemp) == true)
                return;

            Quests.Add(quest.QuestId, quest);
        }

        public bool CompleteQuest(int questId)
        {
            GameRoom room = _player.Room;
            if (_player == null || room == null)
                return false;

            // 완료할 수 있는 퀘스트 인가?
            Quest canQuest = null;
            if (CanCompleteQuests.TryGetValue(questId, out canQuest) == false)
                return false;

            // 보상
            canQuest.CompleteQuest(_player);

            // Obstacle이 있으면 제거
            if(canQuest.ObstacleRemoves != null)
            {
                S_DespawnObstacle deSpawnObstacle = new S_DespawnObstacle();

                foreach (int obstacleId in canQuest.ObstacleRemoves)
                {
                    Obstacle obstacle = _player.Obstacle.RemoveObstacle(obstacleId);
                    if (obstacle != null)
                        deSpawnObstacle.TemplateId.Add(obstacle.TemplateId);
                }

                _player.Session.Send(deSpawnObstacle);
            }

            // 리스트 관리
            Quests.Remove(questId);
            CanCompleteQuests.Remove(questId);
            CompletedQuests.Add(canQuest.QuestId, canQuest);

            ProceddWithQuest();

            return true;
        }

        public void RemoveQuest(int questId)
        {
            if (Quests.Remove(questId) == false)
                return;

        }

        public Quest FindQuest(int questId)
        {
            Quest quest = null;
            if (Quests.TryGetValue(questId, out quest) == false)
                return null;

            return quest;
        }

        public void ProceddWithQuest(int id = 0)
        {
            if (_player == null || _player.Room == null)
                return;

            if (Quests.Count <= 0)
                return;

            List<Quest> quests = new List<Quest>();

            foreach (Quest quest in Quests.Values)
            {
                if (quest.QuestType == QuestType.Hunting)
                    quest.ProceedWithQuest(id);
                else if (quest.QuestType == QuestType.Collection)
                    quest.ProceedWithQuest(id, _player);
                else if (quest.QuestType == QuestType.Complete)
                    quest.ProceedWithQuest(_player);
                
                // 퀘스트 내용이 실행 돼서 변화가 있다면
                if(quest.IsChanged)
                {
                    quest.IsChanged = false;

                    quests.Add(quest);
                }
                // 퀘스트를 완료 했다면
                if(quest.QuestState == QuestState.Cancomplete)
                {
                    CanCompleteQuest(quest);
                }
            }

            if (quests.Count <= 0)
                return;

            foreach (Quest quest in quests)
            {
                RefreshQuest(quest);
            }
        }

        public void RefreshQuest(Quest quest)
        {
            switch (quest.QuestType)
            {
                case QuestType.Hunting:
                    {
                        // TODO : UI갱신하도록 클라에 패킷 전송
                        HuntingQuest huntingQuest = (HuntingQuest)quest;

                        S_RefreshHuntingQuest refreshHuntingQuestPacket = new S_RefreshHuntingQuest();
                        refreshHuntingQuestPacket.QuestId = huntingQuest.QuestId;
                        refreshHuntingQuestPacket.CurrentNumber = huntingQuest.CurrentNumber;

                        _player.Session.Send(refreshHuntingQuestPacket);
                    }
                    break;
                case QuestType.Collection:
                    {
                        // TODO : UI갱신하도록 클라에 패킷 전송
                        CollectionQuest collectionQuest = (CollectionQuest)quest;

                        S_RefreshHuntingQuest refreshHuntingQuestPacket = new S_RefreshHuntingQuest();
                        refreshHuntingQuestPacket.QuestId = collectionQuest.QuestId;
                        refreshHuntingQuestPacket.CurrentNumber = collectionQuest.CurrentNumber;

                        _player.Session.Send(refreshHuntingQuestPacket);
                    }
                    break;
                case QuestType.Complete:
                    {
                        // 그냥 패킷만 보내서 클라쪽에서 확인 하도록 함
                        CompletingQuest collectionQuest = (CompletingQuest)quest;

                        S_RefreshHuntingQuest refreshHuntingQuestPacket = new S_RefreshHuntingQuest();
                        refreshHuntingQuestPacket.QuestId = collectionQuest.QuestId;

                        _player.Session.Send(refreshHuntingQuestPacket);
                    }
                    break;

            }

        }

        private void CanCompleteQuest(Quest quest)
        {
            Quest chackTmp = null;
            if(CanCompleteQuests.TryGetValue(quest.QuestId, out chackTmp))
                return;

            if (CompletedQuests.TryGetValue(quest.QuestId, out chackTmp))
                return;

            CanCompleteQuests.Add(quest.QuestId, quest);

            S_CanCompleteQuest completeQuestPacket = new S_CanCompleteQuest();
            completeQuestPacket.QuestId = quest.QuestId;
            completeQuestPacket.NpcId = quest.NpcId;

            _player.Session.Send(completeQuestPacket);
        }

        public void Clear()
        {
            Quests.Clear();
            CanCompleteQuests.Clear();
            CompletedQuests.Clear();
        }

    }
}
