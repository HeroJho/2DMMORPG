using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager
{
    // 수행중인 퀘스트 목록 받음
    // 이 정보로 UI에 출력
    public Dictionary<int, Quest> Quests { get; set; } = new Dictionary<int, Quest>();
    public Dictionary<int, Quest> CanCompleteQuests { get; set; } = new Dictionary<int, Quest>();
    public Dictionary<int, Quest> CompletedQuests { get; set; } = new Dictionary<int, Quest>();

    private Action<MyPlayerController> _condition;
    // 딕셔너리는 condition을 삭제하는 용도 > 완료하면 굳이 조건을 검사 안하도록
    private Dictionary<int, Action<MyPlayerController>> _conditions = new Dictionary<int, Action<MyPlayerController>>();

    public void ParsingQuest(QuestInfo questInfo)
    {
        if (questInfo == null)
            return;

        foreach (int id in questInfo.Quests)
        {
            QuestData questData = null;
            Managers.Data.QuestDict.TryGetValue(id, out questData);

            switch (questData.questType)
            {
                case QuestType.Hunting:
                    {
                        HuntingQuestData huntinQuestData = (HuntingQuestData)questData;

                        HuntingQuest quest = new HuntingQuest();
                        quest.Init(huntinQuestData);
                        quest.QuestState = QuestState.Proceed;

                        if (!Quests.ContainsKey(quest.QuestId))
                            Quests.Add(quest.QuestId, quest);
                    }
                    break;
                case QuestType.Collection:
                    {
                        CollectionQuestData collectionQuestData = (CollectionQuestData)questData;

                        CollectionQuest quest = new CollectionQuest();
                        quest.Init(collectionQuestData);
                        quest.QuestState = QuestState.Proceed;

                        if (!Quests.ContainsKey(quest.QuestId))
                            Quests.Add(quest.QuestId, quest);
                    }
                    break;
                case QuestType.Complete:
                    {
                        CompleteQuestData collectionQuestData = (CompleteQuestData)questData;

                        CompletingQuest quest = new CompletingQuest();
                        quest.Init(collectionQuestData);
                        quest.QuestState = QuestState.Proceed;

                        if (!Quests.ContainsKey(quest.QuestId))
                            Quests.Add(quest.QuestId, quest);
                    }
                    break;
            }
        }
        foreach (int id in questInfo.CanCompleteQuests)
        {
            QuestData questData = null;
            Managers.Data.QuestDict.TryGetValue(id, out questData);

            Quest quest = null;
            Quests.TryGetValue(id, out quest);
            quest.QuestState = QuestState.Cancomplete;

            if (!CanCompleteQuests.ContainsKey(quest.QuestId))
                CanCompleteQuests.Add(quest.QuestId, quest);
        }

        foreach (int id in questInfo.CompletedQuests)
        {
            QuestData questData = null;
            Managers.Data.QuestDict.TryGetValue(id, out questData);

            switch (questData.questType)
            {
                case QuestType.Hunting:
                    {
                        HuntingQuestData huntinQuestData = (HuntingQuestData)questData;

                        HuntingQuest quest = new HuntingQuest();
                        quest.Init(huntinQuestData);
                        quest.QuestState = QuestState.Complete;

                        if (!CompletedQuests.ContainsKey(quest.QuestId))
                            CompletedQuests.Add(quest.QuestId, quest);
                    }
                    break;
                case QuestType.Collection:
                    {
                        CollectionQuestData collectionQuestData = (CollectionQuestData)questData;

                        CollectionQuest quest = new CollectionQuest();
                        quest.Init(collectionQuestData);
                        quest.QuestState = QuestState.Complete;

                        if (!CompletedQuests.ContainsKey(quest.QuestId))
                            CompletedQuests.Add(quest.QuestId, quest);
                    }
                    break;
                case QuestType.Complete:
                    {
                        CompleteQuestData collectionQuestData = (CompleteQuestData)questData;

                        CompletingQuest quest = new CompletingQuest();
                        quest.Init(collectionQuestData);
                        quest.QuestState = QuestState.Complete;

                        if (!CompletedQuests.ContainsKey(quest.QuestId))
                            CompletedQuests.Add(quest.QuestId, quest);
                    }
                    break;

            }
        }

    }

    public void ViewQuest(NpcController nc)
    {
        if (nc == null)
            return;

        //던전 안내라면
        DungunGiver dungun = nc.GetComponent<DungunGiver>();
        ShopGiver shop = nc.GetComponent<ShopGiver>();
        if (dungun != null)
        {
            dungun.GetPanelInfo();
            return;
        }
        else if(shop != null)
        {
            shop.GetPanelInfo();
            return;
        }

        QuestGiver npc = nc.GetComponent<QuestGiver>();

        // 완료가능 > 수행가능 > 수행중 순으로 반환
        Quest quest = npc.FindStateQuest();

        if (quest == null) // 퀘스트가 없다면 일반 대사
            Managers.UI.ShowPopupUI<UI_Quest>().RefreshUI(npc);
        else // 퀘스트의 상황에 맞게 대사
            Managers.UI.ShowPopupUI<UI_Quest>().RefreshUI(quest);

    }

    public void TryReceiveQuest(Quest quest)
    {
        if (quest == null)
            return;

        // 퀘스트 중복 수락 불가
        if (Quests.ContainsKey(quest.QuestId))
            return;

        // 퀘스트 서버에 전송
        C_AddQuest addQuestPacket = new C_AddQuest();
        addQuestPacket.QuestId = quest.QuestId;
        Managers.Network.Send(addQuestPacket);

    }

    public void TryCompleteQuest(Quest quest)
    {
        if (quest == null)
            return;

        if (!CanCompleteQuests.ContainsKey(quest.QuestId))
            return;

        // 퀘스트 서버에 전송
        C_TryCompleteQuest tryCompleteQuestPacket = new C_TryCompleteQuest();
        tryCompleteQuestPacket.QuestId = quest.QuestId;
        tryCompleteQuestPacket.NpcId = quest.NpcId;
        Managers.Network.Send(tryCompleteQuestPacket);

    }

    public void ReceiveQuest(S_AddQuest addQuestPacket)
    {
        QuestGiver npc = Managers.Object.FindNpcWithId(addQuestPacket.NpcId).GetComponent<QuestGiver>();
        
        Quest quest = null;
        npc.QuestList.TryGetValue(addQuestPacket.QuestId, out quest);

        Quests.Add(addQuestPacket.QuestId, quest);
        quest.QuestState = QuestState.Proceed;
        npc.RefreshMark();

        // 퀘스트판넬 갱신
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.QuestUI.RefreshUI();
    }

    public bool CompleteQuest(int questId)
    {
        // 완료할 수 있는 퀘스트 인가?
        Quest canQuest = null;
        if (CanCompleteQuests.TryGetValue(questId, out canQuest) == false)
            return false;

        canQuest.QuestState = QuestState.Complete;

        Quests.Remove(questId);
        CanCompleteQuests.Remove(questId);
        CompletedQuests.Add(canQuest.QuestId, canQuest);

        // 마크 갱신
        GameObject go = Managers.Object.FindNpcWithId(canQuest.NpcId);
        go.GetComponent<QuestGiver>().RefreshMark();
        // 완료했으니 완료 퀘스트 조건 체크
        CheckCondition();

        // 컷씬 있으면 실행
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(questId, out questData);
        if(questData.cutSceneId != -1)
            Managers.CutScene.StartCutScene(questData.cutSceneId);

        // 퀘스트판넬 갱신
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.QuestUI.RefreshUI();

        return true;
    }

    public void RefreshQuest(S_RefreshHuntingQuest refreshHuntingQuestPacket)
    {
        Quest quest = FindQuest(refreshHuntingQuestPacket.QuestId);

        switch (quest.QuestType)
        {
            case QuestType.Hunting:
                {
                    HuntingQuest huntingQuest = (HuntingQuest)quest;
                    huntingQuest.CurrentNumber = refreshHuntingQuestPacket.CurrentNumber;
                }
                break;
            case QuestType.Collection:
                {
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    collectionQuest.CurrentNumber = refreshHuntingQuestPacket.CurrentNumber;
                    // 수집퀘는 완료 > 진행중 으로 변경 가능 (아이템을 버리거나)
                    // 완료가능은 서버에서 따로 통보해서 상태변경
                    if(collectionQuest.CurrentNumber < collectionQuest.PurposeNumber)
                    {
                        collectionQuest.QuestState = QuestState.Proceed;

                        // Proceed로 바꼇다면 Npc퀘스트 마크도 변경
                        CanCompleteQuests.Remove(collectionQuest.QuestId);
                        QuestGiver questGiver = Managers.Object.FindNpcWithId(collectionQuest.NpcId).
                            GetComponent<QuestGiver>();

                        questGiver.RefreshMark();
                    }
                }
                break;
            case QuestType.Complete:
                {
                    CompletingQuest collectionQuest = (CompletingQuest)quest;
                    collectionQuest.RefreshQuest();
                }
                break;
        }

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.QuestUI.RefreshUI();

    }

    public Quest FindQuest(int id)
    {
        Quest quest = null;
        Quests.TryGetValue(id, out quest);

        return quest;
    }

    public void InitQuests(QuestGiver questGiver)
    {
        foreach (QuestData questData in Managers.Data.QuestDict.Values)
        {
            if (questGiver.NpcId != questData.npcId)
                continue;

            // 이미 성공한 퀘스트는 Npc퀘스트에 등록 X
            Quest checkTemp = null;
            if (CompletedQuests.TryGetValue(questData.id, out checkTemp) == true)
                continue;

            questGiver.AddQuest(questData);
        }

        questGiver.RefreshMark();
    }


    public void AddCondition(Action<MyPlayerController> condition, int questId)
    {
        _condition += condition;

        _conditions.Add(questId, condition);
    }

    public void DeleteCondition(int questId)
    {
        Action<MyPlayerController> condition = null;
        _conditions.TryGetValue(questId, out condition);

        _condition -= condition;

        _conditions.Remove(questId);

    }

    public void CheckCondition()
    {
        MyPlayerController mpc = Managers.Object.MyPlayer;

        if(_condition != null)
            _condition.Invoke(mpc);
    }

    public void Clear()
    {
        //CanCompleteQuests.Clear();
        //CompletedQuests.Clear();
        //Quests.Clear();

        _condition = null;
        _conditions.Clear();
    }
}
