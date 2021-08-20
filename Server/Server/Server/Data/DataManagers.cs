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

        public static void LoadData()
        {
            StatDict = LoadJson<Data.StatData, int, StatInfo>("StatData").MakeDict();
            SkillDict = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
            ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
            MonsterDict = LoadJson<Data.MonsterLoader, int, Data.MonsterData>("MonsterData").MakeDict();

        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
