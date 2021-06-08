using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [TooltipAttribute("IgnoreColliderの大きさ")]
    public float ignoreSize = 5;

    [TooltipAttribute("コライダー2Dの参照")]
    public Collider2D collider2D;

    private LayerMask layerMask;
    private LayerMask playerMask;
    // Start is called before the first frame update
    void Start()
    {
        //レイヤーマスクを取得
        layerMask = 1 << gameObject.layer;
        playerMask = 1 << LayerMask.NameToLayer("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Ignore();
    }

    private void Ignore()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, ignoreSize, Vector2.zero, 0, layerMask);

        //指定のレイヤーマスクのオブジェクトに触れた場合、そのオブジェクトのコライダーをIgnoreさせる
        foreach (RaycastHit2D hit in hits)
        {
            if (!hit || hit.collider.tag == "Ground") break;

            if (hit.collider && collider2D)
            {
                Physics2D.IgnoreCollision(hit.collider, collider2D);
            }
        }

        //プレイヤーとヒットした場合も同様
        RaycastHit2D phit = Physics2D.CircleCast(transform.position, ignoreSize, Vector2.zero, 0, playerMask);

        if (!phit) return;

        Debug.Log(phit.collider.name);
        Physics2D.IgnoreCollision(phit.collider, collider2D);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, ignoreSize);
    }
}
