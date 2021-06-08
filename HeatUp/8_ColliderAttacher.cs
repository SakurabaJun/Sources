using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderAttacher : MonoBehaviour
{
    [Tooltip("親キャラクター")]
    public Character master;

    void Reset()
    {
        Transform parent = transform.parent;
        while(true)
        {
            if (parent == null) break;

            Character chara = parent.GetComponent<Character>();
            if(chara)
            {
                master = chara;
                break;
            }
            parent = parent.parent;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!master) return;

        //当たり判定を統括するため、hitしたら親のhit関数を呼ぶ
        master.OnTriggerEnter2DOfAttacher(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!master) return;

        //当たり判定を統括するため、hitしたら親のhit関数を呼ぶ
        master.OnTriggerStay2DOfAttacher(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!master) return;

        //当たり判定を統括するため、hitしたら親のhit関数を呼ぶ
        master.OnTriggerExit2DOfAttacher(collision);
    }
}
