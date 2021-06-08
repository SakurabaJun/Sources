using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class Voxelizer : EditorWindow
{
    GameObject select;

    MeshFilter meshFilter;

    MeshRenderer meshRenderer;

    MeshCollider meshCollider;

    Texture2D meshTexture;

    GameObject chunkprefab;

    Transform transformCache;

    void Awake()
    {
        //Chunkプレハブの取得
        chunkprefab = EditorGUIUtility.Load("Chunk.prefab") as GameObject;
    }

    [MenuItem("Window/Voxelizer")]
    static void Open()
    {
        EditorWindow.GetWindow<Voxelizer>("Voxelizer");
    }


    void OnEnable()
    {
        //オブジェクトの選択時にOnChanged関数のイベント追加
        Selection.selectionChanged += OnChanged;
    }

    void OnDisable()
    {
        //オブジェクトの非選択時にOnChanged関数のイベント削除
        Selection.selectionChanged -= OnChanged;
    }

    /// <summary>
    /// 選択されたオブジェクトに必須コンポーネントが追加されているかを確認する。
    /// </summary>
    void OnChanged()
    {
        select = Selection.activeGameObject;
        if (!select) return;

        //アセットなので、選択したオブジェクトからGetComponentしている。エディター上のみの仕組み。
        meshFilter = select.GetComponent<MeshFilter>();
        meshRenderer = select.GetComponent<MeshRenderer>();
        if (meshFilter && meshRenderer)
        {
            canGenerate = true;
        }
        else
        {
            canGenerate = false;
        }
        Repaint();
    }

    bool canGenerate = false;

    /// <summary>
    /// 簡略度 0.0fから1.0f
    /// </summary>
    float simplified;

    float GetHalfSimplified
    {
        get { return simplified * 0.5f; }
    }

    void OnGUI()
    {
        GUILayout.Label("MeshFilterとMeshRendererを含んでいる\nオブジェクトを選択してください。");

        simplified = EditorGUILayout.Slider("Simplified", simplified, 0.0f, 1.0f);

        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(!canGenerate);
        if (GUILayout.Button("Generate"))
        {
            Init();
            Generate();
            DestroyImmediate(meshCollider);
            select.SetActive(false);
        }
        EditorGUI.EndDisabledGroup();
    }

    Vector3Int localSize;
    Vector3 center;

    void Init()
    {
        meshCollider=select.GetComponent<MeshCollider>();
        if (!meshCollider)
        {
            meshCollider = select.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        //メッシュのサイズを取得する
        Vector3 size = meshFilter.sharedMesh.bounds.size;
        localSize = Vector3Int.FloorToInt(size) + Vector3Int.one;

        //メッシュの中心を取得する
        center = meshFilter.sharedMesh.bounds.center;

        transformCache = select.transform;
    }

    /// <summary>
    /// 頂点情報をもとにvoxelの位置を取得しChunkに代入
    /// </summary>
    public void Generate()
    {
        //Chunkの生成
        Chunk chunk = (PrefabUtility.InstantiatePrefab(chunkprefab) as GameObject).GetComponent<Chunk>();
        chunk.gameObject.name = chunkprefab.name + "(Voxels)";
        Transform chunkTransform = chunk.transform;

        //位置とサイズの変更
        chunk.SetSize = localSize;
        Vector3 searchPos = transformCache.position + center - new Vector3(localSize.x * 0.5f, localSize.y * 0.5f, localSize.z * 0.5f);
        chunkTransform.position = searchPos;

        //位置とサイズの変更
        List<Color> colors = new List<Color>();
        colors.Add(new Color(0, 0, 0, 0));

        //テクスチャの取得
        meshTexture = meshRenderer.sharedMaterial.mainTexture as Texture2D;

        chunk.ClearVoxel();

        for (int x = 0; x < localSize.x; x++)
        {
            for (int y = 0; y < localSize.y; y++)
            {
                for (int z = 0; z < localSize.z; z++)
                {
                    Vector3 pos = searchPos + new Vector3(x, y, z) * VoxelData.fSize;

                    for (int i = 0; i < 6; i++)
                    {
                        Color color;

                        //メッシュがあるかどうかを確認し、あった場合はColor情報を読み取る
                        if (CheckMesh(pos, VoxelData.faceCheck[i], out color))
                        {
                            SetColor(colors, color);
                            chunk.SetVoxel(new Vector3Int(x, y, z), SetColor(colors, color));
                            break;
                        }
                    }
                }
            }
        }

        chunk.CreateTexture(colors.ToArray());

        chunk.UpdateMesh();
    }

    int SetColor(List<Color> colors, Color color)
    {
        int index = colors.IndexOf(color);

        //カラー情報をすべて確認し、Simplified（間略値）をもとにニアイコールを求める
        for (int  i = 0; i < colors.Count; i++)
        {
            Color c = colors[i];
            if (c.r > color.r - GetHalfSimplified && c.r < color.r + GetHalfSimplified &&
                c.g > color.g - GetHalfSimplified && c.g < color.g + GetHalfSimplified &&
                c.b > color.b - GetHalfSimplified && c.b < color.b + GetHalfSimplified &&
                c.a > color.a - GetHalfSimplified && c.a < color.a + GetHalfSimplified) 
            {
                return i;
            }
        }

        //新しい色だと認識されると配列に追加される
        colors.Add(color);
        return colors.Count - 1;
    }

    bool CheckMesh(Vector3 center, Vector3 dir, out Color color)
    {
        Vector3 pos = center - dir * VoxelData.fSize * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(pos, dir, out hit, VoxelData.fSize)) 
        {
            //ヒットしたら位置をもとにUV情報を取得する
            Vector2 uv = hit.textureCoord;
            uv.x *= meshTexture.width;
            uv.y *= meshTexture.height;
            color = meshTexture.GetPixel((int)uv.x, (int)uv.y);
            return true;
        }
        else
        {
            color = Color.black;
            //ヒットしなかった場合はreturn false
            return false;
        }
    }
}
