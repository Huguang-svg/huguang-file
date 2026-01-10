using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一劳永逸的“完全重开”工具：清理 DontDestroyOnLoad 残留对象，并重新加载起始场景。
/// 用途：结局回到 UI界面时，确保游戏状态彻底重置，避免遮罩/单例/协程残留导致 UI 失效、对话不触发等问题。
/// </summary>
public class GameRestarter : MonoBehaviour
{
    public static GameRestarter Instance { get; private set; }

    [Tooltip("重开后的起始场景名")]
    public string startSceneName = "UI界面";

    [Tooltip("是否同时重置 Time.timeScale")]
    public bool resetTimeScale = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RestartToStartScene()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        if (resetTimeScale) Time.timeScale = 1f;

        // 先尽量把 FadeManager 的遮罩解除（若存在）
        // 注意：C# 不允许在带 catch 的 try 块里直接 yield return，所以这里改为安全判空调用
        if (FadeManager.Instance != null)
        {
            // 确保不会卡住点击；FadeIn 会在结束时 blocksRaycasts=false
            yield return FadeManager.Instance.FadeIn(0.05f);
        }

        // 清理所有 DontDestroyOnLoad 对象（保留本对象，以便执行后续逻辑）
        CleanupDontDestroyOnLoadExceptSelf();

        // 强制单场景方式加载起始场景
        SceneManager.LoadScene(startSceneName, LoadSceneMode.Single);

        yield return null;
    }

    private void CleanupDontDestroyOnLoadExceptSelf()
    {
        // 通过 Resources 找到所有对象，包括 DontDestroyOnLoad 场景中的
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            var go = all[i];
            if (go == null) continue;

            // 排除资源、编辑器对象
            if (!go.scene.IsValid()) continue;

            if (go.scene.name == "DontDestroyOnLoad")
            {
                if (go == this.gameObject) continue;
                // 避免误删隐藏的系统对象（一般 name 为空或包含 Editor）
                try
                {
                    Destroy(go);
                }
                catch { /* 忽略 */ }
            }
        }

        // 同时把我们已知的单例引用置空（避免持有已 Destroy 的引用）
        // 注意：FadeManager/SceneTransitionManager 的 Instance 是 private set，无法在这里直接清。
        // 但 Destroy 后下一次场景中重新放置/创建即可恢复。
    }
}

