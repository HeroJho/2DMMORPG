using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HuntingQuest : Quest
{
    public int MonsterId; // 어떤 몬스터를
    public int PurposeNumber; // 퀘스트 마릿수
    public int CurrentNumber; // 현재 마릿수

    public void Init(HuntingQuestData questData)
    {
        QuestId = questData.id;
        NpcId = questData.npcId;
        QuestType = questData.questType;
        QuestState = QuestState.Cannotaccapt;

        Level = questData.condition.level;
        CompleteQuests = questData.condition.completeQuests;

        MonsterId = questData.monsterId;
        PurposeNumber = questData.purposeNumber;
        CurrentNumber = questData.currentNumber;

        Title = questData.title;
        Description = questData.description;
        Contents = questData.contents;

        foreach (QuestRewardData rewardData in questData.rewards)
            Rewards.Add(rewardData);

    }
}