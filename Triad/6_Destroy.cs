using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一定時間経過すると破棄される
/// </summary>
public class Destroy : MonoBehaviour
{
    /// <summary>
    /// 破棄されるまでの時間
    /// </summary>
    [Tooltip("破棄されるまでの時間")]
    public float time = 1;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, time);
    }
}
