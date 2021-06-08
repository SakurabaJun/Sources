using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    [Tooltip("サイズ")]
    public Vector2 size = Vector2.one;

    [Tooltip("調整オフセット")]
    public float offset;

    [Tooltip("壊れるまでの時間")]
    public float breakTime = 0.5f;

    [Tooltip("当たり判定用レイヤーマスク")]
    public LayerMask layerMask;

    [TooltipAttribute("スケルトンアニメーションの参照（Spine）")]
    public Spine.Unity.SkeletonAnimation skeletonAnimation;

    [TooltipAttribute("ポリゴンコライダー2Dの参照")]
    public PolygonCollider2D collider;

    Vector2 center;

    bool isBreak = false;

    float currentTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        center = new Vector2(transform.position.x, transform.position.y + offset);
    }

    // Update is called once per frame
    void Update()
    {
        //BOXキャストをして、ヒットしているプレイヤーを検出
        RaycastHit2D hit = Physics2D.BoxCast(center, size, 0, Vector2.zero, 0, layerMask);

        if (!hit) return;

        Player player = hit.collider.GetComponent<Player>();
        if (!player.CompareLayerAndColliderMask(gameObject.layer)) return;

        skeletonAnimation.timeScale = 1;
        isBreak = true;

        //壊れていたらcurrentTimeにdeltaTimeを追加していく。
        if (isBreak)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= breakTime) 
            {
                //コライダーを無効にする
                collider.enabled = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        center = new Vector2(transform.position.x, transform.position.y + offset);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, size);
    }
}
