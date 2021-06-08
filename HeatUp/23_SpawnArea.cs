using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [TooltipAttribute("スポーンさせるオブジェクト")]
    public GameObject spawnObject;

    [TooltipAttribute("スポーンさせる総量")]
    public int totalAmount = 1;

    [TooltipAttribute("間隔を指定する場合0以外の数字を入力")]
    public float interval = 0;

    [TooltipAttribute("intervalが0以上の場合一間隔につき何個スポーンさせるか")]
    [Min(1)]
    public int amount = 1;

    [TooltipAttribute("スポーントリガーの参照")]
    public GameObject parent;

    float currentTime = 0;
    int count = 0;
    bool isInterval = false;

    void Update()
    {
        if (isInterval) return;

        currentTime += Time.deltaTime;

        //intervalが指定されている場合はその秒数ごとにスポーンする
        if (currentTime > interval)
        {
            //いっぺんにスポーンさせる
            for (int i = 0; i < amount; i++)
            {
                OneSpwan();
                count++;
                if (count >= totalAmount)
                {
                    Destroy(parent);
                }
            }
            currentTime = 0;
        }
    }

    /// <summary>
    /// スポーンさせる
    /// </summary>
    public void Spawn()
    {
        if (interval == 0) 
        {
            for (int i = 0; i < totalAmount; i++)
            {
                OneSpwan();
            }
            Destroy(parent);
        }
        else
        {
            isInterval = true;
        }
    }

    void OneSpwan()
    {
        Vector3 pos = transform.position;
        Vector2 size = transform.localScale;

        //範囲からランダム位置取得
        float x = Random.Range(pos.x - size.x * 0.5f, pos.x + size.x * 0.5f);
        float y = Random.Range(pos.y - size.y * 0.5f, pos.y + size.y * 0.5f);
        Vector3 spawnPos = new Vector3(x, y, transform.position.z);

        //オブジェクトの生成
        GameObject g = Instantiate(spawnObject, spawnPos, Quaternion.identity);
        g.layer = gameObject.layer;
        MeshRenderer mr = g.GetComponent<MeshRenderer>();
        mr.sortingOrder = count + 2;

    }

    void OnDrawGizmos()
    {
        Color c =Gizmos.color;
        c = Color.green;
        c.a = 0.3f;
        Gizmos.color = c;
        Gizmos.DrawCube(transform.position, new Vector2(transform.localScale.x, transform.localScale.y));
    }
}