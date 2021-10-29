using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InteractionPanel : UI_Base
{
    enum Texts
    {
        NameText
    }
    enum Buttons
    {
        XButton,
        InvitePartyButton
    }

    PlayerController _pc = null;

    public override void Init()
    {
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        BindEvent();
    }

    void BindEvent()
    {
        BindEvent(Get<Button>((int)Buttons.XButton).gameObject, (e) =>
        {
            gameObject.SetActive(false);
            Get<Text>((int)Texts.NameText).text = null;

        }, Define.UIEvent.LeftClick);

        BindEvent(Get<Button>((int)Buttons.InvitePartyButton).gameObject, (e) =>
        {
            if (_pc == null)
                return;

            C_InvitePlayer invitePlayerPacket = new C_InvitePlayer();
            invitePlayerPacket.PlayerId = _pc.GetComponent<PlayerController>().Id;
            Managers.Network.Send(invitePlayerPacket);

        }, Define.UIEvent.LeftClick);

    }

    public void SetPlayerInfo(PlayerController pc)
    {
        _pc = pc;
        if (_pc == null)
            return;

        gameObject.SetActive(true);

        Get<Text>((int)Texts.NameText).text = _pc.Name;
    }

}
