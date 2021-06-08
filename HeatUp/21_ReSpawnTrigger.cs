using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReSpawnTrigger : MonoBehaviour
{
    [TooltipAttribute("プレイヤーのレイヤーマスク")]
    public LayerMask layerMask;

    ReSpawn reSpawn;
    LayerMask targetMask;

    // Start is called before the first frame update
    void Start()
    {
        string str = LayerMask.LayerToName(gameObject.layer);
        targetMask = LayerMask.GetMask(str);
        reSpawn = GameObject.Find("ReSpawn").GetComponent<ReSpawn>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.zero, 0, layerMask);
        if (!hit) return;

        Player player = hit.collider.GetComponent<Player>();

        if (!player || !player.CompareLayerAndColliderMask(gameObject.layer)) return;

        reSpawn.reSpawnTrigger = this;
    }

    void OnDrawGizmos()
    {
        Color color = Color.yellow;
        color.a = 0.3f;
        Gizmos.color = color;

        Vector2 size = transform.localScale;
        Gizmos.DrawCube(transform.position, size);
    }
}
