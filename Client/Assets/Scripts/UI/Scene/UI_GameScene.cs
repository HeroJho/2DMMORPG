using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; private set; }
    public UI_Inventory InvenUI { get; private set; }
    public UI_StatBar StatBarUI { get; private set; }

    public override void Init()
    {
        base.Init();

        StatUI = GetComponentInChildren<UI_Stat>();
        InvenUI = GetComponentInChildren<UI_Inventory>();
        StatBarUI = GetComponentInChildren<UI_StatBar>();

        StatUI.gameObject.SetActive(false);
        InvenUI.gameObject.SetActive(false);
    }
}
