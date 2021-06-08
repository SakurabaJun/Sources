using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class SpriteRender3D : MonoBehaviour
{
    [SerializeField] Material mat;

    [SerializeField] Sprite sprite;

    private CommandBuffer buffer;
    private Matrix4x4[] matrices;

    private static int idMainTex = Shader.PropertyToID("_MainTex");

    private void Update()
    {
        matrices = new Matrix4x4[] {
            Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one),
            Matrix4x4.TRS(new Vector3(-1, 0, 0), Quaternion.identity, Vector3.one),
            Matrix4x4.TRS(new Vector3( 1, 0, 0), Quaternion.identity, Vector3.one),
        };
        
        var mesh = SpriteToMesh(sprite);
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(idMainTex, sprite.texture);

        buffer = new CommandBuffer();
        foreach (var matrix in matrices)
        {
            buffer.DrawMesh(mesh, matrix, mat, 0, 0, propertyBlock);
        }

        //バッファの登録
        Camera.main.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, buffer);
    }

    private Mesh SpriteToMesh(Sprite sprite)
    {
        var mesh = new Mesh();
        mesh.SetVertices(Array.ConvertAll(sprite.vertices, c => (Vector3)c).ToList());
        mesh.SetUVs(0, sprite.uv.ToList());
        mesh.SetTriangles(Array.ConvertAll(sprite.triangles, c => (int)c), 0);

        return mesh;
    }
}
