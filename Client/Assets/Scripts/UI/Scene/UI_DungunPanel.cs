using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DungunPanel : UI_Base
{
    enum Texts
    {
        NameText,
    }
    enum Buttons
    {
        XButton,
        GetInButton
    }
    enum Images
    {
        
    }

    int _dungunId;

    bool _init = false;
    public override void Init()
    {
        if (_init)
            return;

        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        BindEvent();

        _init = true;
    }

    public void OnPanel(DungunData data)
    {
        if (_init == false)
            Init();

        _dungunId = data.npcId;
        Get<Text>((int)Texts.NameText).text = data.name;
        gameObject.SetActive(true);
    }

    void BindEvent()
    {

        BindEvent(Get<Button>((int)Buttons.XButton).gameObject, (e) =>
        {
            gameObject.SetActive(false);
            Get<Text>((int)Texts.NameText).text = null;

        }, Define.UIEvent.LeftClick);

        BindEvent(Get<Button>((int)Buttons.GetInButton).gameObject, (e) =>
        {
            C_TryGetInDungun tryDungunPacket = new C_TryGetInDungun();
            tryDungunPacket.Id = _dungunId;

            Managers.Network.Send(tryDungunPacket);

        }, Define.UIEvent.LeftClick);

    }
}
