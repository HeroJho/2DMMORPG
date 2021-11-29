using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    UI_LoginScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        Managers.Web.BaseUrl = "https://localhost:5001/api";

        Screen.SetResolution(960, 540, false);

        _sceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>();
    }

    public override void Clear()
    {
        
    }

}
