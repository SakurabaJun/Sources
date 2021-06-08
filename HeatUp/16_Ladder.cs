using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Ladder : MonoBehaviour
{
    [TooltipAttribute("梯子の長さ")]
    public int lenght = 1;

    [TooltipAttribute("梯子の中間部分のプレハブ")]
    public GameObject middle;

    [TooltipAttribute("梯子の末端部分のプレハブ")]
    public GameObject bottom;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag != "Player") return;

        Player player = collision.GetComponent<Player>();

        //梯子に触れた場合プレイヤースクリプトにレイヤーを比較してもらい、梯子に登れるかを確認する
        if (!player.CompareLayerAndColliderMask(gameObject.layer) || player.climb) return;

        if (!player.controller.IsGrounded)
        {
            player.Climb();
        }
        else
        {
            player.ResetClimb();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag != "Player") return;

        Player player = collision.GetComponent<Player>();

        //梯子に触れた場合プレイヤースクリプトにレイヤーを比較してもらい、梯子から離れられるかを確認する
        if (!player.CompareLayerAndColliderMask(gameObject.layer)) return;

        player.ResetClimb();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            return;
        }

        foreach (Transform child in transform)
        {
            EditorApplication.delayCall += () => DestroyImmediate(child.gameObject);
        }
#endif
        GenerateLadder();
    }

    private void GenerateLadder()
    {
        //サイズの確認
        Vector2 topSize = GetComponent<SpriteRenderer>().bounds.size;
        Vector2 middleSize = middle.GetComponent<SpriteRenderer>().bounds.size;
        Vector2 bottomSize = bottom.GetComponent<SpriteRenderer>().bounds.size;

        //サイズの確認
        for (int i = 0; i < lenght; i++)
        {
            GameObject middleObject = Instantiate(middle, transform);
            middleObject.layer = gameObject.layer;
            middleObject.transform.localPosition = new Vector3(0, -(topSize.y + middleSize.y * i), 0);
        }

        //ボトムの生成
        GameObject bottomObject = Instantiate(bottom, transform);
        bottomObject.transform.localPosition = new Vector3(0, -(topSize.y + middleSize.y * lenght), 0);
        bottomObject.layer = gameObject.layer;

        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();

        Vector2 size = boxCollider2D.size;
        size.y = topSize.y + middleSize.y * lenght + bottomSize.y;
        boxCollider2D.size = size;

        Vector2 offset = boxCollider2D.offset;
        offset.y = -size.y / 2;
        boxCollider2D.offset = offset;
    }
}
