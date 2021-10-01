using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CanClassUp : UI_Scene
{

    public override void Init()
    {
        BindEvent();
    }

    private void BindEvent()
    {

        BindEvent(gameObject, (e) =>
        {
            if (Managers.Object.MyPlayer.Stat.CanUpClass == false)
                return;

            Managers.UI.ShowPopupUI<UI_ClassUp>();

        }, Define.UIEvent.LeftClick);

    }
}
