using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScenePos : MonoBehaviour
{
    [SerializeField]
    private int CutSceneNum;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Managers.CutScene.StartCutScene(CutSceneNum, true);
    }
}
