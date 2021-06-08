using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LogDisplay : MonoBehaviour
{
    [SerializeField] int m_MaxLogCount = 20;

    [SerializeField] Rect m_Area = new Rect(220, 0, 400, 400);

    Queue<string> m_LogMessages = new Queue<string>();

    StringBuilder m_StringBuilder = new StringBuilder();

    void Start()
    {
        // ログが出力される際に呼んでくれる
        Application.logMessageReceived += LogReceived;
    }

    void LogReceived(string text, string stackTrace, LogType type)
    {
        m_LogMessages.Enqueue(text);

        while (m_LogMessages.Count > m_MaxLogCount)
        {
            m_LogMessages.Dequeue();
        }
    }

    void OnGUI()
    {
        GUIStyle myStyle = new GUIStyle();
        myStyle.fontSize = 50;

        m_StringBuilder.Length = 0;

        foreach (string s in m_LogMessages)
        {
            m_StringBuilder.Append(s).Append(System.Environment.NewLine);
        }

        //画面に表示
        GUI.Label(m_Area, m_StringBuilder.ToString(), myStyle);
    }
}