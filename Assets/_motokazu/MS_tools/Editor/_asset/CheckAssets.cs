using UnityEditor;
using UnityEngine;
/// <summary>
/// 選択アセットのタイプをログに表示
/// </summary>
public class CheckClassType : Editor
{
    [MenuItem("MS_Tools/Assets/Check Select Object:選択アセットのタイプをログに表示")]
    private static void CheckSelectObject()
    {
        Debug.Log(Selection.activeObject.GetType());
    }
}
