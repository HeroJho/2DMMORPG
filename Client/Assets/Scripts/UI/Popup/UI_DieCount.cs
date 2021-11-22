using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DieCount : UI_Popup
{
    enum Buttons
    {
        SpawnButton
    }

    enum Texts
    {
        CountText
    }

    Coroutine _count = null;
    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        BindEvent();

        _count = StartCoroutine("StartCount");
    }

    void BindEvent()
    {
        // ReSpawnButton
        BindEvent(Get<Button>((int)Buttons.SpawnButton).gameObject, (e) =>
        {
            MyPlayerController mpc = Managers.Object.MyPlayer;

            C_Respawn respawnPacket = new C_Respawn();
            respawnPacket.PlayerId = mpc.Id;
            Managers.Network.Send(respawnPacket);

            if(_count != null)
            {
                StopCoroutine(_count);
                _count = null;
            }

            Managers.UI.ClosePopupUI(this);
        });

    }

    IEnumerator StartCount()
    {
        int count = 10;
        while (count >= 0)
        {
            Get<Text>((int)Texts.CountText).text = "..." + count--;
            yield return new WaitForSeconds(1f);
        }

        _count = null;
        Managers.UI.ClosePopupUI(this);
    }

}
