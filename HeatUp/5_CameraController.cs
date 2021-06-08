using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    [Tooltip("プレイヤー")]
    public Player player;

    [Tooltip("フォローカメラクラス")]
    public FollowCamera followCamera;

	void Update()
	{
		CompulsionMoveInUpdate();
	}


    #region CompulsionMove

    CameraTrigger cameraTrigger;

    float cameraAlpha = 0;

    bool jump = false;

    Vector3 cameraStartPos;

    Vector3 cameraStopPos;

    /// <summary>
    /// カメラトリガーを割り当てる
    /// </summary>
    /// <param name="cameraTrigger">割り当てるカメラトリガー</param>
    public void SetCameraTrigger(CameraTrigger cameraTrigger)
	{
        this.cameraTrigger = cameraTrigger;
    }

    void CompulsionMoveInUpdate()
	{
        //カメラトリガーがない場合はreturn;
        if (!cameraTrigger) return;

        //cameraAlphaが0の時、初期位置などをセットする
        if (cameraAlpha == 0)
        {
            cameraStartPos = transform.position;
            cameraStopPos = cameraTrigger.connectionTrigger.transform.position + followCamera.offset;
            followCamera.follow = false;
            player.inputStop = true;
            player.compulsionInput = cameraTrigger.go.vector;
        }

        //cameraAlphaが1より小さいの時、移動させる
        if (cameraAlpha < 1)
        {
            cameraAlpha += cameraTrigger.cameraSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(cameraStartPos, cameraStopPos, cameraAlpha);
            if (cameraAlpha > cameraTrigger.jumpAlpha && !jump)
            {
                player.transform.position = cameraTrigger.connectionTrigger.come.transform.position;
                jump = true;
            }
            if (jump)
            {
                player.compulsionInput = cameraTrigger.connectionTrigger.come.vector;
                string s = LayerMask.LayerToName(cameraTrigger.connectionTrigger.gameObject.layer);
                LayerMask l = LayerMask.GetMask(s);
                player.SetCollisionMask(l);
            }
        }
        else
        {
            //移動終了したら、値をリセットする
            transform.position = cameraStopPos;
            followCamera.follow = true;
            player.inputStop = false;
            jump = false;
            cameraAlpha = 0;
            cameraTrigger = null;
        }
    }

    #endregion
}
