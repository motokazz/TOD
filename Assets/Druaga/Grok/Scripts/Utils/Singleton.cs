using UnityEngine;

/// <summary>
/// ジェネリックなSingleton基底クラス
/// シーン内に1つだけ存在するように保証します
/// </summary>
/// <typeparam name="T">シングルトンにするクラス（MonoBehaviourを継承）</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    // 外部からアクセスするためのプロパティ
    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    // シーン内で探す
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        // なければ新規作成
                        GameObject singletonObject = new GameObject(typeof(T).Name + " (Singleton)");
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                        Debug.Log($"[Singleton] {typeof(T).Name} が自動生成されました。");
                    }
                }

                return _instance;
            }
        }
    }

    // 初期化時の保護
    protected virtual void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                // 既に存在している場合、このオブジェクトを破棄
                Destroy(gameObject);
                return;
            }
        }
    }

    // シーンがアンロードされたときのクリーンアップ（必要に応じて）
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    // 明示的に初期化したい場合に呼ぶ（任意）
    public static void Initialize()
    {
        // 必要に応じて事前初期化
        var _ = Instance;
    }
}