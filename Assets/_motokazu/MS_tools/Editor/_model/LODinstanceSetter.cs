using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LODDistanceSetter : EditorWindow
{
    private Camera referenceCamera;
    float[] distances = new float[] { 10f, 20f, 40f }; // LOD0: 10m〜, LOD1: 20m〜, LOD2: 40m〜

    [MenuItem("MS_Tools/Model/LOD 距離で一括設定")]
    static void Init()
    {
        LODDistanceSetter window = (LODDistanceSetter)EditorWindow.GetWindow(typeof(LODDistanceSetter));
        window.titleContent = new GUIContent("LOD Distance Setter");
        window.Show();
    }

    void OnGUI()
    {
        //referenceCamera = (Camera)EditorGUILayout.ObjectField("参照カメラ", referenceCamera, typeof(Camera), true);
        referenceCamera = SceneView.lastActiveSceneView?.camera;


        if (referenceCamera == null)
        {
            EditorGUILayout.HelpBox("カメラを指定してください。", MessageType.Warning);
            return;
        }

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = EditorGUILayout.FloatField($"LOD{i} 開始距離 (m)", distances[i]);
        }

        if (GUILayout.Button("選択中のLODGroupに適用"))
        {
            ApplyToSelected();
        }
    }

    void ApplyToSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("GameObjectが選択されていません。");
            return;
        }

        int count = 0;
        foreach (GameObject go in selected)
        {
            LODGroup lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) continue;

            ApplyLODTransitionByDistance(lodGroup);
            count++;
        }

        Debug.Log($"LOD設定を {count} 個のLODGroupに適用しました。");
    }

    void ApplyLODTransitionByDistance(LODGroup lodGroup)
    {
        Bounds bounds = GetObjectBounds(lodGroup);
        Vector3 boundsCenter = bounds.center;
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        LOD[] lods = lodGroup.GetLODs();

        for (int i = 0; i < lods.Length && i < distances.Length; i++)
        {
            float desiredDistance = distances[i];
            float transitionHeight = GetTransitionHeightAsUnityDoes(bounds, referenceCamera, desiredDistance ,lodGroup.transform.localScale);

            // Unityの制限でLODは screen size の降順でなければならない
            if (i > 0 && transitionHeight >= lods[i - 1].screenRelativeTransitionHeight)
            {
                transitionHeight = lods[i - 1].screenRelativeTransitionHeight - 0.01f;
            }

            lods[i].screenRelativeTransitionHeight = Mathf.Clamp01(transitionHeight);
            Debug.Log($"LOD{i} Distance: {desiredDistance} => TransitionHeight: {transitionHeight}");
        }

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }

    Bounds GetObjectBounds(LODGroup lodGroup)
    {
        Renderer[] renderers = lodGroup.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(lodGroup.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    float GetTransitionHeightAsUnityDoes(Bounds bounds, Camera cam, float distance,Vector3 scale)
    {
        if (cam == null || bounds.size.magnitude == 0)
            return 1f;

        Vector3 worldSpaceSize = bounds.size;
        float objectSize = Mathf.Max(worldSpaceSize.x / scale.x, worldSpaceSize.y / scale.y, worldSpaceSize.z / scale.z); // Unityも最大辺を使う
        objectSize = Mathf.Max(worldSpaceSize.x, worldSpaceSize.y, worldSpaceSize.z);
        if (cam.orthographic)
        {
            float vertical = cam.orthographicSize * 2f;
            return objectSize / vertical;
        }
        else
        {
            float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return objectSize / frustumHeight;
        }
    }


}

