using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// サーバー開設やルーム作成を行うためのアドレス保持クラス
/// </summary>
public class Address : MonoBehaviour
{
    /// <summary>
    /// IPアドレス
    /// </summary>
    public static string ipAddress;

    /// <summary>
    /// ルームコード
    /// </summary>
    public static string roomCode;

    /// <summary>
    /// ホストかサーバーか（ホスト＝true）
    /// </summary>
    public static bool host = true;

    /// <summary>
    /// IPアドレスとルームコードをセットする（ipAddress+" "+roomCode）
    /// </summary>
    public static void SetAddress(string str)
    {
        string[] strs = str.Split(' ');
        ipAddress = strs[0];
        roomCode = strs[1];
    }

    /// <summary>
    /// ホストをセットする
    /// </summary>
    public static void SetHost(bool h)
    {
        host = h;
    }
}
