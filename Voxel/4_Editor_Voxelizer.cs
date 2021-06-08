#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Voxelizer))]
public class Editor_Voxelizer : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Voxelizer voxelizer = target as Voxelizer;

        if (GUILayout.Button("Generate"))
        {
            //voxelizer.Generate();
        }
    }
}
#endif
