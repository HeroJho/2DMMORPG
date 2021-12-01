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

    public UI_SceneChange ChangeUI { get; private set; }
    public UI_Massage MassageUI { get; private set; }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));

        BindEvent();

        ChangeUI = GetComponentInChildren<UI_SceneChange>();
        MassageUI = GetComponentInChildren<UI_Massage>();

        MassageUI.gameObject.SetActive(false);
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
            if(res.CreateOk)
                MassageUI.WriteMassage("계정을 만들었습니다!", true);
            else
                MassageUI.WriteMassage("중복된 계정입니다!", false);

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

            // 로그인에 성공하면 서버 선택창 이동
            if (res.LoginOk)
            {
                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;

                UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                popup.SetServers(res.ServerList);
            }
            else
            {
                MassageUI.WriteMassage("로그인에 실패했습니다!\n아이디와 비밀번호를 확인해 주세요.", false);
            }

        });
    }

}
