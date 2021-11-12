using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossHp : UI_Base
{
    [SerializeField]
    Slider _hpBar;

    public override void Init()
    {
        
    }

    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.value = ratio;
    }
}
