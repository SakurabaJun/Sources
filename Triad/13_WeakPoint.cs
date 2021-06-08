using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeakPoint : MonoBehaviour {

    [HideInInspector]
    public string key;

    [HideInInspector]
    public Goddess goddess;

    public AudioSource audioSource;

    [HideInInspector]
    public AudioClip bell;

    Camera camera;

    public GoddessNetwork network;

    public Renderer renderer;

    const float selectRange = 60;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        FindSelectCamera();
    }

    [HideInInspector]
    public bool locked = false;

    //カメラを探す
    void FindSelectCamera()
    {
        if (locked)
        {
            return;
        }

        //カメラに映っているかを確認
        if (renderer.isVisible) 
        {
            //カメラとの角度をもとに選択か、選択解除かを判断
            Vector3 direction = Vector3.Normalize(camera.transform.position - transform.position);
            float angle = Vector3.Angle(transform.right, direction);

            if (angle < selectRange)
            {
                Select();
                return;
            }
            else
            {
                UnSelect();
            }
        }
        else
        {
            UnSelect();
        }
    }

    [HideInInspector]
    public bool selected = false;

    /// <summary>
    /// 選択する
    /// </summary>
    public void Select()
    {
        if (selected)
        {
            return;
        }
        selected = true;

        //選択された場合はネットワーク通信をしサーバークライアント間で同期させる
        network.SendWeakPoint(GetIndex(), Color.red);
        goddess.StartVocalization(this);
    }

    /// <summary>
    /// 選択解除する
    /// </summary>
    public void UnSelect()
    {
        if (!selected)
        {
            return;
        }
        selected = false;

        //選択解除された場合はネットワーク通信をしサーバークライアント間で同期させる
        network.SendWeakPoint(GetIndex(), Color.white);
        goddess.StopVocalization();
    }

    /// <summary>
    /// 弱点ポイントの色の更新
    /// </summary>
    /// <param name="color">更新する色</param>
    public void UpdateColor(Color color)
    {
        transform.GetChild(0).GetComponent<Renderer>().material.color = color;
        if(color == Color.blue)
        {
            locked = true;
        }

        if (!selected)
        {
            if (color == Color.white)
            {
                locked = false;
            }
            else
            {
                locked = true;
            }
        }
    }
    /// <summary>
    /// 弱点ポイントのインデックスを取得する
    /// </summary>
    /// <returns>弱点ポイント のインデックス</returns>
    public int GetIndex()
    {
        return transform.parent.GetSiblingIndex();
    }

    /// <summary>
    /// クリップを追加する
    /// </summary>
    /// <param name="clip">追加するオーディオクリップ</param>
    public void AddClip(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    /// <summary>
    /// ベルを鳴らす
    /// </summary>
    public void PlayBell()
    {
        audioSource.clip = bell;
        audioSource.Play();
    }

    /// <summary>
    /// ベルを止める
    /// </summary>
    public void StopBell()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }
}
