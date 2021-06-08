using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("PlaneOptions")]
    [TooltipAttribute("茎の長さの初期値")]
    public int defaultLenght = 1;
    [TooltipAttribute("茎の長さの最大値")]
    public int maxLenght = 5;

    [Header("CollisionOptions")]
    [TooltipAttribute("プレイヤーのレイヤーマスク")]
    public LayerMask layerMask;

    [Header("Reference")]
    [TooltipAttribute("茎のプレハブ")]
    public GameObject stem;

    List<GameObject> stems;

    Vector2 size;
    Vector2 colSize;
    Vector2 colOffset;
    Player player;

    // Start is called before the first frame update
    void Start()
    {
        //サイズを取得する
        size = stem.GetComponent<SpriteRenderer>().bounds.size;
        colSize = size;
        stems = new List<GameObject>();

        //茎を成長させる
        ApplyGrowth(defaultLenght);
    }

    void Update()
    {
        Vector3 center = transform.position + new Vector3(colOffset.x, colOffset.y, 0);
        float angle = Vector3.Angle(transform.position, stems[0].transform.position);
        RaycastHit2D hit = Physics2D.BoxCast(center, colSize, angle, Vector2.zero, 0, layerMask);

        if (hit)
        {
            Player p = hit.collider.gameObject.GetComponent<Player>();

            //ヒットしている場合は奥行を判定する
            if (!p.CompareLayerAndColliderMask(gameObject.layer)) 
            {
                return;
            }
            if (!player)
            {
                player = hit.collider.GetComponent<Player>();
                if (!player.controller.IsGrounded)
                {
                    player.Climb();
                }
                else
                {
                    player.ResetClimb();
                    player = null;
                }
            }
        }
        else if (player) 
        {
            player.ResetClimb();
            player = null;
        }
    }

    /// <summary>
    /// Plantを成長させる関数
    /// <param name="lenght">成長させる茎の長さ</param>
    /// </summary>
    public void ApplyGrowth(int lenght)
    {
        for(int i = 0; i < lenght; i++)
        {
            if (stems.Count >= maxLenght) break;

            //茎のプレハブをインスタンス化する
            GameObject s = Instantiate(stem, transform);
            s.transform.localPosition = new Vector3(0, -stems.Count * size.y, 0);
            s.layer = gameObject.layer;
            stems.Add(s);
            UpdateCollider();
        }
    }

    void UpdateCollider()
    {
        colSize.y = stems.Count * size.y;
        colOffset.y = -(stems.Count - 1) * size.y / 2;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "plant");
    }
}

