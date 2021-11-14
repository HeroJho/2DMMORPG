using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<int, StatInfo> StatDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static Dictionary<int, Data.Skill> SkillDict { get; private set; } = new Dictionary<int, Skill>();
        public static Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();
        public static Dictionary<int, Data.MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
        public static Dictionary<int, Data.NpcData> NpcDict { get; private set; } = new Dictionary<int, NpcData>();
        public static Dictionary<int, Data.QuestData> QuestDict { get; private set; } = new Dictionary<int, QuestData>();
        public static Dictionary<int, Data.ObstacleData> ObstacleDict { get; private set; } = new Dictionary<int, ObstacleData>();
        public static Dictionary<int, Data.DungunData> DungunDict { get; private set; } = new Dictionary<int, DungunData>();


        public static void LoadData()
        {
            StatDict = LoadJson<Data.StatData, int, StatInfo>("StatData").MakeDict();
            SkillDict = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
            ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
            MonsterDict = LoadJson<Data.MonsterLoader, int, Data.MonsterData>("MonsterData").MakeDict();
            NpcDict = LoadJson<Data.NpcLoader, int, Data.NpcData>("NpcData").MakeDict();
            QuestDict = LoadJson<Data.QuestLoader, int, Data.QuestData>("QuestData").MakeDict();
            ObstacleDict = LoadJson<Data.ObstacleLoader, int, Data.ObstacleData>("ObstacleData").MakeDict();
            DungunDict = LoadJson<Data.DungunLoader, int, Data.DungunData>("DungunData").MakeDict();

        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
