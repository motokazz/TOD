// ConditionDataEditor.cs
// このスクリプトを Assets/Editor フォルダに配置してください（Editorフォルダが存在しない場合は作成）。
// これにより、インスペクターで conditionType に応じて関連フィールドのみを表示/非表示します。

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConditionData))]
public class ConditionDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // conditionType のシリアライズドプロパティを取得
        SerializedProperty conditionTypeProp = serializedObject.FindProperty("conditionType");

        // conditionType を描画（変更を検知）
        EditorGUILayout.PropertyField(conditionTypeProp);

        ConditionType type = (ConditionType)conditionTypeProp.enumValueIndex;

        // タイプに応じてセクションを描画
        switch (type)
        {
            case ConditionType.KillCount:
                DrawKillCountSection(serializedObject);
                break;
            case ConditionType.PassPosition:
                DrawPassPositionSection(serializedObject);
                break;
            case ConditionType.Time:
                DrawTimeSection(serializedObject);
                break;
            case ConditionType.BlockSpellCount:
                DrawBlockSpellCountSection(serializedObject);
                break;
            // 他のタイプを追加（例: CustomSequence など）
            default:
                EditorGUILayout.HelpBox("このタイプの設定フィールドは未実装です。", MessageType.Info);
                break;
        }

        // 共通フィールド（description など）を描画
        DrawCommonSection(serializedObject);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawKillCountSection(SerializedObject obj)
    {
        SerializedProperty targetEnemyIdsProp = obj.FindProperty("targetEnemyIds");
        SerializedProperty requiredCountProp = obj.FindProperty("requiredCount");

        EditorGUILayout.PropertyField(targetEnemyIdsProp);
        EditorGUILayout.PropertyField(requiredCountProp);
    }

    private void DrawPassPositionSection(SerializedObject obj)
    {
        SerializedProperty targetPositionProp = obj.FindProperty("targetPosition");
        SerializedProperty requireKeyAndOpenDoorProp = obj.FindProperty("requireKeyAndOpenDoor");

        EditorGUILayout.PropertyField(targetPositionProp);
        EditorGUILayout.PropertyField(requireKeyAndOpenDoorProp);
    }

    private void DrawTimeSection(SerializedObject obj)
    {
        SerializedProperty timeLimitProp = obj.FindProperty("timeLimit");

        EditorGUILayout.PropertyField(timeLimitProp);
    }
    private void DrawBlockSpellCountSection(SerializedObject obj)
    {
        SerializedProperty requiredBlockCountProp = obj.FindProperty("requiredBlockCount");

        EditorGUILayout.PropertyField(requiredBlockCountProp);
    }
    private void DrawCommonSection(SerializedObject obj)
    {
        SerializedProperty descriptionProp = obj.FindProperty("description");

        EditorGUILayout.PropertyField(descriptionProp);
    }
}