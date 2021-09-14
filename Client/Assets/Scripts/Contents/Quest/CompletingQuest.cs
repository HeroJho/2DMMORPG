using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompletingQuest : Quest
{
    public List<int> CompleteQuestIds { get; private set; } = new List<int>();
    public List<int> CompletedQuestIds { get; private set; } = new List<int>();

    public void Init(CompleteQuestData questData)
    {
        QuestId = questData.id;
        NpcId = questData.npcId;
        QuestType = questData.questType;
        QuestState = QuestState.Cannotaccapt;

        Level = questData.condition.level;
        CompleteQuests = questData.condition.completeQuests;

        foreach (int id in questData.completeQuestIds)
        {
            CompleteQuestIds.Add(id);
        }

        Title = questData.title;
        Description = questData.description;
        Contents = questData.contents;

        foreach (QuestRewardData rewardData in questData.rewards)
            Rewards.Add(rewardData);

    }

    public void RefreshQuest()
    {
        foreach (int id in CompleteQuestIds)
        {
            Quest tempQuest = null;
            Managers.Quest.CompletedQuests.TryGetValue(id, out tempQuest);
            if (tempQuest == null)
                continue;

            if (CompletedQuestIds.Contains(id))
                continue;

            CompletedQuestIds.Add(id);
        }

    }
}
