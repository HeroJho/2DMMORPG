using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Die : UI_Popup
{
    enum Buttons
    {
        SpawnButton
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));

        BindEvent();
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

            Managers.UI.ClosePopupUI(this);
        });
    }

}
