using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [Tooltip("プレイヤーのレイヤーマスク")]
    public LayerMask playerMask;

    [Tooltip("カメラのスピード")]
    public float cameraSpeed = 1;

    [Tooltip("ジャンプのアルファ値")]
    [Range(0, 1)]
    public float jumpAlpha = 0.5f;

    [Tooltip("接続先のカメラトリガー")]
    public CameraTrigger connectionTrigger;

    [Tooltip("行先の方向の参照")]
    public MoveDirection go;

    [Tooltip("来る先の方向の参照")]
    public MoveDirection come;

    [Tooltip("ボックスコライダー2Dの参照")]
    public BoxCollider2D collider;

    LayerMask layerMask;

    private void Start()
    {
        //レイヤーマスクの文字列を取得し、レイヤーマスクに割り当てる
        string str = LayerMask.LayerToName(gameObject.layer);
        layerMask = LayerMask.GetMask(str);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawCube(transform.position, collider.bounds.size);

        //接続されているトリガーがあれば線を描画する
        if (connectionTrigger && connectionTrigger.go && connectionTrigger.come) 
        {
            Gizmos.DrawLine(transform.position, connectionTrigger.come.transform.position);
        }
    }
}
