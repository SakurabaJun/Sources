using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReSpawn : MonoBehaviour
{
    [TooltipAttribute("プレイヤーのリファレンス")]
    public Player player;

    [HideInInspector]
    public ReSpawnTrigger reSpawnTrigger;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player"&& reSpawnTrigger)
        {
            player.transform.position = reSpawnTrigger.transform.position;
            player.controller.collisionMask = 1 << reSpawnTrigger.gameObject.layer;
        }
    }
}
