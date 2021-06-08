using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.Common;

/// <summary>
/// DetectedPanelを探すクラス
/// </summary>
public class DetectedPanelFinder : MonoBehaviour
{
    /// <summary>
    /// すべてのDetectedPlaneVisualizer
    /// </summary>
    public static List<DetectedPlaneVisualizer> dpv = new List<DetectedPlaneVisualizer>();

    /// <summary>
    /// すべてのDetectedPlaneVisualizerからランダムな場所を返す
    /// </summary>
    /// <returns>ランダムな値を返す（DetectedPlaneVisualizerがない場合はVector3.zeroを返す）</returns>
    public static Vector3 FindRandomPosition()
    {
        List<Vector3> allPos = new List<Vector3>();

        //すべてのDetectedPlaneVisualizerをforで検索
        foreach (DetectedPlaneVisualizer d in dpv)
        {
            Matrix4x4 localToWorld = d.transform.localToWorldMatrix;

            Mesh m = d.GetComponent<MeshFilter>().mesh;

            //頂点情報を取得
            foreach (Vector3 v in m.vertices)
            {
                Vector3 wp = localToWorld.MultiplyPoint3x4(v);
                allPos.Add(wp);
            }

            //取得した頂点情報をもとにランダムな3点を取得する
            Vector3[] triangle = new Vector3[3];
            for(int i = 0; i < triangle.Length; i++)
            {
                int index = Random.Range(0, allPos.Count);
                triangle[i] = allPos[index];
                allPos.RemoveAt(index);
            }

            //ランダムな3点からランダムな位置を取得する
            return RandomPointInsideTriangle(triangle[0], triangle[1], triangle[2]);
        }
        print("Detectedパネルが存在しません");
        return Vector3.zero;
    }

    static Vector3 RandomPointInsideTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = Random.value;
        float b = Random.value;

        if (a + b > 1f)
        {
            a = 1f - a;
            b = 1f - b;
        }

        float c = 1f - a - b;

        return a * p1 + b * p2 + c * p3;
    }
}
