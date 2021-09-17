using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class CompletingQuest : Quest
    {
        public List<int> CompleteQuestIds { get; private set; } = new List<int>();

        public CompletingQuest(int templateId)
        {
            Init(templateId);
        }

        public CompletingQuest(QuestDb questDb)
        {
            LoadQuest(questDb);
        }

        void Init(int templateId)
        {
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(templateId, out questData);
            if (questData.questType != QuestType.Complete)
                return;

            CompleteQuestData data = (CompleteQuestData)questData;
            {
                QuestDbId = 0;
                QuestId = data.id;
                NpcId = data.npcId;
                QuestType = QuestType.Complete;
                QuestState = QuestState.Proceed;
                ObstacleRemoves = data.obstacleRemoves;

                CompleteQuestIds = data.completeQuestIds;
            }
        }

        public void LoadQuest(QuestDb questDb)
        {
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(questDb.TmeplateId, out questData);

            if (questData == null)
                return;

            CompleteQuestData data = (CompleteQuestData)questData;
            {
                QuestId = data.id;
                NpcId = data.npcId;
                QuestType = QuestType.Complete;
                ObstacleRemoves = data.obstacleRemoves;

                CompleteQuestIds = data.completeQuestIds;

                QuestDbId = questDb.QuestDbId;
                QuestState = (QuestState)questDb.QuestState;
            }
        }

        public override void ProceedWithQuest(Player player)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;
            List<int> questIds = new List<int>();
            foreach (int id in CompleteQuestIds)
            {
                Quest quest = null;
                player.Quest.CompletedQuests.TryGetValue(id, out quest);

                if (quest == null)
                    continue;

                questIds.Add(id);
            }

            if (questIds.Count == 0)
                return;

            // 퀘스트 완료
            foreach (int id in CompleteQuestIds)
            {
                if (!questIds.Contains(id))
                {
                    QuestState = QuestState.Proceed;
                    break;
                }

                QuestState = QuestState.Cancomplete;
            }


            if(questIds.Count > 0)
                IsChanged = true;
        }

        public override void CompleteQuest(Player player)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            // 보상
            QuestData quest = null;
            if (DataManager.QuestDict.TryGetValue(QuestId, out quest) == false)
                return;
            foreach (QuestRewardData rewardData in quest.rewards)
            {
                if (rewardData.itemId != 0)
                {
                    DbTransaction.RewardQuestPlayer(player, rewardData, room);
                }
                else if (rewardData.exp != 0)
                {
                    player.GetEx(rewardData.exp);
                }
            }

            QuestState = QuestState.Complete;
        }

    }
}

