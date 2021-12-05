using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    public string BaseUrl { get; set; } = "https://localhost:5001/api";

    public void SendPostRequest<T>(string url, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, res));
    }

    public void SendPost(string url, object obj)
    {
        Managers.Instance.StartCoroutine(CoSendWeb(url, UnityWebRequest.kHttpVerbPOST, obj));
    }

    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> res)
    {
        UI_LoginScene loginScene = Managers.UI.SceneUI as UI_LoginScene;

        string sendUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;
        if(obj != null)
        {
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            // 웹 서버에 obj의 json을 upload
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            // 웹 서버의 응답을 받을 버퍼(text형태로 json으로 온다)
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            // 서버가 다 처리를 하고 응답이 올 때 까지 yield 기다린다.
            loginScene.MassageUI.WriteMassage("잠시만 기다려주세요...", true);
            yield return uwr.SendWebRequest();

            if(uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
                loginScene.MassageUI.WriteMassage("서버와 연결이 원활하지 않습니다!", false);
            }
            else
            {
                // json형태로 온 데이터를 T형으로 변환
                T resObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(uwr.downloadHandler.text);
                res.Invoke(resObj);
            }

        }

    }
    IEnumerator CoSendWeb(string url, string method, object obj)
    {
        UI_LoginScene loginScene = Managers.UI.SceneUI as UI_LoginScene;

        string sendUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;
        if (obj != null)
        {
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            // 웹 서버에 obj의 json을 upload
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            // 웹 서버의 응답을 받을 버퍼(text형태로 json으로 온다)
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();
        }
    }
}
