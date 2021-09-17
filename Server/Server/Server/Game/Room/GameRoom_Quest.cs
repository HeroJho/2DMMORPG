using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public partial class GameRoom : JobSerializer
    {

        public void HandleAcceptQuest(Player player, C_AddQuest questPacket)
        {
            if (player == null && player.Room == null)
                return;

            // 받을 수 있는 퀘스트 인가?
            // 퀘스트 조건 확인 하기
            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(questPacket.QuestId, out questData);
            if (questData == null)
                return;
            // 클리어한 퀘스트이냐
            Quest clearQuest = null;
            if (player.Quest.CompletedQuests.TryGetValue(questPacket.QuestId, out clearQuest) == true)
                return;
            // 퀘스트 레벨이 되냐
            if (player.Stat.Level < questData.condition.level)
                return;
            // 클리어 퀘스트를 충족하냐
            foreach (int questId in questData.condition.completeQuests)
            {
                Quest clear = null;
                if (player.Quest.CompletedQuests.TryGetValue(questId, out clear) == false)
                    return;
            }

            Quest quest = Quest.MakeQuest(questPacket.QuestId);

            player.Quest.AcceptQuest(quest);

            // 클라 통보
            S_AddQuest addQuestPacket = new S_AddQuest();
            addQuestPacket.QuestId = questPacket.QuestId;
            addQuestPacket.NpcId = questData.npcId;
            player.Session.Send(addQuestPacket);

            // 수집퀘스트인 경우 수락과 동시에 한번 실행
            if (quest.QuestType == QuestType.Collection)
            {
                CollectionQuest collectionQuest = (CollectionQuest)quest;
                player.Quest.ProceddWithQuest(collectionQuest.ItemId);
            }

        }

        public void HandleTryCompleteQuest(Player player, C_TryCompleteQuest questPacket)
        {
            if (player == null && player.Room == null)
                return;

            if (!player.Quest.CompleteQuest(questPacket.QuestId))
                return;

            // 클라 통보
            S_CompleteQuest completeQuestPacket = new S_CompleteQuest();
            completeQuestPacket.QuestId = questPacket.QuestId;
            completeQuestPacket.NpcId = questPacket.NpcId;
            player.Session.Send(completeQuestPacket);
        }

    }
}
