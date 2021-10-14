using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Buff : UI_Base
{
    [SerializeField]
    private Image _cover;
    public Image Image { get; set; }

    public override void Init()
    {
        Image = GetComponent<Image>();
        _cover.fillAmount = 0;
    }

    public void SetRatio(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _cover.fillAmount = ratio;
    }
}
