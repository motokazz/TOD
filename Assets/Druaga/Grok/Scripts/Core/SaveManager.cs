using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Singletonパターン
    public static SaveManager Instance { get; private set; }

    // PlayerPrefsキー（クロスプラットフォーム、暗号化不要のシンプル実装）
    private const string HIGH_SCORE_KEY = "Druaga_HighScoreFloor";
    private const string CONTINUE_FLOOR_KEY = "Druaga_ContinueFloor";
    private const string GAME_CLEARED_KEY = "Druaga_GameCleared";
    private const string BEST_CLEAR_TIME_KEY = "Druaga_BestClearTime"; // オプション: クリア時間記録

    [Header("Debug")]
    [SerializeField] private bool autoSaveHighScore = true; // Editorでテスト用ON/OFF

    private void Awake()
    {
        // Singleton初期化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // タイトル↔ゲーム耐性
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// ハイスコア保存（到達フロア数）。上書きせず上回った時のみ
    /// GameManager.OnGameOver/OnGameClearから呼ぶ
    /// </summary>
    public void SaveHighScore(int floor)
    {
        if (!autoSaveHighScore) return;

        int currentHigh = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (floor > currentHigh)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, floor);
            PlayerPrefs.Save();
            Debug.Log($"New High Score: Floor {floor}");
        }
    }

    /// <summary>
    /// ハイスコアロード（タイトルUI表示用）
    /// </summary>
    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    /// <summary>
    /// コンティニューフロア保存（死亡時or指定時）
    /// 例: 20F到達でSaveContinueFloor(20);
    /// </summary>
    public void SaveContinueFloor(int floor)
    {
        if (!autoSaveHighScore) return;

        PlayerPrefs.SetInt(CONTINUE_FLOOR_KEY, Mathf.Max(floor, PlayerPrefs.GetInt(CONTINUE_FLOOR_KEY, 0)));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// コンティニューフロアロード（0=無効）
    /// </summary>
    public int LoadContinueFloor()
    {
        return PlayerPrefs.GetInt(CONTINUE_FLOOR_KEY, 0);
    }

    /// <summary>
    /// クリアフラグ保存/ロード（初クリア時スペシャル演出）
    /// </summary>
    public void SetGameCleared(bool cleared)
    {
        PlayerPrefs.SetInt(GAME_CLEARED_KEY, cleared ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsGameCleared()
    {
        return PlayerPrefs.GetInt(GAME_CLEARED_KEY, 0) == 1;
    }

    /// <summary>
    /// オプション: クリア時間記録（秒単位）
    /// </summary>
    public void SaveBestClearTime(float timeSeconds)
    {
        float currentBest = PlayerPrefs.GetFloat(BEST_CLEAR_TIME_KEY, float.MaxValue);
        if (timeSeconds < currentBest)
        {
            PlayerPrefs.SetFloat(BEST_CLEAR_TIME_KEY, timeSeconds);
            PlayerPrefs.Save();
        }
    }

    public float LoadBestClearTime()
    {
        return PlayerPrefs.GetFloat(BEST_CLEAR_TIME_KEY, 0f);
    }

    /// <summary>
    /// 全データリセット（タイトルメニュー用）
    /// </summary>
    public void ResetAllSaves()
    {
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        PlayerPrefs.DeleteKey(CONTINUE_FLOOR_KEY);
        PlayerPrefs.DeleteKey(GAME_CLEARED_KEY);
        PlayerPrefs.DeleteKey(BEST_CLEAR_TIME_KEY);
        PlayerPrefs.Save();
        Debug.Log("All Saves Reset!");
    }
}