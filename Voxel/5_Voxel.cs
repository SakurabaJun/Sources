using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    [SerializeField, Tooltip("MeshFilterの参照")] 
    MeshFilter meshFilter;

	[SerializeField, Tooltip("MeshRendererの参照")]
    MeshRenderer meshRenderer;

    [SerializeField, Tooltip("Rigidbodyの参照")]
    Rigidbody rigidbody;

    public Rigidbody GetRigitbody
    {
        get { return rigidbody; }
    }

    int vertexIndex = 0;

    List<Vector3> vertices = new List<Vector3>();

    List<int> triangles = new List<int>();

    List<Vector2> uvs = new List<Vector2>();

    void Start()
    {
        Destroy(gameObject, Random.Range(5, 10));
    }

    /// <summary>
    /// ボクセルの頂点、UVセット
    /// </summary>
    /// <param name="localScale">ローカル座標</param>
    /// <param name="type">ボクセルのタイプ（テクスチャーID）</param>
    public void AddVoxelData(Vector3 localScale, int type)
    {
        transform.localScale = localScale;

        int amount = meshRenderer.material.mainTexture.width / VoxelData.colorChunkSize;

        //一面6頂点を6回追加する
        for (int j = 0; j < 6; j++)
        {
            for (int i = 0; i < 6; i++)
            {
                int triangleIndex = VoxelData.voxelTris[j, i];
                vertices.Add((VoxelData.voxelVerts[triangleIndex]));
                triangles.Add(vertexIndex);
                uvs.Add(VoxelData.GetUVFromType(amount, i, type));
                vertexIndex++;
            }
        }

        CreateMesh();
    }

    /// <summary>
    /// メッシュの生成
    /// </summary>
    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

}
