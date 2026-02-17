using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singletonパターン
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private FloorManager floorManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AudioManager audioManager; // オプション: BGM/SE制御

    [Header("Game Settings")]
    [SerializeField] private int initialLives = 3;
    [SerializeField] private float zapFadeTime = 1.5f; // ZAP時の暗転時間

    [Header("Databases")]
    [SerializeField] private EnemyDatabase enemyDatabase;

    // 状態
    private int lives;
    private int currentFloor = 0;
    private bool gameActive = false;
    private bool gameCleared = false;

    // イベント（他のスクリプトから通知）
    public System.Action<int> OnFloorLoaded;      // フロアロード時
    public System.Action<int> OnLivesChanged;     // 残機変更時
    public System.Action OnGameOver;              // ゲームオーバー
    public System.Action<int> OnGameClear;  // int = 最終到達フロア

    private void Awake()
    {
        // Singleton初期化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移耐性（タイトル用）
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初期化
        lives = initialLives;
    }

    private void Start()
    {
        // ゲーム開始（タイトルから呼ぶ想定）
        if (!gameActive)
        {
            // EnemyDatabase が割り当てられているかチェック
            if (enemyDatabase == null)
            {
                Debug.LogError("EnemyDatabase がインスペクターで割り当てられていません");
            }

            // FloorManager を先にロード（Instanceを確実に作る）
            if (floorManager == null)
            {
                // シーンにFloorManagerが存在することを想定
                var floorManagerObj = floorManager;
                if (floorManagerObj == null)
                {
                    Debug.LogError("シーンにFloorManagerオブジェクトがありません");
                }
            }

            StartNewGame();
        }
    }

    /// <summary>
    /// 新規ゲーム開始（残機リセット、1Fロード）
    /// </summary>
    public void StartNewGame()
    {
        lives = initialLives;
        currentFloor = 1;
        gameActive = true;
        gameCleared = false;
        uiManager?.UpdateLives(lives);
        LoadFloor(currentFloor);
        audioManager?.PlayBGM("TitleToGame"); // オプション
    }

    /// <summary>
    /// 通常死亡処理（残機-1、現在フロア再開）
    /// PlayerControllerから呼ぶ
    /// </summary>
    public void OnPlayerDeath()
    {
        lives--;
        uiManager?.UpdateLives(lives);

        if (lives <= 0)
        {
            GameOver();
            return;
        }

        // 現在フロア再開（アイテム保持）
        floorManager.RestartCurrentFloor();
        audioManager?.PlaySE("Death");
    }

    /// <summary>
    /// ZAP処理（下層ワープ + アイテム一部リセット）
    /// FloorManagerや条件スクリプトから呼ぶ（事前にlives--推奨）
    /// 例: ZapPlayer(Random.Range(1, currentFloor));
    /// </summary>
    public void ZapPlayer(int targetFloor)
    {
        if (!gameActive) return;

        // 残機チェック（呼び元で--済み想定、念のため）
        if (lives <= 0)
        {
            GameOver();
            return;
        }

        StartCoroutine(DoZap(targetFloor));
        audioManager?.PlaySE("Zap");
    }

    private IEnumerator DoZap(int targetFloor)
    {
        // フェードアウト（UI or Screen overlay）
        uiManager?.FadeOut(zapFadeTime);

        yield return new WaitForSeconds(zapFadeTime);

        // 現在のフロアアンロード
        floorManager.UnloadCurrentFloor();

        // ZAP特有: アイテムリセット（例: マトック回数0、ブーツ無効、剣パワーダウン）
        if (ItemInventory.Instance != null)
        {
            ItemInventory.Instance.OnZapReset(); // 実装必須
        }

        // 新フロアロード
        currentFloor = Mathf.Clamp(targetFloor, 1, currentFloor - 1);
        LoadFloor(currentFloor);
    }

    /// <summary>
    /// フロアクリア処理（次フロアへ）
    /// FloorManagerから呼ぶ（宝箱取ったかで表/裏決定）
    /// </summary>
    public void OnFloorCleared()
    {
        int nextFloor = floorManager.CurrentFloorNum; // FloorManager実装: treasureTaken ? current+1 : backEquivalent

        if (nextFloor > 60)
        {
            GameClear();
            return;
        }

        currentFloor = nextFloor;
        LoadFloor(nextFloor);
        audioManager?.PlaySE("FloorClear");
    }

    /// <summary>
    /// フロアロード（内部用）
    /// </summary>
    public void LoadFloor(int floor)
    {
        floorManager.LoadFloor(floor);
        uiManager?.UpdateFloor(floor);
        OnFloorLoaded?.Invoke(floor);
        gameActive = true;
    }

    /// <summary>
    /// ゲームオーバー
    /// </summary>
    private void GameOver()
    {
        gameActive = false;
        uiManager?.ShowGameOver();
        OnGameOver?.Invoke();

        // ハイスコア保存（オプション）
        SaveManager.Instance?.SaveHighScore(currentFloor);

        // 3秒後タイトル戻り（or キー待ち）
        StartCoroutine(ReturnToTitle(3f));
        audioManager?.PlayBGM("GameOver");
    }

    /// <summary>
    /// ゲームクリア（60Fドルアーガ撃破後）
    /// </summary>
    public void GameClear()
    {
        gameActive = false;
        gameCleared = true;
        uiManager?.ShowGameClear(currentFloor);
        OnGameClear?.Invoke(currentFloor);

        // ハイスコア
        SaveManager.Instance?.SaveHighScore(60);

        StartCoroutine(ReturnToTitle(5f));
        audioManager?.PlayBGM("Clear");
    }

    private IEnumerator ReturnToTitle(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Title"); // タイトルシーン想定
    }

    // コンティニュー用（アーケード風、残機回復で途中から）
    public void ContinueGame(int startFloor)
    {
        lives = initialLives;
        currentFloor = startFloor;
        LoadFloor(startFloor);
        gameActive = true;
    }

    // Getter
    public int Lives => lives;
    public int CurrentFloor => currentFloor;
    public bool IsGameActive => gameActive;
}