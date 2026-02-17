using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class ConditionDataImporter : EditorWindow
{
    private static readonly string SaveFolder = "Assets/Data/Conditions"; // 保存先フォルダ（事前に作成推奨）

    [MenuItem("TODTools/条件データ/CSVからインポート")]
    private static void ImportConditionsFromCSV()
    {
        string csvPath = EditorUtility.OpenFilePanel("CSVを選択", "", "csv");
        if (string.IsNullOrEmpty(csvPath)) return;

        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            AssetDatabase.Refresh();
        }

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            EditorUtility.DisplayDialog("エラー", "CSVにデータ行がありません", "OK");
            return;
        }

        // ヘッダー: id,type,targetPositionX,targetPositionY,requireKeyAndOpenDoor,requiredCount,targetEnemyIds,description
        int createdCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            string[] cols = ParseCSVLine(line); // カンマ区切り＋エスケープ対応
            if (cols.Length < 6) continue;

            string id = cols[0].Trim();
            string typeStr = cols[1].Trim();
            string posXStr = cols[2].Trim();
            string posYStr = cols[3].Trim();
            string reqKeyStr = cols[4].Trim().ToLower();
            string reqCountStr = cols[5].Trim();
            string targetEnemyIds = cols.Length > 6 ? cols[6].Trim() : "";
            string description = cols.Length > 7 ? cols[7].Trim() : "";

            if (!System.Enum.TryParse<ConditionType>(typeStr, out var conditionType))
            {
                Debug.LogWarning($"無効なconditionTypeをスキップ: {typeStr} (行 {i + 1})");
                continue;
            }

            ConditionData data = ScriptableObject.CreateInstance<ConditionData>();
            data.name = id; // アセット名にも使う
            data.conditionType = conditionType;

            // 共通項目
            if (int.TryParse(posXStr, out int x) && int.TryParse(posYStr, out int y))
            {
                data.targetPosition = new Vector2Int(x, y);
            }
            data.requireKeyAndOpenDoor = reqKeyStr is "true" or "1" or "yes";
            data.requiredCount = int.TryParse(reqCountStr, out int count) ? count : 0;
            data.description = description;

            // KillCount用
            if (conditionType == ConditionType.KillCount && !string.IsNullOrEmpty(targetEnemyIds))
            {
                data.targetEnemyIds = targetEnemyIds
                    .Split(';')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            // 保存
            string assetPath = $"{SaveFolder}/{id}.asset";
            int suffix = 1;
            while (AssetDatabase.LoadAssetAtPath<ConditionData>(assetPath) != null)
            {
                assetPath = $"{SaveFolder}/{id}_{suffix++}.asset";
            }

            AssetDatabase.CreateAsset(data, assetPath);
            createdCount++;
            Debug.Log($"作成: {assetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完了", $"CSVインポート完了！\n{createdCount}件のConditionDataを作成しました。", "OK");
    }

    // 簡易CSVパーサー（"..." 内のカンマ対応）
    private static string[] ParseCSVLine(string line)
    {
        var result = new List<string>();
        bool inQuote = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuote = !inQuote;
                continue;
            }

            if (c == ',' && !inQuote)
            {
                result.Add(current.Trim(' '));
                current = "";
                continue;
            }

            current += c;
        }

        result.Add(current.Trim(' '));
        return result.ToArray();
    }
}