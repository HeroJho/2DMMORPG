using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopGiver : MonoBehaviour
{
    [SerializeField]
    private int _npcId;

    public void GetPanelInfo()
    {
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene.ShopPanelUI.RefreshUI(_npcId);
        gameScene.ShopPanelUI.gameObject.SetActive(true);
    }

}
