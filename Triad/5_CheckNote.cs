using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckNote : MonoBehaviour {

	[Tooltip("音程を表示するテキスト")]
	public Text noteText;

	Goddess goddess;

	// Use this for initialization
	void Start () {
		//女神像の取得
		goddess = GetComponent<Goddess>();
	}
	
	// Update is called once per frame
	void Update () {
		GetNoteName();
	}

	void GetNoteName()
    {
		//スペクトラムデータをもとに音程を求める
		float[] spectrum = new float[8192];
		AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
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
		noteText.text = noteName;
	}

	bool isStart = false;

	public void StartCheck()
    {
		isStart = true;
    }
}
