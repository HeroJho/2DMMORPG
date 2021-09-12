using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public abstract class Quest
    {
        public int QuestDbId { get; protected set; }
        public int QuestId { get; protected set; }
        public int NpcId { get; protected set; } // 누구의 퀘스트
        public QuestType QuestType { get; protected set; }
        public QuestState QuestState { get; protected set; }
        public bool IsChanged { get; set; } = false;


        public virtual void ProceedWithQuest(int monsterId = 0)
        {

        }

        public virtual void ProceedWithQuest(int id, Player player)
        {

        }

        public virtual void CompleteQuest(Player player)
        {

        }

        // 퀘스트 수락할 때 만듬
        public static Quest MakeQuest(int templateId)
        {
            Quest quest = null;

            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(templateId, out questData);

            if (questData == null)
                return null;

            switch (questData.questType)
            {
                case QuestType.Hunting:
                    quest = new HuntingQuest(templateId);
                    break;
                case QuestType.Collection:
                    quest = new CollectionQuest(templateId);
                    break;

            }

            return quest;
        }
        // 퀘스트를 불러올 때 만듬
        public static Quest MakeQuest(QuestDb questDb)
        {
            Quest quest = null;

            QuestData questData = null;
            DataManager.QuestDict.TryGetValue(questDb.TmeplateId, out questData);

            if (questData == null)
                return null;

            switch (questData.questType)
            {
                case QuestType.Hunting:
                    quest = new HuntingQuest(questDb);
                    break;
                case QuestType.Collection:
                    quest = new CollectionQuest(questDb);
                    break;

            }

            return quest;

        }

    }
}
