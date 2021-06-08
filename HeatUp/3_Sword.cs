using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sword : MonoBehaviour
{
    [Header("AttackOptions")]

    [SerializeField, TooltipAttribute("攻撃間隔")]
    public float interval = 0.5f;

    [SerializeField, TooltipAttribute("発動時間")]
    public float activateTime = 0.5f;

    [SerializeField, TooltipAttribute("キャラクターに与えるダメージ")]
    public int damageToPlayer = 2;

    [Header("TemperatureOptions")]
    [TooltipAttribute("初期温度")]
    public int defaultTemperature = 10;

    [TooltipAttribute("最大温度")]
    public int maxTemperature = 1000;

    [TooltipAttribute("温度上昇間隔")]
    public float increaseInterval = 1;

    [TooltipAttribute("温度上昇の初期値")]
    public float temperatureIncrease = 10;

    [TooltipAttribute("温度上昇の加速度")]
    public float temperatureAcceleration = 1;

    [TooltipAttribute("温度による色の種類")]
    public SwordColor[] swordColors;

    [Header("CollisionOptions")]
    [SerializeField, TooltipAttribute("攻撃範囲")]
    public Vector2 attackRange = Vector2.one;

    [SerializeField, TooltipAttribute("キャラクターの左右にある当たり判定の位置")]
    public Vector2 sideOffset = Vector2.zero;

    [SerializeField, TooltipAttribute("キャラクターの下にある当たり判定の位置")]
    public float underOffset = 0;

    [Header("CameraOptions")]
    [SerializeField, TooltipAttribute("カメラシェイクをオンにする温度")]
    public int cameraShakeTemperature = 50;

    [SerializeField, TooltipAttribute("カメラシェイクの最大震度")]
    public float maxCameraShake = 0.4f;

    [Header("Reference")]

    [SerializeField, TooltipAttribute("色を変更するスロット")]
    [Spine.Unity.SpineSlot]
    public string slot;

    [SerializeField, TooltipAttribute("剣のマテリアル")]
    public Material material;

    [HideInInspector]
    public int temperature;

    [SerializeField, TooltipAttribute("温度上昇を止める")]
    public bool stop = false;

    [TooltipAttribute("プレイヤーの参照")]
    public Player player;

    [TooltipAttribute("AudioSouceの参照")]
    public AudioSource[] audioSources;

    [TooltipAttribute("カメラシェイクの参照（メインカメラにアタッチされているコンポーネント）")]
    public CameraShake cameraShake;

    [TooltipAttribute("スケルトンアニメーションの参照（Spine）")]
    public Spine.Unity.SkeletonAnimation skeletonAnimation;

    IEnumerator attackStart;

    [System.Serializable]
    public struct SwordColor
    {
        public int temperature;
        public Color color;
    }

    void Start()
    {
        temperature = defaultTemperature;

        //色の更新
        UpdateColor();
    }

    void Update()
    {
        if (stop) return;

        if (barrels)
        {
            Vector3 playerPos = transform.position;
            playerPos.x = barrels.transform.position.x;
            transform.position = playerPos;
        }

        TemperatureIncrease();
    }

    #region Attack

    bool attackInput = false;
    bool attack = false;
    LayerMask layerMask;

    public void Attack(bool under, LayerMask layerMask)
    {
        attackInput = true;
        attackStart = AttackStart(under, layerMask);

        //攻撃コールチンスタート
        StartCoroutine(attackStart);
    }

    public void AttackFinish()
    {
        attackInput = false;
        attackStart = null;
        barrels = null;
    }

    private IEnumerator AttackStart(bool under, LayerMask layerMask)
    {
        yield return new WaitForSeconds(activateTime);
        attack = true;
        this.layerMask = layerMask;
        StartCoroutine(AttackLoop(under));
        yield break;
    }

    private IEnumerator AttackLoop(bool under)
    {
        while (attack)
        {
            //intervalごとにてきにヒットさせる
            if (under)
            {
                //下の当たり判定確認
                UnderHit(layerMask);
            }
            else
            {
                //左右の当たり判定確認
                SideHit(player.right, layerMask);
            }
            if (!attackInput)
            {
                attack = false;
            }
            audioSources[1].Play();
            yield return new WaitForSeconds(interval);
        }
        yield break;
    }

    void SideHit(bool right, LayerMask enemyMask)
    {
        Vector2 swordOffset = right ? sideOffset : new Vector2(-sideOffset.x, sideOffset.y);
        Vector2 center = new Vector2(transform.position.x, transform.position.y) + swordOffset;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(center, attackRange, 0, Vector2.zero, 0, enemyMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (!hit) return;


            //鉄の樽
            Iron_BarrelsHit(hit);

            //ボス
            BossHit(hit);

            //スイッチ
            Switch_AttackHit(hit);

            //蜂
            BeeAttack(hit);

            //キャラクター
            CharacterHit(hit);
        }
    }

    Barrels barrels;

    void UnderHit(LayerMask enemyMask)
    {
        //中心点を取得
        Vector2 center = new Vector2(transform.position.x, transform.position.y - underOffset);
        RaycastHit2D underHit = Physics2D.BoxCast(center, attackRange, 0, Vector2.zero, 0, enemyMask);

        if (!underHit) return;

        barrels = underHit.collider.GetComponent<Barrels>();

        if (!barrels) return;

        //温度/（温度/4）のダメージを与える
        barrels.ApplyDamage((float)temperature / (maxTemperature / 4));
        attackInput = false;
    }

    bool CharacterHit(RaycastHit2D hit)
    {
        Character chara = hit.collider.GetComponent<Character>();

        if (!chara) return;

        if (temperature > 0)
        {
            audioSources[0].Play();
        }

        int maxHp = chara.maxHp;
        chara.ApplyDamage(temperature);
        UpdateTemperature(-maxHp);
    }

    bool Iron_BarrelsHit(RaycastHit2D hit)
    {
        Iron_Barrels iron_barrels = hit.collider.GetComponent<Iron_Barrels>();

        if (!iron_barrels) return false;

        //音を出す
        if (!audioSources[0].isPlaying)
        {
            audioSources[0].Play();
        }

        //飛んでいく方向と温度を樽に送る
        iron_barrels.ApplyDamage((hit.transform.position - transform.position).normalized, temperature);
        attackInput = false;
        audioSources[0].Stop();
        UpdateTemperature(-10);
        return true;
    }

    bool BossHit(RaycastHit2D hit)
    {
        GolemHead bossHead = hit.collider.GetComponent<GolemHead>();
        if (!bossHead) return false;

        bossHead.master.ApplyDamage(temperature);
        return true;
    }

    bool Switch_AttackHit(RaycastHit2D hit)
    {
        Switch_Attack switch_Attach = hit.collider.GetComponent<Switch_Attack>();

        if (!switch_Attach) return false;

        switch_Attach.onSwitch(this);
        return true;
    }

    bool BeeAttack(RaycastHit2D hit)
    {
        Hati hati = hit.collider.GetComponent<Hati>();
        if (!hati) return false;

        hati.ApplyDamage(temperature);
        return true;
    }

    #endregion

    #region Temperature

    float totalTime = 0;
    float currentTime = 0;

    void SetMaterial(Color color)
    {
        //Spineのスロットをもとにマテリアルをセット
        Spine.Slot s = skeletonAnimation.skeleton.FindSlot(slot);
        material.SetColor("_Color", color);
        skeletonAnimation.CustomSlotMaterials.Remove(s);
        skeletonAnimation.CustomSlotMaterials.Add(s, material);
    }

    void TemperatureIncrease()
    {
        totalTime += Time.deltaTime;
        currentTime += Time.deltaTime;
        if (currentTime > increaseInterval)
        {
            //温度を求める（温度には上昇率があるため、(temperatureIncrease + temperatureAcceleration * totalTime)で求める）
            int temp = (int)(temperatureIncrease + temperatureAcceleration * totalTime);
            if (UpdateTemperature(temp))
            {
                totalTime = 0;
            }
            if (cameraShakeTemperature < temperature)
            {
                //温度が高すぎる場合はカメラをシェイクする
                int max = maxTemperature - cameraShakeTemperature;
                int difference = temperature - cameraShakeTemperature;
                float alpha = (float)difference / max;
                float magnitude = Mathf.Lerp(0.0f, maxCameraShake, alpha);
                cameraShake.ShakeStop();
                cameraShake.Shake(magnitude);
            }
            else
            {
                cameraShake.ShakeStop();
            }
            currentTime = 0;
        }
    }

    /// <summary>
    /// 温度を更新する
    /// </summary>
    /// <param name="tempe">与える温度</param>
    /// <returns>温度がmaxに達している場合はtrue</returns>
    public bool UpdateTemperature(int tempe)
    {
        bool release = false;

        //温度がmaxに達している場合はtrue
        if (temperature <= 0 || temperature >= maxTemperature)
        {
            release = true;
        }

        temperature += tempe;
        temperature = Mathf.Clamp(temperature, 0, maxTemperature);

        //色の更新
        UpdateColor();
        if (player.maxTemperature < temperature)
        {
            player.ApplyDamage(damageToPlayer);
        }
        return release;
    }

    void UpdateColor()
    {
        int min = -1;
        int minIndex = 0;

        //剣にはそれぞれの温度が指定されるので、すべて検索して最小値の色を求める
        for (int i = 0; i < swordColors.Length; i++)
        {
            if (swordColors[i].temperature <= temperature)
            {
                int temp = temperature - swordColors[i].temperature;
                if (min == -1)
                {
                    min = temp;
                    minIndex = i;
                }
                else if (min > temp)
                {
                    min = temp;
                    minIndex = i;
                }
            }
        }
        SetMaterial(swordColors[minIndex].color);
    }

    #endregion

    void OnDrawGizmos()
    {
        Vector2 center = new Vector2(transform.position.x, transform.position.y) + sideOffset;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireCube(center, attackRange);
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        center = new Vector2(transform.position.x, transform.position.y) + new Vector2(-sideOffset.x, sideOffset.y);
        Gizmos.DrawWireCube(center, attackRange);
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        center = new Vector2(transform.position.x, transform.position.y - underOffset);
        Gizmos.DrawWireCube(center, attackRange);
    }
}