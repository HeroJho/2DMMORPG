using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill : UI_Base
{
    enum Images
    {
        DragBar,
    }

    enum Buttons
    {
        Button_X
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));


        BindEvent();

    }

    void BindEvent()
    {
        // 드레그로 창 이동
        BindEvent(Get<Image>((int)Images.DragBar).gameObject, (e) =>
        {
            gameObject.transform.position = e.position;

        }, Define.UIEvent.Drag);

        // X버튼 누르면 끈다
        BindEvent(Get<Button>((int)Buttons.Button_X).gameObject, (e) =>
        {
            gameObject.SetActive(false);

        }, Define.UIEvent.LeftClick);

    }

}
