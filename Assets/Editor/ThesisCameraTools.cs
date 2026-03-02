using UnityEditor;
using UnityEngine;

public class ThesisCameraTools
{
    [MenuItem("Thesis/Snap to Front View")]
    public static void SnapFront()
    {
        SceneView.lastActiveSceneView.LookAt(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), 10f);
    }

    [MenuItem("Thesis/Snap to Top View")]
    public static void SnapTop()
    {
        SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.Euler(90, 0, 0), 15f);
    }
}