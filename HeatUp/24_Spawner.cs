using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public LayerMask playerMask;
    public List<SpawnArea> spawnArea;
    // Start is called before the first frame update

    //エディター内だけの処理
    void Reset()
    {
        //自分の子供に含まれているスポーンエリアを検索する
        spawnArea = new List<SpawnArea>();
        int lenght = transform.childCount;
        for (int i = 0; i < lenght; i++)
        {
            SpawnArea s = transform.GetChild(i).GetComponent<SpawnArea>();
            spawnArea.Add(s);
        }
    }

    private bool spawn = false;
    private void Update()
    {
        if(spawn) return;

        //playerがhitしたかを確認する
        RaycastHit2D hit = Physics2D.BoxCast(transform.position,
            transform.localScale, 0, Vector2.zero, 0, playerMask);

        if (!hit) return;

        //レイヤーマスクと比較する（奥行判定）
        LayerMask collisionMask = hit.collider.GetComponent<Player>().controller.collisionMask;
        string str = LayerMask.LayerToName(gameObject.layer);
        LayerMask myLayer = LayerMask.GetMask(str);
        if (collisionMask == myLayer)
        {
            foreach (SpawnArea s in spawnArea)
            {
                s.Spawn();
            }
            spawn = true;
        }
    }

    void OnDrawGizmos()
    {
        Color c = Gizmos.color;
        c = Color.cyan;
        c.a = 0.3f;
        Gizmos.color = c;
        Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x, transform.localScale.y));
        spawnArea = new List<SpawnArea>();
        int lenght = transform.childCount;
        for (int i = 0; i < lenght; i++)
        {
            SpawnArea s = transform.GetChild(i).GetComponent<SpawnArea>();
            spawnArea.Add(s);
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < lenght; i++)
        {
            Gizmos.DrawLine(transform.position, spawnArea[i].transform.position);
        }
    }
}
