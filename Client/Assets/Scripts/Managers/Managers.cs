using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static public Managers Instance { get { Init(); return s_instance; } }

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
    WebManager _web = new WebManager();


    public static InventoryManager Inven { get { return Instance._inven; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static MapManager Map { get { return Instance._map; } }
    public static ObjectManager Object { get { return Instance._obj; } }
    public static SkillManager Skill { get { return Instance._skill; } }
    public static QuestManager Quest { get { return Instance._quest; } }
    public static CutSceneManager CutScene { get { return Instance._cutScene; } set { Instance._cutScene = value; } }
    public static CommunicationManager Communication { get { return Instance._communication; } }
    public static ChatManager Chat { get { return Instance._chat; } }
    public static WebManager Web { get { return Instance._web; } }

    #endregion

    #region Core
    DataManager _data = new DataManager();
    ResourceManager _resource = new ResourceManager();
    UIManager _ui = new UIManager();
    SceneManagerEx _scene = new SceneManagerEx();


    public static DataManager Data { get { return Instance._data; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    #endregion

    void Start()
    {
        GameObject cutobj = GameObject.Find("CutScene");
        if(cutobj != null)
        {
            _cutScene = cutobj.GetComponent<CutSceneManager>();
            CutScene.Init();
        }

        Init();
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

            Data.LoadData();

        }
    }

    public static void Clear()
    {
        UI.Clear();
    }

    public void ChangeScene(ServerInfo info)
    {
        StartCoroutine("Change", info);
    }
    IEnumerator Change(ServerInfo info)
    {
        UI_LoginScene loginScene = UI.SceneUI as UI_LoginScene;
        loginScene.ChangeUI.PadeIns();
        loginScene.MassageUI.WriteMassage("접속 중입니다...", true);

        yield return new WaitForSeconds(2f);

        Network.ConnectToGame(info);
        Scene.LoadScene(Define.Scene.Game);
    }
}
