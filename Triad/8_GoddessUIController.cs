using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoddessUIController : MonoBehaviour
{
    [Tooltip("プレイヤーへの指示を表示するテキスト")]
    [SerializeField] Text instruction;

    [Tooltip("時間を表示するテキスト")]
    [SerializeField] Text time;

    [Tooltip("ゲームオーバーオブジェクト")]
    [SerializeField] GameObject gameOver;

    /// <summary>
    /// 指示テキストに文字列をセットする
    /// </summary>
    public void SetInstruction(string str)
    {
        instruction.text = str;
    }

    /// <summary>
    /// 時間テキストに数値をセットする
    /// </summary>
    public void SetTime(int t)
    {
        if (time) time.text = t.ToString();
    }

    /// <summary>
    /// 時間を表示するテキストを表示または非表示にする
    /// </summary>
    /// <param name="b">True＝表示</param>
    public void SetActiveTime(bool b)
    {
        if (time) time.gameObject.SetActive(b);
    }

    /// <summary>
    /// ゲームオーバーを表示または非表示にする
    /// </summary>
    /// <param name="b">True＝表示</param>
    public void SetActiverGameOver(bool b)
    {
        gameOver.SetActive(b);
    }
}
