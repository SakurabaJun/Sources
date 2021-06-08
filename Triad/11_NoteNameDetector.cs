using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ピッチ測定
/// </summary>
public static class NoteNameDetector
{
    public static string[] noteNames = { "ド", "ド#", "レ", "レ#", "ミ", "ファ", "ファ#", "ソ", "ソ#", "ラ", "ラ#", "シ", "計測なし" };

    /// <summary>
    /// 周波数からノートネームを取得（12 = 計測結果なし）
    /// </summary>
    /// <param name="freq">周波数</param>
    public static string GetNoteName(float freq)
    {
        var noteNumber = calculateNoteNumberFromFrequency(freq);
        var note = noteNumber % 12;
        if (noteNumber < 0)
        {
            note = 12;
        }

        note++;
        if (note == 12)
        {
            note = 0;
        }
        else if (note == 13)
        {
            note = 12;
        }
        return noteNames[note];
    }

    private static int calculateNoteNumberFromFrequency(float freq)
    {
        return Mathf.FloorToInt(69 + 12 * Mathf.Log(freq / 440, 2));
    }
}
