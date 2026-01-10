using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局单例 FadeManager：负责全屏淡入/淡出与场景切换
/// 使用示例：FadeManager.Instance.TransitionToScene("场景名");
/// 将自动在场景之间保持单例并创建全屏遮罩 UI（若未手动配置）。
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Tooltip("默认淡入/淡出时长（秒）")]
    public float defaultFadeDuration = 0.5f;
    [Tooltip("遮罩颜色")]
    public Color fadeColor = Color.black;

    // UI 元素
    private Canvas fadeCanvas;
    private Image fadeImage;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUI();
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
    }

    private void Start()
    {
        // 场景开始时做一次淡入（从黑到透明）
        StartCoroutine(FadeIn(defaultFadeDuration));
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // 进入“选人”场景后，下一帧检查并修复可能被错误禁用的 按钮跳转 根对象
        if (newScene.name == "莱茵河选人")
        {
            StopCoroutine(nameof(FixSelectionSceneNextFrame));
            StartCoroutine(nameof(FixSelectionSceneNextFrame));
        }
    }

    private IEnumerator FixSelectionSceneNextFrame()
    {
        // 等一帧，让场景对象全部 Awake/OnEnable 完成
        yield return null;

        // 找到场景内的 按钮跳转（包含 inactive）并强制激活它所在的 GameObject
        var jumpers = Resources.FindObjectsOfTypeAll<按钮跳转>();
        for (int i = 0; i < jumpers.Length; i++)
        {
            var j = jumpers[i];
            if (j == null) continue;
            if (j.gameObject == null) continue;
            if (j.gameObject.scene.name != "莱茵河选人") continue;

            if (!j.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[FadeManager] FixSelectionScene: 检测到按钮跳转对象被禁用，强制 SetActive(true)。go={j.gameObject.name}");
                j.gameObject.SetActive(true);
            }
        }

        // 同时确保遮罩不阻挡射线
        if (canvasGroup != null && canvasGroup.alpha <= 0.001f)
            canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        // 防止某些流程下 blocksRaycasts 残留为 true，导致新场景 UI 完全点不到
        if (canvasGroup != null && canvasGroup.alpha <= 0.001f)
            canvasGroup.blocksRaycasts = false;
    }

    private void EnsureUI()
    {
        if (fadeCanvas != null && fadeImage != null)
        {
            canvasGroup = fadeImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = fadeImage.gameObject.AddComponent<CanvasGroup>();
            return;
        }

        // 创建 Canvas 根节点
        var canvasGO = new GameObject("FadeCanvas");
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        // 创建遮罩 Image
        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = fadeColor;
        var rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // CanvasGroup 用于平滑控制透明度并阻断交互
        canvasGroup = fadeImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = fadeImage.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 对外接口：开始场景过渡（淡出 -> 加载 -> 淡入）
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        // 避免重复触发导致 blocksRaycasts 一直为 true
        StopAllCoroutines();
        StartCoroutine(TransitionCoroutine(sceneName, defaultFadeDuration, defaultFadeDuration));
    }

    public void TransitionToScene(string sceneName, float fadeOutDuration, float fadeInDuration)
    {
        // 避免重复触发导致 blocksRaycasts 一直为 true
        StopAllCoroutines();
        StartCoroutine(TransitionCoroutine(sceneName, fadeOutDuration, fadeInDuration));
    }

    private IEnumerator TransitionCoroutine(string sceneName, float fadeOutDuration, float fadeInDuration)
    {
        yield return StartCoroutine(FadeOut(fadeOutDuration));
        // 异步加载场景
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
        yield return StartCoroutine(FadeIn(fadeInDuration));
    }

    public IEnumerator FadeOut(float duration)
    {
        if (canvasGroup == null) EnsureUI();
        canvasGroup.blocksRaycasts = true;
        float t = 0f;
        float start = canvasGroup.alpha;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 1f, Mathf.Clamp01(t / duration));
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn(float duration)
    {
        if (canvasGroup == null) EnsureUI();
        float t = 0f;
        float start = canvasGroup.alpha;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / duration));
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
