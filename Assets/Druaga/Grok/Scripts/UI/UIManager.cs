using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main UI Elements")]
    [SerializeField] private TextMeshProUGUI floorText;             // "FLOOR 01"
    [SerializeField] private TextMeshProUGUI livesText;             // "×3"
    [SerializeField] private TextMeshProUGUI timeText;              // "TIME  00:00"（必要時）
    [SerializeField] private Image fadePanel;                       // 画面全体のフェード用黒パネル

    [Header("Inventory")]
    [SerializeField] private Transform inventoryContainer;          // アイテムアイコンを並べる親
    [SerializeField] private GameObject itemIconPrefab;             // アイコン1つ分のプレハブ（Image + TMP）

    [Header("Game State Screens")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private TextMeshProUGUI finalFloorText;        // クリア時表示用

    [Header("Effects")]
    [SerializeField] private CanvasGroup treasurePopup;             // 宝箱出現時の「TREASURE!」表示
    [SerializeField] private float treasurePopupDuration = 2f;
    [SerializeField] private float fadeDuration = 1.2f;

    [Header("Achievement")]
    [SerializeField] private CanvasGroup achievementPopup;
    [SerializeField] private float achievementPopupDuration = 2f;

    private List<Image> itemIcons = new List<Image>();
    private float currentTime = 0f;
    private bool showTimer = false;

    private void Awake()
    {
        // シングルトン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初期非表示
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (gameClearPanel) gameClearPanel.SetActive(false);
        if (treasurePopup) treasurePopup.gameObject.SetActive(false);

        fadePanel.color = new Color(0, 0, 0, 0);
    }

    private void Update()
    {
        if (showTimer)
        {
            currentTime += Time.deltaTime;
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timeText.text = $"TIME  {minutes:00}:{seconds:00}";
            }
        }
    }

    // ==============================================
    // GameManagerからのコール
    // ==============================================

    public void UpdateFloor(int floor)
    {
        if (floorText != null)
        {
            floorText.text = $"FLOOR {floor:00}";
        }
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"×{lives}";
        }
    }

    public void StartTimer()
    {
        showTimer = true;
        currentTime = 0f;
        if (timeText != null) timeText.gameObject.SetActive(true);
    }

    public void StopTimer()
    {
        showTimer = false;
    }

    public float GetElapsedTime() => currentTime;

    // ==============================================
    // アイテムインベントリ表示
    // ==============================================

    public void RefreshInventory(List<ItemData> ownedItems)
    {
        // 既存アイコン全削除
        foreach (var icon in itemIcons)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        itemIcons.Clear();

        // 新しく生成
        foreach (var item in ownedItems)
        {
            if (item.icon == null) continue;

            var iconObj = Instantiate(itemIconPrefab, inventoryContainer);
            var image = iconObj.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = item.icon;
            }

            itemIcons.Add(image);
        }
    }

    // ==============================================
    // 宝箱出現演出
    // ==============================================

    public void ShowTreasurePopup()
    {
        if (treasurePopup == null) return;

        treasurePopup.gameObject.SetActive(true);
        treasurePopup.alpha = 0f;

        StartCoroutine(FadeInOut(treasurePopup, treasurePopupDuration));
    }

    public void ShowAchievementPopup()
    {
        if (achievementPopup == null) return;

        achievementPopup.gameObject.SetActive(true);
        achievementPopup.alpha = 0f;

        StartCoroutine(FadeInOut(achievementPopup, achievementPopupDuration));
    }

    private IEnumerator FadeInOut(CanvasGroup cg, float duration)
    {
        float half = duration * 0.5f;

        // Fade In
        float t = 0;
        while (t < half)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / half);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(half * 0.6f);

        // Fade Out
        t = 0;
        while (t < half)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / half);
            yield return null;
        }

        cg.gameObject.SetActive(false);
    }

    // ==============================================
    // フェード（ZAP・シーン遷移用）
    // ==============================================

    public void FadeOut(float duration)
    {
        StartCoroutine(Fade(1f, duration));
    }

    public void FadeIn(float duration)
    {
        StartCoroutine(Fade(0f, duration));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        fadePanel.gameObject.SetActive(true);
        float startAlpha = fadePanel.color.a;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, targetAlpha);

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            fadePanel.gameObject.SetActive(false);
        }
    }

    // ==============================================
    // ゲームオーバー / クリア
    // ==============================================

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void ShowGameClear(int finalFloor)
    {
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
            if (finalFloorText != null)
            {
                finalFloorText.text = $"REACHED FLOOR {finalFloor:00}";
            }
            Debug.Log($"ゲームクリア！ 到達フロア: {finalFloor}");
        }
    }

    // ==============================================
    // イベント購読例（GameManager / FloorManager から呼ぶ）
    // ==============================================
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFloorLoaded += UpdateFloor;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnGameClear +=  ShowGameClear;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFloorLoaded -= UpdateFloor;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnGameClear -= ShowGameClear;
        }
    }


}