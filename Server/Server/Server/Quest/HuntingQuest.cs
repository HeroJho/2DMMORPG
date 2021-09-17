using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class HuntingQuest : Quest
    {
        public int MonsterId { get; private set; } // 어떤 몬스터를
        public int PurposeNumber { get; private set; } // 퀘스트 마릿수
        public int CurrentNumber { get; private set; } // 현재 마릿수

        public HuntingQuest(int templateId)
        {
            Init(templateId);
        }

        public HuntingQuest(QuestDb questDb)
        {
            LoadQuest(questDb);
        }

        void Init(int templateId)
        {
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(templateId, out questData);
            if (questData.questType != QuestType.Hunting)
                return;

            HuntingQuestData data = (HuntingQuestData)questData;
            {
                QuestDbId = 0;
                QuestId = data.id;
                NpcId = data.npcId;
                QuestType = QuestType.Hunting;
                QuestState = QuestState.Proceed;
                ObstacleRemoves = data.obstacleRemoves;

                MonsterId = data.monsterId;
                PurposeNumber = data.purposeNumber;
                CurrentNumber = data.currentNumber;                
            }
        }

        public void LoadQuest(QuestDb questDb)
        {
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(questDb.TmeplateId, out questData);

            if (questData == null)
                return;

            HuntingQuestData data = (HuntingQuestData)questData;
            {
                QuestId = data.id;
                NpcId = data.npcId;
                QuestType = QuestType.Hunting;
                ObstacleRemoves = data.obstacleRemoves;

                MonsterId = data.monsterId;
                PurposeNumber = data.purposeNumber;

                QuestDbId = questDb.QuestDbId;
                QuestState = (QuestState)questDb.QuestState;
                if(questDb.CurrentNumber != null)
                    CurrentNumber = questDb.CurrentNumber.Value;
            }
        }

        public override void ProceedWithQuest(int monsterId = 0)
        {
            if (MonsterId != monsterId)
                return;

            CurrentNumber++;

            // 퀘스트 완료
            if(CurrentNumber >= PurposeNumber)
            {
                QuestState = QuestState.Cancomplete;
            }

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
