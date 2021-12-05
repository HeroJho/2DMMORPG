using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiver : MonoBehaviour
{
    public int NpcId { get; set; }

    public string NpcName { get; private set; }
    public string Description { get; private set; }

    public Dictionary<int, Quest> QuestList { get; set; } = new Dictionary<int, Quest>();

    public GameObject ProceedingMark;
    public GameObject QuestionMark;
    public GameObject ExclamationMark;

    public GameObject ProceedingMark_Mini;
    public GameObject QuestionMark_Mini;
    public GameObject ExclamationMark_Mini;

    public void Init(NpcData npcData)
    {
        NpcName = npcData.name;
        Description = npcData.Description;
    }

    public void AddQuest(QuestData questData)
    {
        // 이미 실행중, 완료가능 퀘스트는 새로 만들지 않는다.(퀘스트 State 유지)
        // 완료된 퀘스트는 애초에 Add하지 않음
        // 완료 가능한 퀘스트 먼저 확인 > 완료 가능은 Quests에도 존재한다.
        Quest checkQuest = null;
        if (Managers.Quest.CanCompleteQuests.TryGetValue(questData.id, out checkQuest))
        {
            checkQuest.QuestGiver = this;
            QuestList.Add(checkQuest.QuestId, checkQuest);
            return;
        }
        else if (Managers.Quest.Quests.TryGetValue(questData.id, out checkQuest))
        {
            checkQuest.QuestGiver = this;
            QuestList.Add(checkQuest.QuestId, checkQuest);
            return;
        }

        switch (questData.questType)
        {
            case QuestType.Hunting:
                {                    
                    HuntingQuestData huntinQuestData = (HuntingQuestData)questData;

                    HuntingQuest quest = new HuntingQuest();
                    quest.Init(huntinQuestData);
                    quest.QuestGiver = this;
                    quest.QuestState = QuestState.Cannotaccapt;

                    QuestList.Add(quest.QuestId, quest);

                    // 이벤트 등록
                    BindEvent(quest);

                }
                break;
            case QuestType.Collection:
                {
                    CollectionQuestData collectionQuestData = (CollectionQuestData)questData;

                    CollectionQuest quest = new CollectionQuest();
                    quest.Init(collectionQuestData);
                    quest.QuestGiver = this;
                    quest.QuestState = QuestState.Cannotaccapt;

                    QuestList.Add(quest.QuestId, quest);

                    // 이벤트 등록
                    BindEvent(quest);

                }
                break;
            case QuestType.Complete:
                {
                    CompleteQuestData collectionQuestData = (CompleteQuestData)questData;

                    CompletingQuest quest = new CompletingQuest();
                    quest.Init(collectionQuestData);
                    quest.QuestGiver = this;
                    quest.QuestState = QuestState.Cannotaccapt;

                    QuestList.Add(quest.QuestId, quest);

                    // 이벤트 등록
                    BindEvent(quest);

                }
                break;
        }


    }

    private void BindEvent(Quest quest)
    {
        Managers.Quest.AddCondition(p =>
        {
            // 잠겨있던 퀘스트만 확인하면 됨!
            if (quest.QuestState != QuestState.Cannotaccapt)
            {
                Managers.Quest.DeleteCondition(quest.QuestId);
                return;
            }

            // 레벨이 되냐
            if (p.Stat.Level < quest.Level)
                return;
            // 클리어 퀘스트를 충족하냐
            foreach (int questId in quest.CompleteQuests)
            {
                Quest clear = null;
                if (Managers.Quest.CompletedQuests.TryGetValue(questId, out clear) == false)
                    return;
            }

            quest.QuestState = QuestState.Canaccapt;
            RefreshMark();
        }, quest.QuestId);
    }

    public Quest FindStateQuest()
    {
        foreach (Quest quest in QuestList.Values)
        {
            if (quest.QuestState == QuestState.Cancomplete)
                return quest;
            else if (quest.QuestState == QuestState.Canaccapt)
                return quest;
            else if (quest.QuestState == QuestState.Proceed)
                return quest;
        }

        return null;
    }

    public Quest FindCanAcceptQuest()
    {
        foreach (Quest quest in QuestList.Values)
        {
            if (quest.QuestState == QuestState.Canaccapt)
                return quest;
        }

        return null;
    }

    public Quest FindProceedQuest()
    {
        foreach (Quest quest in QuestList.Values)
        {
            if (quest.QuestState == QuestState.Proceed)
                return quest;
        }

        return null;
    }

    public void RefreshMark()
    {
        ProceedingMark.SetActive(false);
        QuestionMark.SetActive(false);
        ExclamationMark.SetActive(false);

        ProceedingMark_Mini.SetActive(false);
        QuestionMark_Mini.SetActive(false);
        ExclamationMark_Mini.SetActive(false);

        foreach (Quest quest in QuestList.Values)
        {
            // 완료 가능 > 진행중 > 수행 가능 순으로 표시
            if(quest.QuestState == QuestState.Cancomplete)
            {
                QuestionMark.SetActive(true);
                QuestionMark_Mini.SetActive(true);
            }
            else if (quest.QuestState == QuestState.Proceed)
            {
                ProceedingMark.SetActive(true);
                ProceedingMark_Mini.SetActive(true);
            }
            else if (quest.QuestState == QuestState.Canaccapt)
            {
                ExclamationMark.SetActive(true);
                ExclamationMark_Mini.SetActive(true);
            }

        }
    }
}
