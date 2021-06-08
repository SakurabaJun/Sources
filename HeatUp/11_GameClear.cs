using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClear : MonoBehaviour
{
    [TooltipAttribute("指定するキャラクター")]
    public Character character;

    [TooltipAttribute("次のステージパス")]
    public string nextStagePas;

    [TooltipAttribute("何秒後にフェイドアウトがスタートするか")]
    public float fadeDelay = 1;

    [TooltipAttribute("フェイドアウトのスピード")]
    public float speed = 1;

    [TooltipAttribute("フェイドコンポーネントの参照")]
    public Fade[] fades;

    [TooltipAttribute("イメージの参照")]
    public Image image;

    private bool start = false;

    // Update is called once per frame
    void Update()
    {
        if (!character) return;

        if (character.hp <= 0 && !start) 
        {
            Clear();
        }
    }

    public void Clear()
    {
        start = true;
        foreach(Fade f in fades)
        {
            f.gameObject.SetActive(true);
        }
        Invoke("FadeStart", fadeDelay);
    }

    private void FadeStart()
    {
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        float alpha = 0;

        //alphaが1になるまでFadeする
        while (alpha < 1) 
        {
            alpha += Time.deltaTime * speed;
            Color color = image.color;
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        SceneManager.LoadScene(nextStagePas);
        yield break;
    }
}
