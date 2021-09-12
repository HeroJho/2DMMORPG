using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    // Owner
    public QuestGiver QuestGiver { get; set; }

    // Info
    public int QuestId { get; protected set; }
    public int NpcId { get; protected set; }
    public QuestType QuestType { get; protected set; }
    public QuestState QuestState { get; set; }

    // Condition
    public int Level { get; protected set; }
    public List<int> CompleteQuests { get; protected set; }

    // Descript
    public string Title { get; protected set; }
    public string Description { get; protected set; }
    public string Contents { get; protected set; }

    // Reward
    public List<QuestRewardData> Rewards { get; protected set; } = new List<QuestRewardData>();

}
