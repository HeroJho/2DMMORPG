using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    InventoryManager _inven = new InventoryManager();
    NetworkManager _network = new NetworkManager();
    MapManager _map = new MapManager();
    ObjectManager _obj = new ObjectManager();
    SkillManager _skill = new SkillManager();
    QuestManager _quest = new QuestManager();
    CutSceneManager _cutScene;
    CommunicationManager _communication = new CommunicationManager();
    ChatManager _chat = new ChatManager();


    public static InventoryManager Inven { get { return Instance._inven; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static MapManager Map { get { return Instance._map; } }
    public static ObjectManager Object { get { return Instance._obj; } }
    public static SkillManager Skill { get { return Instance._skill; } }
    public static QuestManager Quest { get { return Instance._quest; } }
    public static CutSceneManager CutScene { get { return Instance._cutScene; } }
    public static CommunicationManager Communication { get { return Instance._communication; } }
    public static ChatManager Chat { get { return Instance._chat; } }

    #endregion

    #region Core
    DataManager _data = new DataManager();
    ResourceManager _resource = new ResourceManager();
    UIManager _ui = new UIManager();


    public static DataManager Data { get { return Instance._data; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static UIManager UI { get { return Instance._ui; } }
    #endregion

    void Start()
    {
        Init();
        _cutScene = GameObject.Find("CutScene").GetComponent<CutSceneManager>();

        CutScene.Init();
    }

    void Update()
    {
        _network.Update();
    }

    static void Init()
    {
        if(s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._network.Init();
            Data.LoadData();

        }
    }

    public static void Clear()
    {
        UI.Clear();
    }
}
