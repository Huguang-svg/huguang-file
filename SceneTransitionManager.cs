using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    // 默认淡出状态名（如果使用 Animator）
    public string defaultStateName = "FadeOut";

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Debug.Log("[SceneTransitionManager] Awake()");

        // 监听场景切换：进入选人场景后做一次“下一帧修复”，避免关键UI脚本被错误禁用导致 Start() 不执行。
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"[SceneTransitionManager] activeSceneChanged: {oldScene.name} -> {newScene.name}");

        if (newScene.name == "莱茵河选人")
        {
            StopCoroutine(nameof(FixSelectionSceneNextFrame));
            StartCoroutine(nameof(FixSelectionSceneNextFrame));
        }
    }

    private IEnumerator FixSelectionSceneNextFrame()
    {
        // 等一帧，让场景对象 Awake/OnEnable 先跑完
        yield return null;

        // 找到场景内的 按钮跳转（包含 inactive）并强制激活
        var jumpers = Resources.FindObjectsOfTypeAll<按钮跳转>();
        int found = 0;
        int fixedCount = 0;
        for (int i = 0; i < jumpers.Length; i++)
        {
            var j = jumpers[i];
            if (j == null || j.gameObject == null) continue;
            if (j.gameObject.scene.name != "莱茵河选人") continue;

            found++;
            if (!j.gameObject.activeInHierarchy)
            {
                fixedCount++;
                Debug.LogWarning($"[SceneTransitionManager] FixSelectionScene: 强制 SetActive(true) -> {j.gameObject.name}");
                j.gameObject.SetActive(true);
            }
        }

        Debug.Log($"[SceneTransitionManager] FixSelectionScene: found={found}, fixed={fixedCount}");
    }

    public bool IsTransitioning { get { return isTransitioning; } }

    /// <summary>
    /// 发起切换。优先使用 animator+stateName；如果没有则使用 cover（CanvasGroup）或自动创建遮罩。
    /// </summary>
    public void TransitionToScene(string sceneName, Animator animator = null, string stateName = null, CanvasGroup cover = null, float fadeDuration = 0.5f, float extraDelay = 0f)
    {
        if (isTransitioning)
        {
            Debug.LogError($"[SceneTransitionManager] 已在切换中，忽略重复请求: {sceneName}");
            return;
        }

        Debug.Log($"[SceneTransitionManager] TransitionToScene -> {sceneName}");
        StartCoroutine(TransitionCoroutine(sceneName, animator, stateName ?? defaultStateName, cover, fadeDuration, extraDelay));
    }

    IEnumerator TransitionCoroutine(string sceneName, Animator animator, string stateName, CanvasGroup cover, float fadeDuration, float extraDelay)
    {
        isTransitioning = true;

        bool createdCover = false;
        CanvasGroup activeCover = cover;

        // 如果没有 animator 可用，则确保有 CanvasGroup 遮罩用于渐变
        if (animator == null)
        {
            if (activeCover == null)
            {
                activeCover = CreateCover();
                createdCover = true;
            }
            // 从 0 -> 1
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                activeCover.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            activeCover.alpha = 1f;
        }
        else
        {
            // animator 路径：尝试播放指定状态并等待结束（带超时回退）
            int layer = 0;
            int stateHash = Animator.StringToHash(stateName);
            float timeout = Mathf.Max(5f, fadeDuration + 5f);

            if (animator.HasState(layer, stateHash))
            {
                animator.Play(stateName, layer, 0f);

                float elapsed = 0f;
                // 等待进入该状态
                while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName) && elapsed < 0.5f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                elapsed = 0f;
                // 等待状态播放完成或超时
                while ((animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f || animator.IsInTransition(layer)) && elapsed < timeout)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            else
            {
                // 找不到状态，使用简单遮罩回退
                if (activeCover == null)
                {
                    activeCover = CreateCover();
                    createdCover = true;
                }
                float t2 = 0f;
                while (t2 < fadeDuration)
                {
                    t2 += Time.unscaledDeltaTime;
                    activeCover.alpha = Mathf.Clamp01(t2 / fadeDuration);
                    yield return null;
                }
                activeCover.alpha = 1f;
            }
        }

        // 额外等待
        float waited = 0f;
        while (waited < extraDelay)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        // 开始异步加载
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // 确保渲染一帧避免闪烁
        yield return new WaitForEndOfFrame();

        // 激活场景
        op.allowSceneActivation = true;

        // 等一帧让新场景完成初始化
        yield return null;

        // 如果我们自动创建了遮罩，平滑淡出遮罩再销毁；如果传入了外部 cover，保留由外部控制
        if (createdCover && activeCover != null)
        {
            float t = 0f;
            float outDuration = Mathf.Max(0.15f, fadeDuration); // 最小短时淡出，避免一瞬间消失
            while (t < outDuration)
            {
                t += Time.unscaledDeltaTime;
                activeCover.alpha = Mathf.Clamp01(1f - (t / outDuration));
                yield return null;
            }
            activeCover.alpha = 0f;
            // 关键：解除射线阻挡，避免销毁前一帧仍然挡住新场景 UI
            activeCover.blocksRaycasts = false;
            Destroy(activeCover.gameObject);
        }

        isTransitioning = false;
    }

    // helper: 创建一个全屏 Canvas + Image + CanvasGroup 用作遮罩
    private CanvasGroup CreateCover()
    {
        var go = new GameObject("SceneTransition_Cover");
        DontDestroyOnLoad(go);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // 强制覆盖排序，保证在所有场景 UI 之上
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10000;
        go.AddComponent<CanvasScaler>();
        // 注意：此遮罩只用于渐变显示，不应拦截 UI 点击。
        // 因此不添加 GraphicRaycaster，避免在某些流程下残留遮罩导致 UI 无法点击。
        // go.AddComponent<GraphicRaycaster>();

        var imgGo = new GameObject("Image");
        imgGo.transform.SetParent(go.transform, false);
        var img = imgGo.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = Vector2.zero;
        img.rectTransform.offsetMax = Vector2.zero;

        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        return cg;
    }
}


