using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatBar : UI_Base
{
    enum Sliders
    {
        HPSlider,
        MPSlider,
        EXSlider,
    }

    enum Texts
    {
        LevelText,
    }

    
    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<Text>(typeof(Texts));
    }

    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        Get<Slider>((int)Sliders.HPSlider).value = ratio;
    }

    public void SetMpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        Get<Slider>((int)Sliders.MPSlider).value = ratio;
    }

    public void SetExBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        Get<Slider>((int)Sliders.EXSlider).value = ratio;
    }

    public void SetLevel(int level)
    {
        Get<Text>((int)Texts.LevelText).text = level.ToString();
    }
}
