using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectServerPopup_Item : UI_Base
{
    public ServerInfo Info { get; set; }

    enum Buttons
    {
        SelectServerPopupButton
    }
    enum Texts
    {
        NameText,
        BusyScoreText
    }
    enum Images
    {
        Fill
    }
    enum Sliders
    {
        BusyScoreSlider
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Slider>(typeof(Sliders));

        BindEvent();
    }

    private void BindEvent()
    {
        BindEvent(Get<Button>((int)Buttons.SelectServerPopupButton).gameObject, (e) =>
        {
            Managers.Instance.ChangeScene(Info);
            Managers.UI.ClosePopupUI();

        }, Define.UIEvent.LeftClick);
    }

    public void RefreshUI()
    {
        if (Info == null)
            return;

        GetText((int)Texts.NameText).text = Info.Name;
        SetBusyScoreSlider();
    }

    private void SetBusyScoreSlider()
    {
        GetText((int)Texts.BusyScoreText).text = "혼잡도: " + Info.BusyScore;

        float ratio = 0.0f;
        if (Info.BusyScore > 0)
        {
            ratio = ((float)Info.BusyScore / 5);
        }

        // 혼잡도 색깔
        switch (Info.BusyScore)
        {
            case 0:
            case 1:
                // 초록
                Get<Image>((int)Images.Fill).color = new Color(0 / 255f, 255 / 255f, 0 / 255f);
                break;
            case 2:
            case 3:
                // 노랑
                Get<Image>((int)Images.Fill).color = new Color(255 / 255f, 255 / 255f, 0 / 255f);
                break;
            case 4:
            case 5:
                // 빨강
                Get<Image>((int)Images.Fill).color = new Color(255 / 255f, 0 / 255f, 0 / 255f);
                break;
            default:
                break;
        }

        Get<Slider>((int)Sliders.BusyScoreSlider).value = ratio;
    }

}
