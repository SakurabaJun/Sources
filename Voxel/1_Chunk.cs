using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class Chunk : MonoBehaviour
{
	[SerializeField, Tooltip("MeshFilterの参照")] 
	MeshFilter meshFilter;

	[SerializeField, Tooltip("MeshRendererの参照")] 
	MeshRenderer meshRenderer;

	[SerializeField, Tooltip("MeshColliderの参照")] 
	MeshCollider meshCollider;

	[SerializeField, Tooltip("Chunkの大きさ（全体の大きさはVoxelSizeに依存しています）")] 
	Vector3Int size = Vector3Int.one;

	[SerializeField, Tooltip("保存する名前")] 
	string fileName = "Chunk";

	[SerializeField, Tooltip("Voxelプレハブの参照")]
	GameObject prefab;

	public Vector3Int SetSize
    {
		set 
		{ 
			size = value;
			InitVoxel();
		}
    }

	int vertexIndex = 0;

	List<Vector3> vertices = new List<Vector3>();

	List<int> triangles = new List<int>();

	List<Vector2> uvs = new List<Vector2>();

	int[,,] voxels;

	Transform transformCache;

	void Start()
	{
		transformCache = transform;
		transform.localScale = Vector3.one * VoxelData.fSize;
		LoadChunk();
	}

	/// <summary>
	/// チャンクの更新（メッシュを再生成するので非常に重い。複数の更新がある場合は最後に更新。）
	/// </summary>
	public void UpdateMesh()
    {
		InitMeshData();
		CreateMesh();
	}

	#region 初期化

	/// <summary>
	/// チャンクの再生成（デフォルトに戻る）
	/// </summary>
	public void InitMesh()
	{
		InitVoxel();
		UpdateMesh();
	}

	//ボクセルデータの初期化
	void InitVoxel()
    {
		transform.localScale = Vector3.one * VoxelData.fSize;
		voxels = new int[size.x, size.y, size.z];

		//デフォルトのテクスチャを生成
		Color[] colors = new Color[2];
		colors[0] = new Color(0, 0, 0, 0);
		colors[1] = new Color(1, 0, 0, 0);

		CreateTexture(colors);

		//すべて初期値1にする
		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int z = 0; z < size.z; z++)
				{
					voxels[x, y, z] = 1;
				}
			}
		}
	}

	/// <summary>
	/// ボクセルデータのクリア
	/// </summary>
	public void ClearVoxel()
    {
		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int z = 0; z < size.z; z++)
				{
					voxels[x, y, z] = 0;
				}
			}
		}
	}

	//メッシュデータの初期化
	void InitMeshData()
	{
		vertexIndex = 0;
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();

		meshCollider.sharedMesh = null;

		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int z = 0; z < size.z; z++)
				{
					if (voxels[x, y, z] != 0) 
                    {
						//ボクセルメッシュのデータを追加する
						AddVoxelDataToChunk(new Vector3Int(x, y, z), voxels[x, y, z]);
					}
				}
			}
		}
	}

	#endregion

	#region メッシュ生成

	//ボクセルデータの割り当て
	void AddVoxelDataToChunk(Vector3Int pos, int type)
	{
		Vector3 fPos = pos;
		for (int j = 0; j < 6; j++)
		{
			//隣にボクセルがある場合はメッシュの描画は不必要であるため、ポリゴンの生成は行わない
			if (VoxelCheck(Vector3Int.FloorToInt(pos + VoxelData.faceCheck[j]))) continue;

			for (int i = 0; i < 6; i++)
			{
				int triangleIndex = VoxelData.voxelTris[j, i];
				Vector3 vertexPos = VoxelData.voxelVerts[triangleIndex] + fPos;
				//頂点情報の追加
				vertices.Add(vertexPos);
				triangles.Add(vertexIndex);

				//UVの追加
				AddTexture(i, type);

				vertexIndex++;
			}
		}
	}

	//テクスチャをメッシュデータに割り当て
	void AddTexture(int index, int type)
	{
		uvs.Add(VoxelData.GetUVFromType(texture.width / VoxelData.colorChunkSize, index, type));
	}

	//メッシュデータの配列をメッシュに割り当て
	void CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		//ノーマル再計算
		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;

		meshCollider.sharedMesh = meshFilter.sharedMesh;
	}

	#endregion

	#region ローカルポジションからアクセスできる関数

	//範囲内かを確認
	bool InRangeCheck(Vector3Int pos)
	{
		if (pos.x < 0 || pos.x > size.x - 1 || pos.y < 0 || pos.y > size.y - 1 || pos.z < 0 || pos.z > size.z - 1) return false;

		return true;
	}

	/// <summary>
	/// ローカル座標をもとにボクセルの存在を確認
	/// </summary>
	/// <param name="pos">ローカル座標</param>
	public bool VoxelCheck(Vector3Int pos)
	{
		if (!InRangeCheck(pos)) return false;

		return (voxels[pos.x, pos.y, pos.z] != 0);
	}

	/// <summary>
	/// ボクセルを直接セットする
	/// </summary>
	/// <param name="pos">ローカル座標</param>
	/// <param name="type">ボクセルのタイプ（テクスチャーID）</param>
	public void SetVoxel(Vector3Int pos, int type)
	{
		if (!InRangeCheck(pos)) return;

		voxels[pos.x, pos.y, pos.z] = type;
	}

	#endregion

	#region ワールドポジションからアクセスできる関数

	/// <summary>
	/// ワールド座標をもとにボクセルを探す
	/// </summary>
	/// <param name="pos">ワールド座標</param>
	public Vector3Int FindVoxelFromPosition(Vector3 pos)
    {
		Vector3 localPos = pos - transformCache.position;
		localPos *= 1f / VoxelData.fSize;
		int x = Mathf.FloorToInt(localPos.x);
		int y = Mathf.FloorToInt(localPos.y);
		int z = Mathf.FloorToInt(localPos.z);

		return new Vector3Int(x, y, z);
    }

	/// <summary>
	/// ワールド座標をもとにボクセルを消去
	/// </summary>
	/// <param name="pos">ワールド座標</param>
	/// <param name="doUpdate">true=メッシュの更新をする</param>
	public void RemoveVoxelFromPosition(Vector3 pos, bool doUpdate = true)
    {
		Vector3Int localPos = FindVoxelFromPosition(pos);
		if (!VoxelCheck(localPos)) return;

		int type = voxels[localPos.x, localPos.y, localPos.z];
		voxels[localPos.x, localPos.y, localPos.z] = 0;

        if (doUpdate) UpdateMesh();
	}

	/// <summary>
	/// ワールド座標をもとにボクセルに物理を適応
	/// </summary>
	/// <param name="pos">ワールド座標</param>
	/// <param name="doUpdate">true=メッシュの更新をする</param>
	/// <returns>生成したVoxelクラス</returns>
	public Voxel ChangePhysicsFromPosition(Vector3 pos, bool doUpdate = true)
	{
		Vector3Int localPos = FindVoxelFromPosition(pos);

		//ボクセルの存在を確認する
		if (!VoxelCheck(localPos)) return null;

		int type = voxels[localPos.x, localPos.y, localPos.z];
		voxels[localPos.x, localPos.y, localPos.z] = 0;

		//中心点を取りボクセルを生成させる
		Vector3 prefabPos = transformCache.position + (Vector3)localPos * VoxelData.fSize + VoxelData.vSize * 0.5f;
        Voxel voxel = Instantiate(prefab, prefabPos, Quaternion.identity).GetComponent<Voxel>();
		voxel.AddVoxelData(transform.localScale, type);

        if (doUpdate) UpdateMesh();

		return voxel;
	}

	/// <summary>
	/// ワールド座標をもとにボクセルを追加
	/// </summary>
	/// <param name="pos">ワールド座標</param>
	/// <param name="type">ボクセルのタイプ（テクスチャーID）</param>
	/// <param name="doUpdate">true=メッシュの更新をする</param>
	public void AddVoxelFromPosition(Vector3 pos, byte type, bool doUpdate = true)
    {
		Vector3Int localPos = FindVoxelFromPosition(pos);

		if (!InRangeCheck(localPos)) return;

		voxels[localPos.x, localPos.y, localPos.z] = type;

		if (doUpdate) UpdateMesh();
	}

	#endregion

	#region チャンクのロードと保存

	/// <summary>
	/// チャンクのセーブ
	/// </summary>
	public void SaveChunk()
    {
		if (Directory.Exists(GetSaveFilePath(fileName)))
        {
			Debug.LogError("既にディレクトリが生成されています。");
			return;
        }

		//保存用JsonDataクラスを生成
		JsonData j = new JsonData();
		j.size = size;
		int total = size.x * size.y * size.z;
		j.voxels = new IndexData[total];

		//すべてのボクセル情報をJsonDataに代入する
		int count = 0;
		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int z = 0; z < size.z; z++)
				{
					j.voxels[count] = new IndexData();
					j.voxels[count].b = voxels[x, y, z];
					count++;
				}
			}
		}

		string json = JsonUtility.ToJson(j);

		//フォルダの作成
		Directory.CreateDirectory(GetSaveFilePath(fileName));
		AssetDatabase.ImportAsset(GetSaveFilePath(fileName));

		//Jsonファイルの読み込み
		File.WriteAllText(GetSaveFilePath(fileName, ".json"), json);
		AssetDatabase.ImportAsset(GetSaveFilePath(fileName, ".json"));

		//テクスチャがある場合は保存する
		if (texture)
        {
			j.textureSize = texture.width;
			byte[] png = texture.EncodeToPNG();
			File.WriteAllBytes(GetSaveFilePath(fileName, ".png"), png);
			AssetDatabase.ImportAsset(GetSaveFilePath(fileName, ".png"));
		}

		//メッシュを保存する
		AssetDatabase.CreateAsset(meshFilter.sharedMesh, GetSaveFilePath(fileName, ""));
		AssetDatabase.SaveAssets();

		Debug.Log("ファイルを保存しました。");
		return;
	}

	Texture2D texture;

	JsonData jsonData;

	/// <summary>
	/// チャンクのロード
	/// </summary>
	public void LoadChunk()
    {
		if (!Directory.Exists(GetSaveFilePath(fileName)))
		{
			Debug.LogError("ファイルがありません。");
			return;
		}

		//Jsonファイルを探す
		string json = File.ReadAllText(GetSaveFilePath(fileName, ".json"));
		jsonData = JsonUtility.FromJson<JsonData>(json);

		size = jsonData.size;

		voxels = new int[size.x, size.y, size.z];

		//JsonデータからVoxelに代入する
		int count = 0;
		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int z = 0; z < size.z; z++)
				{
					voxels[x, y, z] = jsonData.voxels[count].b;
					count++;
				}
			}
		}

		//テクスチャを読み込む
		string pngPath = GetSaveFilePath(fileName, ".png");
		if (File.Exists(pngPath))
        {
			byte[] png = File.ReadAllBytes(pngPath);

			texture = new Texture2D(jsonData.textureSize, jsonData.textureSize);
			texture.LoadImage(png);
			texture.filterMode = FilterMode.Point;

			meshRenderer.material.mainTexture = texture;
		}
		UpdateMesh();
	}

	string GetSaveFilePath(string name, string extension = null)
	{
		//Appleというファイルを作ると、フォルダーの中にデータが格納されるため、拡張子（extension）がある場合はファイル、ない場合はフォルダ歳てパスの位置を変える
		string path = "Assets/Chunks/";
		if (extension != null) 
        {
			name += "/" + name + extension;
        }
		path += name;
#if UNITY_EDITOR
#else
        filePath = Application.persistentDataPath + "/" + filePath;
#endif
		return path;
	}

	/// <summary>
	/// テクスチャの生成
	/// </summary>
	/// <param name="colors">追加元のColor配列</param>
	public void CreateTexture(Color[] colors)
	{
		//テクスチャのサイズをカラーの量から算出（25個の場合は5*5なので5）
		int size = (int)(Mathf.Ceil((Mathf.Sqrt((float)colors.Length))));
		VoxelData.ColorWidthAmount = size;
		VoxelData.ColorHeightAmount = size;

		texture = new Texture2D(
			VoxelData.textureWidth,
			VoxelData.textureHeight,
			TextureFormat.ARGB32, false);

		for (int i = 0; i < colors.Length; i++)
		{
			//テクスチャは16＊16のサイズなので、16＊16回分colorChunkを初期化する
			Color[] colorChunk = new Color[VoxelData.colorChunkSize * VoxelData.colorChunkSize];
			for (int j = 0; j < colorChunk.Length; j++)
			{
				colorChunk[j] = colors[i];
			}

			//ピクセルにセットする
			texture.SetPixels(
				i % VoxelData.ColorWidthAmount * VoxelData.colorChunkSize,
				i / VoxelData.ColorHeightAmount * VoxelData.colorChunkSize,
				VoxelData.colorChunkSize, VoxelData.colorChunkSize, colorChunk);
		}

		texture.filterMode = FilterMode.Point;
		texture.Apply();

		meshRenderer.sharedMaterial.mainTexture = texture;
	}

	#endregion
}

[System.Serializable]
public class JsonData
{
	public Vector3Int size;

	public IndexData[] voxels;

	public int textureSize;
}

[System.Serializable]
public class IndexData
{
	public int b;
}