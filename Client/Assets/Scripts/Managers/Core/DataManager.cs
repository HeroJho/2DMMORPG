using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.Skill> SkillDict { get; private set; } = new Dictionary<int, Skill>();
    public Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, Data.MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
    public Dictionary<int, Data.QuestData> QuestDict { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, Data.ObstacleData> ObstacleDict { get; private set; } = new Dictionary<int, ObstacleData>();
    public Dictionary<int, Data.NpcData> NpcDict { get; private set; } = new Dictionary<int, NpcData>();
    public Dictionary<int, Data.DungunData> DungunDict { get; private set; } = new Dictionary<int, DungunData>();

    public SkipDirectorData SkipDirectorData { get; private set; } = new SkipDirectorData();

    public void LoadData()
    {
        SkillDict = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
        ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
        MonsterDict = LoadJson<Data.MonsterLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        QuestDict = LoadJson<Data.QuestLoader, int, Data.QuestData>("QuestData").MakeDict();
        ObstacleDict = LoadJson<Data.ObstacleLoader, int, Data.ObstacleData>("ObstacleData").MakeDict();
        NpcDict = LoadJson<Data.NpcLoader, int, Data.NpcData>("NpcData").MakeDict();
        DungunDict = LoadJson<Data.DungunLoader, int, Data.DungunData>("DungunData").MakeDict();


        SkipDirectorData = LoadLocalJson<Data.SkipDirectorData>("SkipDirectorData");
        
    }

    static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

    static Data LoadLocalJson<Data>(string path)
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Data>(textAsset.text);
    }


    public void WriteData<Data>(Data data, string path)
    {
        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.dataPath + $"/Resources/Data/{path}.Json", jsonData);
    }
}
