using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Goddess : MonoBehaviour
{
	[Tooltip("レベルの配列")]
	public Level[] levels;

	[Tooltip("バリアのプレハブ")]
	public GameObject barrierPrefab;

	[Tooltip("バリア破壊のプレハブ")]
	public GameObject barrierDestPrefab;

	[Tooltip("弱点ポイントのプレハブ")]
	public GameObject weakPointPrefab;

	[Tooltip("浄化のプレハブ")]
	public GameObject purified;

	[Tooltip("女神像のネットワーククラス")]
	public GoddessNetwork network;

	[Tooltip("女神像のUIコントローラー")]
	public GoddessUIController guc;

	[Tooltip("QRコードコントローラー")]
	public QRCodeController qrcc;

	[Tooltip("プレイヤー")]
	public Player player;

	const float barrierSizeInterval = 0.02f;

	[HideInInspector]
	public int level = 0;

	Transform barriersTransform;

    // Use this for initialization
    void Start()
	{
		VoiceInitialize();

		RecordInitialize();

		BarriersInitialize();

		WeakPointInitialize();

		MeasureInitialize();

        PurifiedInitialize();
    }

	// Update is called once per frame
	void Update()
	{
		GetNoteName();
        if (network.isServer)
        {
            TimeLimit();
        }
	}

	/// <summary>
	/// レベルのデータ構造体
	/// </summary>
	[System.Serializable]
	public struct Level
	{
		/// <summary>
		/// バリアの数
		/// </summary>
		[Tooltip("バリアの数")]
		public int barrierAmount;

		/// <summary>
		/// 和音
		/// </summary>
		[Tooltip("和音")]
		public string[] chord;
	}

	/// <summary>
	/// QEコードを非表示にする関数
	/// </summary>
	public void HideQRCord()
    {
		qrcc.Hide();
    }

	#region 女神像

	[Tooltip("アニメーターの参照")]
	public Animator animator;

	[Tooltip("説明TextMeshの参照")]
	public TextMesh explanation;

	void SetExplanation(string str)
    {
        explanation.text = str;
    }

	/// <summary>
	/// 女神像を移動する関数
	/// </summary>
	/// <param name="a">a地点</param>
	/// <param name="b">b地点</param>
	/// <param name="speed">移動スピード</param>
	public void MoveGoddess(Vector3 a, Vector3 b, float speed)
    {
        StartCoroutine(IE_MoveGoddess(a, b, speed));
    }

	IEnumerator IE_MoveGoddess(Vector3 a, Vector3 b, float speed)
    {
        float t = 0;
		while (true)
        {
			t += speed * Time.deltaTime;
            transform.localPosition = Vector3.Lerp(a, b, t);
            yield return null;
			if (t >= 1) 
            {
                transform.localPosition = b;
                yield break;
			}
        }
    }

	#endregion

	#region バリア

	int clear = 0;

	void BarriersInitialize()
	{
		barriersTransform = transform.GetChild(1);
		barrierPrefab = (GameObject)Resources.Load("Character/Barrier");
		barrierDestPrefab = (GameObject)Resources.Load("Character/BarrierDest");
	}

	public void SetBarriers()
	{
        SetExplanation("浄化を開始してください");

        barriers.Clear();
		foreach (Transform child in barriersTransform)
		{
			Destroy(child.gameObject);
		}

		//サーバーだった場合ランダムに声を割り当てる
        if (network.isServer) 
        {
			if (clear == 0) GetVoices();

			SetRandomVoice();
		}

		//バリアの枚数を取得する
		int bAmount = levels[level].barrierAmount;

		//バリアを生成する
		for (int i = 0; i < bAmount; i++)
		{
			if (bAmount - clear > i) 
            {
				GameObject b = Instantiate(barrierPrefab, barriersTransform);
				float size = b.transform.localScale.x + barrierSizeInterval * i;
				b.transform.localScale = new Vector3(size, size, size);
				barriers.Add(b);
			}
		}

		//弱点ポイントを取得する
		SetWeakPoint(barriers[barriers.Count - 1].transform);
	}

	List<GameObject> barriers = new List<GameObject>();

	float progress = 0;

	/// <summary>
	/// ひび割れの追加
	/// </summary>
	public void AddCrack()
	{
		progress += 1.0f / weakPoints.Count;
		foreach (GameObject g in barriers)
		{
			Material m = g.transform.GetChild(1).GetComponent<Renderer>().material;
			m.SetFloat("_Progress", progress);
		}
	}

	#endregion

	#region 弱点

	[HideInInspector]
    public List<WeakPoint> weakPoints = new List<WeakPoint>();

	void WeakPointInitialize()
	{
		weakPointPrefab = (GameObject)Resources.Load("Character/Weakpoint");
	}

	void SetWeakPoint(Transform parent)
	{
		//弱点ポイントのクリア
		weakPoints.Clear();
		foreach (WeakPoint w in weakPoints)
		{
			Destroy(w.gameObject);
		}

		//弱点ポイントの生成
		for (int i = 0; i < levels[level].chord.Length; i++)
		{
			if (i == myKey) break;

			GameObject w = Instantiate(weakPointPrefab, parent);
			WeakPoint s = w.transform.GetChild(0).GetComponent<WeakPoint>();
			s.key = levels[level].chord[i];
			s.goddess = this;
			s.bell = GetBell(s.key);
			TextMesh t = s.transform.GetChild(1).GetComponent<TextMesh>();
			t.text = s.key;
			weakPoints.Add(s);
		}

		//弱点ポイントの向きの変更
		for (int i = 0; i < weakPoints.Count; i++)
		{
			weakPoints[i].transform.parent.Rotate(new Vector3(0, i * (360 / weakPoints.Count), 0), Space.Self);
		}
	}

	/// <summary>
	/// 弱点ポイントを探す関数
	/// </summary>
	/// <param name="index">弱点ポイントのインデックス</param>
	/// <returns>弱点ポイントを返す（存在しない場合はnullを返す）</returns>
	public WeakPoint FindWeakPoint(int index)
	{
		foreach (WeakPoint w in weakPoints)
		{
			if (index != w.GetIndex()) continue;

			return w;
		}
		return null;
	}

	#endregion

	#region プレイヤーへの指示

	const int countdown = 3;

	Coroutine c_Vocalization;

	[HideInInspector]
	public bool selectedWeakPoint = false;

	WeakPoint weakPoint;

	/// <summary>
	/// 発声メソッドをスタートする
	/// </summary>
	/// <param name="weakPoint">弱点ポイント</param>
	public void StartVocalization(WeakPoint weakPoint)
	{
        SetExplanation(string.Empty);
        selectedWeakPoint = true;
		this.weakPoint = weakPoint;
		c_Vocalization = StartCoroutine(IE_Vocalization());
	}

	/// <summary>
	/// 発声メソッドをストップする
	/// </summary>
	public void StopVocalization()
	{
		StopCoroutine(c_Vocalization);
		guc.SetInstruction(string.Empty);
		selectedWeakPoint = false;
		StopRecord();
	}

	IEnumerator IE_Vocalization()
	{
		while (true)
		{
            weakPoint.PlayBell();
			guc.SetInstruction("発声の準備をしてください");

			StartMeasure();
			for (int i = 0; i < countdown; i++)
			{
				guc.SetInstruction((countdown - i).ToString());
				yield return new WaitForSeconds(1);
			}
			weakPoint.StopBell();

			//測定開始
			guc.SetInstruction("発声してください");
			StopMeasure();
			StartMeasure();
			yield return new WaitForSeconds(recordingTime);
			StopMeasure();
			//測定停止

			//保存した音声を再生する
			PlayRecord();
			int score = GetScore();
            player.AddMyScore(score);
			guc.SetInstruction(score.ToString() + "点です！");
			yield return new WaitForSeconds(recordingTime);

			//scoreが低すぎた場合はもう一度録音させる
			if (score < minScore)
			{
				guc.SetInstruction("もう一度録音します");
				yield return new WaitForSeconds(1);
			}
			else
			{
				break;
			}
		}

		//相手のスマホに音声データを送る
		guc.SetInstruction("送信中...");
        weakPoint.locked = true;
		network.SendWeakPoint(weakPoint.GetIndex(), Color.blue);
		network.SendAudio(weakPoint.GetIndex(), audioSource.clip);
		network.SendCrack();
		selectedWeakPoint = false;
        yield return new WaitForSeconds(1);
		guc.SetInstruction(string.Empty);
        TryPurification();
		yield break;
	}

	#endregion

	#region 録音

	[HideInInspector]
	public AudioSource audioSource;

	const int recordingTime = 3;

	const int sampleRate = 16000;

	void RecordInitialize()
	{
		audioSource = GetComponent<AudioSource>();
		StartRecord();
		StopRecord();
	}

	//録音開始
	void StartRecord()
	{
		if (Microphone.devices.Length == 0)
		{
			Debug.Log("マイクが見つかりません");
			return;
		}

		string devName = Microphone.devices[0];

		audioSource.clip = Microphone.Start(devName, true, recordingTime, sampleRate);
		while (Microphone.GetPosition(null) <= 0) { }
	}

	//録音停止
	void StopRecord()
	{
		string devName = Microphone.devices[0];
		Microphone.End(devName);
		audioSource.loop = false;
		audioSource.mute = false;
	}

	/// <summary>
	/// 録音したオーディオの再生
	/// </summary>
	public void PlayRecord()
	{
		audioSource.Play();
	}

	void TryPurification()
	{
		//すべての弱点ポイントを検索しaudioClipの存在を確認する
		foreach (WeakPoint w in weakPoints)
		{
            if (!w.audioSource.clip)
            {
				//一つでもなかった場合はreturn
				return;
            }
		}

		network.SendPurification();
    }

	void PlayAllClip()
    {
        PlayVoice();

		foreach (WeakPoint w in weakPoints)
		{
			w.audioSource.Play();
		}
    }


	void RemoveAllClip()
    {
        foreach (WeakPoint w in weakPoints)
        {
            if (w)
            {
				w.audioSource.clip = null;
			}
		}
    }

	public void StopAllClip()
    {
        StopVoice();

        foreach (WeakPoint w in weakPoints)
        {
            w.audioSource.Stop();
        }
    }

	/// <summary>
	/// クリップを弱点ポイントに追加する
	/// </summary>
	/// <param name="index">弱点ポイントのインデックス</param>
	/// <param name="clip">追加するオーディオクリップ</param>
	public void AddClip(int index, AudioClip clip)
	{
		FindWeakPoint(index).audioSource.clip = clip;
	}

	#endregion

	#region 音程

	const int minScore = 60;

	bool getNoteStart = false;

	List<bool> right = new List<bool>();

	AudioMixerGroup mixerGroup;

	[HideInInspector]
	public int myKey;

	Tuner tuner;

	void MeasureInitialize()
	{
		mixerGroup = audioSource.outputAudioMixerGroup;
		tuner = GameObject.Find("Dial").GetComponent<Tuner>();
	}

	void StartMeasure()
	{
		audioSource.outputAudioMixerGroup = mixerGroup;
		getNoteStart = true;
		right.Clear();
		StartRecord();
		audioSource.Play();
	}

	void StopMeasure()
	{
		audioSource.outputAudioMixerGroup = null;
		getNoteStart = false;
		audioSource.Stop();
		StopRecord();
	}

	void GetNoteName()
	{
		if (!getNoteStart) return;

		//周波数をもとに音程を取得
		float[] spectrum = new float[8192];
		audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
		var maxIndex = 0;
		var maxValue = 0.0f;
		for (int i = 0; i < spectrum.Length; i++)
		{
			var val = spectrum[i];
			if (val > maxValue)
			{
				maxValue = val;
				maxIndex = i;
			}
		}
		var freq = maxIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length;

		string noteName = NoteNameDetector.GetNoteName(freq);

		//音程が合っているかをrightに保存
		right.Add(weakPoint.key == noteName);

        if (tuner) tuner.Rotation(noteName, weakPoint.key);

    }

	/// <summary>
	/// 現在の音程と必要とされる音程を取得
	/// </summary>
	/// <param name="nowNote">現在判定を行っている音程</param>
	/// <param name="rightNote">必要とされている音程</param>
	public void GetNowNote(ref string nowNote, ref string rightNote)
    {
        if (weakPoint)
        {
			rightNote = weakPoint.key;
		}
        else
        {
			rightNote = null;
        }
    }

	int GetScore()
	{
		int score = 0;
		foreach (bool r in right)
		{
			if (r == true)
			{
				score++;
			}
		}
		return (int)(100 * ((float)score / right.Count));
	}

	#endregion

	#region 声

	AudioSource voiceSouce;

	public void VoiceInitialize()
	{
		voiceSouce = transform.GetChild(2).GetComponent<AudioSource>();
	}

	List<AudioClip> voices = new List<AudioClip>();

	void GetVoices()
    {
		voices.Clear();
		foreach (string s in levels[level].chord)
		{
			voices.Add(GetVoice(s));
		}
	}

	AudioClip GetVoice(string str)
    {
		return Resources.Load<AudioClip>("Character/Voice/" + str);
    }

	AudioClip GetBell(string str)
	{
		return Resources.Load<AudioClip>("Character/Bell/" + str);
	}

	void SetRandomVoice()
    {
		//ランダムを初期化する
		Random.InitState(System.DateTime.Now.Millisecond);

		int random = Random.Range(0, voices.Count);
		AudioClip randomClip = voices[random];
		print(randomClip);

		//コードとランダムに割り当てた声を比較して自分のキーコードを取得する
		for (int i = 0; i < levels[level].chord.Length; i++) 
        {
			if (levels[level].chord[i] == randomClip.name) 
			{
				myKey = i;
			}
		}

		//サーバーだった場合はクライアントに声を送る
        if (network.isServer)
        {
			network.SendGoddessVoice();
		}
	}

	/// <summary>
	/// 自分の声を割り当てる
	/// </summary>
	/// <param name="key">音程</param>
	public void SetMyVoice(int key)
    {
        myKey = key;
        SetBarriers();
    }

	void PlayVoice()
	{
		voiceSouce.clip = GetVoice(levels[level].chord[myKey]);
		voiceSouce.Play();
	}

	void StopVoice()
	{
		voiceSouce.clip = null;
		voiceSouce.Stop();
	}

	#endregion

	#region 浄化

	Vector3 spawnPos = new Vector3(0, 0.3f, 0);

	void PurifiedInitialize()
    {
        purified = Resources.Load<GameObject>("Purified/Purified");
    }

	public void Purification()
	{
		PlayAllClip();

		if (barriers.Count > 1)
		{
			Invoke("Destruction", 1);
			Invoke("Divide", 2);
		}
        else
        {
			animator.SetBool("purification", true);
			clear = 0;
			Invoke("Destruction", 1);
			Invoke("PurificationMoveGoddess", 2);
		}
	}

	void PurificationMoveGoddess()
	{
        player.AddPurified();

		guc.SetInstruction("浄化が完了しました");

		//サーバーの場合は時間を足す
        if (network.isServer)
        {
            time += subtractTime;
        }

		Vector3 start = transform.localPosition;
		Vector3 end = start + spawnPos;
		MoveGoddess(start, end, 1f);
        if (network.isServer)
        {
            Invoke("SetNextPosition", 2);
        }

		//浄化完了の文字を出現させる
        Instantiate(purified, guc.GetCanvas().transform);
    }

	void Divide()
    {
		clear++;
		voices.Remove(GetVoice(levels[level].chord[myKey]));
		SetBarriers();
	}

	void SetNextPosition()
    {
		//新しく別の場所に女神像を移動させる
        RemoveAllClip();
        guc.SetInstruction(string.Empty);
        Vector3 pos = DetectedPanelFinder.FindRandomPosition();
        transform.position = pos;

        Vector3 rot = transform.localEulerAngles;
        float rotY = Random.Range(0, 360);
        rot.y = rotY;
        transform.localEulerAngles = rot;

        network.SendRespawn();
    }

	void Destruction()
	{
		GameObject b = barriers[barriers.Count - 1];
		b.transform.GetChild(0).gameObject.SetActive(false);
		b.transform.GetChild(1).gameObject.SetActive(false);
		GameObject g = Instantiate(barrierDestPrefab);
		g.transform.position = barriersTransform.position;

		//弱点ポイントをフェードアウトさせる
        foreach(WeakPoint w in weakPoints)
        {
            w.FadeOut();
        }
	}

	/// <summary>
	/// リスポーン
	/// </summary>
	/// <param name="pos">スポーン位置</param>
	/// <param name="rotY">Y軸回転</param>
	/// <param name="space">スペース</param>
	public void Respawn(Vector3 pos, float rotY, Space space)
	{
        stopTimeLimit = false;

        animator.SetBool("purification", false);

        SetExplanation("浄化を開始してください");

        level++;
		if (level >= levels.Length) 
        {
			time = maxTime - subtractTime;
			if(time < minTime)
            {
				time = minTime;
            }
			level = 0;
        }
		SetBarriers();

        if (space == Space.World) 
        {
            transform.position = pos;
        }
        else if (space == Space.Self) 
        {
            transform.localPosition = pos;
        }

        Vector3 rot = transform.localEulerAngles;
        rot.y = rotY;
        transform.localEulerAngles = rot;

        Vector3 end = transform.localPosition;
		//女神像を少し浮かせるためにVector3のオフセットを足す
		Vector3 start = end + new Vector3(0, 0.3f, 0);

        MoveGoddess(start, end, 1f);
	}

	#endregion

	#region タイマー

	int time = 200;

	int maxTime;

	const int minTime = 60;

	const int subtractTime = 20;

	public void GameStart()
    {
        maxTime = time;
		guc.SetActiveTime(true);

		//タイマーはサーバーのみ処理し、クライアントに送信することでフレーム落ちしても、時間がずれないようにしている
        if (network.isServer)
        {
            StartCoroutine(IE_Timer());
        }
    }

	IEnumerator IE_Timer()
    {
		while(true)
        {
			time--;
			guc.SetTime(time);
            network.SendTime(time);
			if (time <= 0) 
            {
				break;
            }
			yield return new WaitForSeconds(1);
		}

        TimeUp();

		//平均値を送信
        network.SendAverage();
        yield break;
    }

	public void SetTime(int time)
    {
        this.time = time;
        guc.SetTime(this.time);
    }

	void TimeUp()
    {
        stopTimeLimit = true;

        foreach(WeakPoint w in weakPoints)
        {
            w.locked = true;
        }
    }

	#endregion

	#region デスポーン

	const float maxTimeLimit = 15;

	float timeLimit = maxTimeLimit;

	bool stopTimeLimit = false;

	void TimeLimit()
    {
        if (stopTimeLimit) return;

		//常に時間を引いて、0以下になったらデスポーンさせる
        timeLimit -= Time.deltaTime;
        if (timeLimit <= 0)
        {
            timeLimit = maxTimeLimit;
            stopTimeLimit = true;
            network.SendDespawn();
        }
    }

	public void DespawnMoveGoddess()
    {
        Vector3 start = transform.localPosition;
        Vector3 end = start - spawnPos;
        MoveGoddess(start, end, 1f);

		//サーバーの場合のみ次スポーン位置を探す（クライアントの場合はスポーン位置決定後、送信される）
        if (network.isServer)
        {
            Invoke("SetNextPosition", 2);
        }
    }

	public void StopTimeLimit()
    {
        stopTimeLimit = true;
    }

	public void ResetTimeLimit()
    {
        timeLimit = maxTimeLimit;
        stopTimeLimit = false;
    }

    #endregion
}
