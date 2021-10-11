using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region Skill

    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public List<skillPointInfo> skillPointInfos;
        public SkillType skillType;
        public string iconPath;
        public string description;
        public List<ConditionInfo> conditions;
        public projectileInfo projectile;
        public explosionInfo explosion;
        public summoningInfo summoning;

    }
    [Serializable]
    public class skillPointInfo
    {
        public float cooldown;
        public int damage;
        public int mp;
    }

    [Serializable]
    public class projectileInfo
    {
        public string name;
        public List<projectilePointInfo> projectilePointInfos;
        public string prefab;
    }
    [Serializable]
    public class projectilePointInfo
    {
        public float speed;
        public int range;
    }

    [Serializable]
    public class explosionInfo
    {
        public string name;
        public List<explosionPointInfo> explosionPointInfos;
    }
    [Serializable]
    public class explosionPointInfo
    {
        public int radian;
    }

    [Serializable]
    public class summoningInfo
    {
        public string name;
        public List<summoningPointInfo> summoningPointInfos;
        public string prefab;
    }
    [Serializable]
    public class summoningPointInfo
    {
        public int radian;
        public int duration;
    }



    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }

    #endregion

    #region Item

    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public string description;
        public ItemType itemType;
        public string iconPath;
    }

    [Serializable]
    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }

    [Serializable]
    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }

    [Serializable]
    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public PosionType posionType;
        public int maxCount;
    }

    [Serializable]
    public class CollectionData : ItemData
    {
        public CollectionType collectionType;
        public int maxCount;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<ArmorData> armors = new List<ArmorData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();
        public List<CollectionData> collections = new List<CollectionData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in weapons)
            {
                item.itemType = ItemType.Weapon;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in armors)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in collections)
            {
                item.itemType = ItemType.Collection;
                dict.Add(item.id, item);
            }

            return dict;
        }
    }

    #endregion

    #region Monster

    [Serializable]
    public class MonsterData
    {
        public int id;
        public string name;
        public StatInfo stat;
        public string prefabPath;
    }

    [Serializable]
    public class MonsterLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
                dict.Add(monster.id, monster);
            return dict;
        }
    }

    #endregion

    #region Npc

    [Serializable]
    public class NpcData
    {
        public int id;
        public string name;
        public string Description;
    }

    [Serializable]
    public class NpcLoader : ILoader<int, NpcData>
    {
        public List<NpcData> npcs = new List<NpcData>();

        public Dictionary<int, NpcData> MakeDict()
        {
            Dictionary<int, NpcData> dict = new Dictionary<int, NpcData>();
            foreach (NpcData npc in npcs)
                dict.Add(npc.id, npc);
            return dict;
        }
    }

    #endregion

    #region Quest

    [Serializable]
    public class QuestData
    {
        public int id;
        public int npcId;
        public QuestType questType;
        public Condition condition;
        public string title;
        public string description;
        public string completeDescription;
        public string contents;
        public List<QuestRewardData> rewards;
        public int cutSceneId;
    }
    [Serializable]
    public class Condition
    {
        public int level;
        public List<int> completeQuests;
    }
    [Serializable]
    public class QuestRewardData
    {
        public int exp;
        public int gold;
        public int itemId;
        public int count; // 얼마나
    }
    [Serializable]
    public class HuntingQuestData : QuestData
    {
        public int monsterId;
        public int purposeNumber;
        public int currentNumber;
    }
    [Serializable]
    public class CollectionQuestData : QuestData
    {
        public int itemId;
        public int purposeNumber;
        public int currentNumber;
    }
    [Serializable]
    public class CompleteQuestData : QuestData
    {
        public List<int> completeQuestIds;
    }

    [Serializable]
    public class QuestLoader : ILoader<int, QuestData>
    {
        public List<HuntingQuestData> hunting = new List<HuntingQuestData>();
        public List<CollectionQuestData> collection = new List<CollectionQuestData>();
        public List<CompleteQuestData> complete = new List<CompleteQuestData>();

        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData quest in hunting)
                dict.Add(quest.id, quest);
            foreach (QuestData quest in collection)
                dict.Add(quest.id, quest);
            foreach (QuestData quest in complete)
                dict.Add(quest.id, quest);
            return dict;
        }
    }

    #endregion

    #region Obstacle

    [Serializable]
    public class ObstacleData
    {
        public int id;
        public Vector2Int spawnPos;
        public List<Vector2Int> obstaclePos;
    }

    [Serializable]
    public class ObstacleLoader : ILoader<int, ObstacleData>
    {
        public List<ObstacleData> obstacle = new List<ObstacleData>();

        public Dictionary<int, ObstacleData> MakeDict()
        {
            Dictionary<int, ObstacleData> dict = new Dictionary<int, ObstacleData>();
            foreach (ObstacleData quest in obstacle)
                dict.Add(quest.id, quest);
            return dict;
        }
    }

    #endregion





    // ------------Local----------
    #region SkipDirector

    [Serializable]
    public class SkipDirectorData
    {
        public List<int> skipDirectors;
    }

    #endregion
}
