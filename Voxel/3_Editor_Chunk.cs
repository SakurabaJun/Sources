#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Chunk))]
public class Editor_Chunk : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Chunk chunk = target as Chunk;

        if (GUILayout.Button("Generate"))
        {
            chunk.InitMesh();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Save"))
        {
            chunk.SaveChunk();
        }

        if (GUILayout.Button("Load"))
        {
            chunk.LoadChunk();
        }

        EditorUtility.SetDirty(target);
    }
}
#endif