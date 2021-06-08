using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCharacter : Character
{
    protected override void Start()
    {
        base.Start();//消さないでください。

        //ここから処理を書いてください。
    }

    protected override void Update()
    {
        base.Update();//消さないでください。

        //ここから処理を書いてください。
    }

    
    //キャラクターのhpが0になった時にDestroy(gameObject)以外の処理を書きたい場合はコメントを解除してください。
    /*
    protected override void DestroyCharacter()
    {
        Debug.Log("A");
        //base.DestroyCharacter();//Destroyしたくない場合は消去する。
    }
    */
}
