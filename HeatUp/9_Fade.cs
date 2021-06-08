using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Fade : MonoBehaviour
{
    [TooltipAttribute("スクリプト実行時にプレイさせるか")]
    public bool play = true;

    [TooltipAttribute("フェイドのスピード")]
    public float speed = 1;

    [TooltipAttribute("フェイドの動き")]
    public FadeMovement fadeMovement;

    [TooltipAttribute("InOutを使用する場合の遅延")]
    public float delay = 0;

    [TooltipAttribute("ループさせるか")]
    public bool loop = false;

    [TooltipAttribute("処理している間はキャラクターのインプットを中止する")]
    public bool inputStop = false;

    [TooltipAttribute("プレイヤーの参照")]
    public Player player;

    public enum FadeMovement
    {
        In,
        Out,
        InOut,
        OutIn
    }
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        //inputStopがtrueの場合はプレイヤーのインプットを止める

        if (inputStop) player.inputStop = true;

        canvasGroup = GetComponent<CanvasGroup>();

        if (play) Play();
    }

    public void Play()
    {
        switch (fadeMovement)
        {
            case FadeMovement.In:
                StartCoroutine(IEStart(true));
                break;
            case FadeMovement.Out:
                StartCoroutine(IEStart(false));
                break;
            case FadeMovement.InOut:
                StartCoroutine(IEInOut(true));
                break;
            case FadeMovement.OutIn:
                StartCoroutine(IEInOut(false));
                break;
        }
    }

    private IEnumerator IEInOut(bool inOut)
    {
        float alpha = 0;

        //FadeIn
        while (true)
        {
            alpha += speed * Time.deltaTime;
            canvasGroup.alpha = inOut ? alpha : 1f - alpha;
            if (alpha >= 1) break;

            yield return null;
        }
        yield return new WaitForSeconds(delay);

        alpha = 0;
        //FadeOut
        while (true)
        {
            alpha += speed * Time.deltaTime;
            canvasGroup.alpha = inOut ? 1f - alpha : alpha;
            if (alpha >= 1)
            {
                if(inputStop) player.inputStop = false;

                if (loop) StartCoroutine(IEInOut(inOut));

                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator IEStart(bool fade)
    {
        if (!canvasGroup) yield break;

        yield return new WaitForSeconds(delay);
        float alpha = 0;
        while (true)
        {
            alpha += speed * Time.deltaTime;
            canvasGroup.alpha = fade ? alpha : 1f - alpha;
            if (alpha >= 1)
            {
                if (inputStop) player.inputStop = false;

                if (loop) StartCoroutine(IEStart(fade));

                yield break;
            }
            yield return null;
        }
    }
}
