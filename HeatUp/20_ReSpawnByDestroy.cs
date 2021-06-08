using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReSpawnByDestroy : MonoBehaviour
{
    public float time = 1;
    public GameObject prefab;
    public GameObject origin;
    bool spawn = false;
    // Start is called before the first frame update
    void Start()
    {
        origin = transform.GetChild(0).gameObject;

        if (!prefab || !origin) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //キャラクターがDestroyしたらスポーンさせる
        if (!origin && !spawn) StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        spawn = true;
        yield return new WaitForSeconds(time);
        GameObject g = Instantiate(prefab, transform);
        g.transform.localPosition = Vector3.zero;
        g.layer = gameObject.layer;
        origin = g;
        spawn = false;
        StopCoroutine(Spawn());
    }
}
