using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MpBar : MonoBehaviour
{
    [SerializeField]
    Transform _mpBar = null;

    public void SetMpBar(float ratio)
    {
        // 0보다 작으면 0, 1보다 크면 1 반환
        ratio = Mathf.Clamp(ratio, 0, 1);
        _mpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
