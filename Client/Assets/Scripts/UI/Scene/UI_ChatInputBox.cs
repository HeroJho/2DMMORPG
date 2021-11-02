using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatInputBox : UI_Base
{
    private InputField _inputField;
    public InputField GetInputField { get { return _inputField; } }

    public override void Init()
    {
        _inputField = GetComponent<InputField>();
    }

    public string GetChat()
    {
        string str = _inputField.text;
        _inputField.text = null;
        return str;
    }


}
