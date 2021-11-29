using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    enum GameObjects
    {
        Input_ID,
        Input_Password
    }

    enum Buttons
    {
        CreateButton,
        LoginButton
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));

        BindEvent();
    }

    public void BindEvent()
    {
        // 오른 클릭시 장착패킷
        BindEvent(Get<Button>((int)Buttons.CreateButton).gameObject, OnClickCreateButton, Define.UIEvent.LeftClick);
        BindEvent(Get<Button>((int)Buttons.LoginButton).gameObject, OnClickLoginButton, Define.UIEvent.LeftClick);
    }

    public void OnClickCreateButton(PointerEventData evt)
    {
        string account = Get<GameObject>((int)GameObjects.Input_ID).GetComponent<InputField>().text;
        string password = Get<GameObject>((int)GameObjects.Input_Password).GetComponent<InputField>().text;


        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
        {
            Debug.Log(res.CreateOk);

            Get<GameObject>((int)GameObjects.Input_ID).GetComponent<InputField>().text = "";
            Get<GameObject>((int)GameObjects.Input_Password).GetComponent<InputField>().text = "";

        });
    }

    public void OnClickLoginButton(PointerEventData evt)
    {
        string account = Get<GameObject>((int)GameObjects.Input_ID).GetComponent<InputField>().text;
        string password = Get<GameObject>((int)GameObjects.Input_Password).GetComponent<InputField>().text;

        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
        {
            Debug.Log(res.LoginOk);
            Get<GameObject>((int)GameObjects.Input_ID).GetComponent<InputField>().text = "";
            Get<GameObject>((int)GameObjects.Input_Password).GetComponent<InputField>().text = "";

            // 로그인에 성공하면 씬이동
            if (res.LoginOk)
            {
                // 서버와 연결
                Managers.Network.ConnectToGame();
                Managers.Scene.LoadScene(Define.Scene.Game);
            }

        });
    }

}
