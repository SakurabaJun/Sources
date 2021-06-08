using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
	/// <summary>
	/// 一色のサイズ
	/// </summary>
	public static readonly int colorChunkSize = 16;

	/// <summary>
	/// テクスチャの間隔を返す（0.0fから1.0f）
	/// </summary>
	/// <param name="size">テクスチャの量</param>
	/// <returns>テクスチャの間隔</returns>
	public static float TextureInterval(int size)
	{
		return 1f / (float)size;
	}

	/// <summary>
	/// テクスチャの幅
	/// </summary>
	public static int textureWidth = 256;

	/// <summary>
	/// テクスチャの幅をもとにカラーの量を返す
	/// </summary>
	/// <returns>カラーの量</returns>
	public static int ColorWidthAmount
    {
		get { return textureWidth / colorChunkSize; }
		set { textureWidth = value * colorChunkSize; }
    }

	/// <summary>
	/// テクスチャの高さ
	/// </summary>
	public static int textureHeight = 256;

	/// <summary>
	/// テクスチャの幅をもとにカラーの量を返す
	/// </summary>
	/// <returns>カラーの量</returns>
	public static int ColorHeightAmount
	{
		get { return textureHeight / colorChunkSize; }
		set { textureHeight = value * colorChunkSize; }
	}

	/// <summary>
	/// テクスチャの幅をもとにUV値を返す
	/// </summary>
	/// <returns>UV値</returns>
	public static Vector2 GetUVFromType(int amount, int index, int type)
    {
		return new Vector2(
			(type % ColorWidthAmount) * TextureInterval(amount),
			(type / ColorWidthAmount) * TextureInterval(amount))
			+ voxelUvs[index] * new Vector2(TextureInterval(amount), TextureInterval(amount));
	}

	/// <summary>
	/// Voxelのサイズ（float）
	/// </summary>
	public static readonly float fSize = 1f;

	/// <summary>
	/// Voxelのサイズ（Vector3）
	/// </summary>
	public static Vector3 vSize
    {
		get { return Vector3.one * fSize; }
    }

	/// <summary>
	/// Voxelの頂点
	/// </summary>
	public static readonly Vector3[] voxelVerts = new Vector3[8] {

		new Vector3(0.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 1.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 1.0f),

	};

	/// <summary>
	/// 頂点の順番
	/// </summary>
	public static readonly int[,] voxelTris = new int[6, 6] {

		{0, 3, 1, 1, 3, 2},
		{5, 6, 4, 4, 6, 7},
		{3, 7, 2, 2, 7, 6},
		{1, 5, 0, 0, 5, 4},
		{4, 7, 0, 0, 7, 3},
		{1, 2, 5, 5, 2, 6}

	};

	/// <summary>
	/// 面があるかを調べるための配列（面の隣にVoxelを確認するため）
	/// </summary>
	public static readonly Vector3[] faceCheck = new Vector3[6]
	{
		new Vector3(0.0f, 0.0f, -1.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, -1.0f, 0.0f),
		new Vector3(-1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f)
	};

	/// <summary>
	/// UVの配列
	/// </summary>
	public static readonly Vector2[] voxelUvs = new Vector2[6] 
	{

		new Vector2 (0.0f, 0.0f),
		new Vector2 (0.0f, 1.0f),
		new Vector2 (1.0f, 0.0f),
		new Vector2 (1.0f, 0.0f),
		new Vector2 (0.0f, 1.0f),
		new Vector2 (1.0f, 1.0f)
	};

	/// <summary>
	/// テスト用カラー配列
	/// </summary>
	public static readonly Color[] colors = new Color[4]
	{
		Color.clear,
		Color.red,
		Color.blue,
		Color.green,
	};

}