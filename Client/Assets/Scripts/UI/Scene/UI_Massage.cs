using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Massage : UI_Base
{
    [SerializeField]
    private Text _text;

    private WaitForSeconds waitTime = new WaitForSeconds(2.0f);

    public override void Init()
    {
        
    }

    Coroutine _cor = null;
    public void WriteMassage(string str, bool isGreen)
    {
        // 카운트 중에는 모든 안내판 무시
        if (_countCor != null)
            return;

        if (_cor != null)
        {
            StopCoroutine(_cor);
            gameObject.SetActive(false);
            _cor = null;
        }

        if (isGreen)
            _text.color = new Color(90 / 255f, 245 / 255f, 125 / 255f, 200 / 255f);
        else
            _text.color = new Color(245 / 255f, 90 / 255f, 90 / 255f, 200 / 255f);

        _text.text = str;
        gameObject.SetActive(true);
        _cor = StartCoroutine("StartMassage");
    }
    IEnumerator StartMassage()
    {
        yield return waitTime;
        gameObject.SetActive(false);
    }

    Coroutine _countCor = null;
    public void WriteCount(bool isGreen)
    {
        if (_countCor != null)
            return;

        if (_cor != null)
        {
            StopCoroutine(_cor);
            gameObject.SetActive(false);
            _cor = null;
        }

        if (isGreen)
            _text.color = new Color(90 / 255f, 245 / 255f, 125 / 255f, 200 / 255f);
        else
            _text.color = new Color(245 / 255f, 90 / 255f, 90 / 255f, 200 / 255f);

        gameObject.SetActive(true);
        _countCor = StartCoroutine("StartCount");
    }
    IEnumerator StartCount()
    {
        int count = 10;
        while (count > 0)
        {
            _text.text = "곧 귀환 합니다! \n" + count + "초";
            yield return new WaitForSeconds(1f);
            count--;
        }
        gameObject.SetActive(false);
        _countCor = null;
    }
}
