using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    bool isShake = false;

    /// <summary>
    /// 振動開始
    /// </summary>
    /// <param name="duration">持続期間</param>
    /// <param name="magnitude">振動の規模</param>
    public void Shake(float duration, float magnitude)
    {
        if (isShake) return;

        StartCoroutine(ShakeOnec(duration, magnitude));
    }

    /// <summary>
    /// 振動開始
    /// </summary>
    /// <param name="magnitude">振動の規模</param>
    public void Shake(float magnitude)
    {
        if (isShake) return;

        StartCoroutine(ShakeStart(magnitude));
    }

    /// <summary>
    /// 振動停止
    /// </summary>
    public void ShakeStop()
    {
        isShake = false;
        StopCoroutine(ShakeOnec(0, 0));
        StopCoroutine(ShakeStart(0));
    }

    IEnumerator ShakeOnec(float duration, float magnitude)
    {
        isShake = true;
        float elapsed = 0.0f;

        //duration秒の間カメラを振動させる
        while (elapsed < duration) 
        {
            Vector3 originPos = transform.localPosition;

            float x = Random.Range(-10, 10) * 0.1f * magnitude;
            float y = Random.Range(-10, 10) * 0.1f * magnitude;
            transform.localPosition = new Vector3(originPos.x + x, originPos.y + y, originPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        isShake = false;
    }

    IEnumerator ShakeStart(float magnitude)
    {
        isShake = true;
        while (isShake)
        {
            //Stopされるまでずっと振動させる。
            Vector3 originPos = transform.localPosition;
            float x = Random.Range(-10, 10) * 0.1f * magnitude;
            float y = Random.Range(-10, 10) * 0.1f * magnitude;
            transform.localPosition = new Vector3(originPos.x + x, originPos.y + y, originPos.z);
            yield return null;
        }
    }
}
