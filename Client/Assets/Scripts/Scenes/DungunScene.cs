using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungunScene : BaseScene
{
    UI_GameScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Dungun;

        Managers.Map.LoadMap(2);

        Screen.SetResolution(960, 540, false);

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();

        StartCoroutine("aaa");

        Managers.CutScene = GameObject.Find("CutScene").GetComponent<CutSceneManager>();
        Managers.CutScene.Init();
    }

    IEnumerator aaa()
    {
        yield return new WaitForSeconds(5f);

        // 던전으로 왔따면 씬 전환 됐다는 패킷 전송
        C_GetInDungun dungunPacket = new C_GetInDungun();
        dungunPacket.Id = 2;
        Managers.Network.Send(dungunPacket);
        Debug.Log("SendGetDungun");
    }

    public override void Clear()
    {

    }
}
