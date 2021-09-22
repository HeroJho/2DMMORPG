using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class CollectionQuest : Quest
    {
        public int ItemId { get; private set; } // 어떤 몬스터를
        public int PurposeNumber { get; private set; } // 퀘스트 마릿수
        public int CurrentNumber { get; private set; } // 현재 마릿수

        public CollectionQuest(int templateId)
        {
            Init(templateId);
        }

        public CollectionQuest(QuestDb questDb)
        {
            LoadQuest(questDb);
        }

        void Init(int templateId)
        {
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(templateId, out questData);
            if (questData.questType != QuestType.Collection)
                return;

            CollectionQuestData data = (CollectionQuestData)questData;
            {
                QuestDbId = 0;
                QuestId = data.id;
                NpcId = data.npcId;
                QuestType = QuestType.Collection;
                QuestState = QuestState.Proceed;
                ObstacleRemoves = data.obstacleRemoves;

                ItemId = data.itemId;
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

            CollectionQuestData data = (CollectionQuestData)questData;
            {
                QuestId = data.id;
                NpcId = data.npcId;
                QuestType = QuestType.Collection;
                ObstacleRemoves = data.obstacleRemoves;

                ItemId = data.itemId;
                PurposeNumber = data.purposeNumber;

                QuestDbId = questDb.QuestDbId;
                QuestState = (QuestState)questDb.QuestState;
                if (questDb.CurrentNumber != null)
                    CurrentNumber = questDb.CurrentNumber.Value;
            }
        }

        public override void ProceedWithQuest(int id, Player player)
        {
            if (player == null || player.Room == null)
                return;

            if (id != ItemId) // 먹은 아이템이 아예 다른 아이템이라면 바로 취소 > 서치 시간 단축
                return;

            // 인벤에 아이템이 있는지 찾아본다
            // 아이템을 버릴 경우 없어졌으니 0으로 초기화
            Item item = player.Inven.Find(i => i.TemplateId == ItemId);
            if (item == null)
                CurrentNumber = 0;
            else
                CurrentNumber = item.Count;


            // 퀘스트 완료
            if (CurrentNumber >= PurposeNumber)
            {
                QuestState = QuestState.Cancomplete;
            }
            else if(QuestState == QuestState.Cancomplete)
            {// 수집 퀘스트는 템을 버리거나 할 수 있어서 Cancomplete > Proceed 로 변경 가능
                QuestState = QuestState.Proceed;
                player.Quest.CanCompleteQuests.Remove(QuestId);
            }

            IsChanged = true;

        }

        public override void CompleteQuest(Player player)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            // 인벤에 아이템이 있는지 찾아본다
            Item item = player.Inven.Find(i => i.TemplateId == ItemId);
            if (item == null)
                return;

            // 해당 아이템 삭제
            DbTransaction.RemoveCountItem(player, item, PurposeNumber, room);

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
