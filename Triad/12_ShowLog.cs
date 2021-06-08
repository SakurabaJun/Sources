using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLog : MonoBehaviour {


    GameObject scroll;

    Transform content;

    GameObject prefab;

    void Start()
    {
        //ログ出力をのイベントに追加する
        Application.logMessageReceived += OnLogMessage;
        prefab = (GameObject)Resources.Load("Log/Text");
        scroll = transform.GetChild(0).gameObject;
        content = scroll.transform.GetChild(0).GetChild(0);
    }

    private void OnLogMessage(string i_logText, string i_stackTrace, LogType i_type)
    {
        if (string.IsNullOrEmpty(i_logText))
        {
            return;
        }
        GameObject text = Instantiate(prefab, content);
        text.GetComponent<Text>().text = i_logText;

    }

    public void Show()
    {
        scroll.SetActive(!scroll.active);
    }
}
