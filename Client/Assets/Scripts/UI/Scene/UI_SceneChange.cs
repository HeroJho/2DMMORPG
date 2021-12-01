using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SceneChange : UI_Base
{
    Image _image = null;

    public override void Init()
    {
        _image = GetComponent<Image>();
    }

    public void ChangeRoom()
    {
        StartCoroutine("PadeIn");
    }

    public void ArrivedRoom()
    {
        StartCoroutine("PadeOut");
    }

    public void PadeIns()
    {
        StartCoroutine("PadeInNoChange");
    }

    IEnumerator PadeIn()
    {
        float a = 0;
        while (a <= 1.0f)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, a += 0.1f);

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2f);

        if(Managers.Scene.CurrentScene is GameScene)
            Managers.Scene.LoadScene(Define.Scene.Dungun);
        else
            Managers.Scene.LoadScene(Define.Scene.Game);
    }

    IEnumerator PadeOut()
    {
        float a = 1.0f;
        while (a >= 0)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, a -= 0.1f);

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator PadeInNoChange()
    {
        float a = 0;
        while (a <= 1.0f)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, a += 0.1f);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
