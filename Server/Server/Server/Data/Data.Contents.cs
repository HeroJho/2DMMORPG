using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    #region Stat

    [Serializable]
    public class StatData : ILoader<int, StatInfo>
    {
        public List<StatInfo> stats = new List<StatInfo>();

        public Dictionary<int, StatInfo> MakeDict()
        {
            Dictionary<int, StatInfo> dict = new Dictionary<int, StatInfo>();
            foreach (StatInfo stat in stats)
            {
                stat.Hp = stat.MaxHp;
                stat.Mp = stat.MaxMp;
                dict.Add(stat.Level, stat);
            }
            return dict;
        }
    }

    #endregion

    #region Skill

    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public int mp;
        public SkillType skillType;
        public projectileInfo projectile;
        public explosionInfo explosion;
    }

    public class projectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefab;
    }

    public class explosionInfo
    {
        public string name;
        public int radian;
        public string prefab;
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
        public ItemType itemType;
    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }

    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }

    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public PosionType posionType;
        public int maxCount;
        public int recovery;
    }

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
        public List<RewardData> rewards;
    }

    public class RewardData
    {
        public int probability; // 100분율
        public int itemId; // 어떤 아이템을
        public int count; // 몇개를
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
        public int x;
        public int y;
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
        public List<QuestRewardData> rewards;
    }

    public class Condition
    {
        public int level;
        public List<int> completeQuests;
    }

    public class QuestRewardData
    {
        public int exp;
        public int gold;
        public int itemId;
        public int count; // 얼마나
    }

    public class HuntingQuestData : QuestData
    {
        public int monsterId;
        public int purposeNumber;
        public int currentNumber;
    }

    public class CollectionQuestData : QuestData
    {
        public int itemId;
        public int purposeNumber;
        public int currentNumber;
    }

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

}
