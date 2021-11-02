using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro _boxText;
    [SerializeField]
    private GameObject _box;

    private void Start()
    {
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);
    }

    private Coroutine _count = null;
    public void Write(string str)
    {
        if (_count != null)
        {
            StopCoroutine(_count);
            _count = null;
            transform.localPosition = Vector3.zero;
        }

        _boxText.text = str;

        float x = _boxText.preferredWidth;
        x = (x > 2) ? 2 : x + 0.3f;
        _box.transform.localScale = new Vector2(x, _boxText.preferredHeight + 0.3f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (_boxText.preferredHeight / 2) + 1.4f);
        
        gameObject.SetActive(true);
        _count = StartCoroutine("CountChatBox");
    }


    IEnumerator CountChatBox()
    {
        yield return new WaitForSeconds(3);
        _count = null;
        transform.localPosition = Vector3.zero;

        gameObject.SetActive(false);
    }


}
