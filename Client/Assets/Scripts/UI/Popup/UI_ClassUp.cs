using System.Collections;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ClassUp : UI_Popup
{
    enum Images
    {
        Class_Warrior,
        Class_Mage
    }
    enum Buttons
    {
        XButton
    }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        BindEvent();
    }

    void BindEvent()
    {
        BindEvent(Get<Image>((int)Images.Class_Warrior).gameObject, (e) =>
        {
            if (Managers.Object.MyPlayer.Stat.CanUpClass == false)
                return;

            C_ClassUp classUpPacket = new C_ClassUp();
            classUpPacket.ClassType = JobClassType.Warrior;

        }, Define.UIEvent.LeftClick);
        BindEvent(Get<Image>((int)Images.Class_Mage).gameObject, (e) =>
        {
            if (Managers.Object.MyPlayer.Stat.CanUpClass == false)
                return;

            C_ClassUp classUpPacket = new C_ClassUp();
            classUpPacket.ClassType = JobClassType.Mage;

        }, Define.UIEvent.LeftClick);

        // X버튼
        BindEvent(Get<Button>((int)Buttons.XButton).gameObject, (e) =>
        {
            Managers.UI.ClosePopupUI(this);

        }, Define.UIEvent.LeftClick);

    }
}
