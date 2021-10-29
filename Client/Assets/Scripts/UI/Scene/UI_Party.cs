using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Party : UI_Base
{
    public PlayerController Pc { get; set; }

    [SerializeField]
    private Text _name;
    [SerializeField]
    private Slider _hpBar;
    [SerializeField]
    private Slider _mpBar;

    public override void Init()
    {
        
    }




}
