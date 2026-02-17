using UnityEditor;
using UnityEngine;

public class OcclusionPrefabResolver : EditorWindow
{
    private long prefabId; // targetPrefab の値

    [MenuItem("Tools/Resolve Occlusion Prefab")]
    static void Init()
    {
        GetWindow<OcclusionPrefabResolver>("Occlusion Prefab Resolver");
    }

    void OnGUI()
    {
        prefabId = EditorGUILayout.LongField("targetPrefab ID", prefabId);

        if (GUILayout.Button("Resolve Prefab"))
        {
            var obj = EditorUtility.InstanceIDToObject((int)prefabId);
            if (obj != null)
            {
                Debug.Log($"Prefab found: {obj.name}", obj);
                Selection.activeObject = obj;
            }
            else
            {
                Debug.LogWarning("No prefab found for targetPrefab ID " + prefabId);
            }
        }
    }
}
