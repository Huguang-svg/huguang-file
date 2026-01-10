using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 在场景为 "莱茵河" 时，让名为 小女孩 的对象在 delay 秒后从画面右下角外移动到目标位置
// 变更（重要）：
// - 若对象为 UI（含 RectTransform 且处于 Screen Space Canvas 下），优先使用 anchoredPosition 进行移动，避免全屏/分辨率/CanvasScaler 变化导致的 world position 漂移。
// - 若对象不是 UI，则保持原来的世界坐标移动逻辑。
public class GirlEntrance : MonoBehaviour
{
    private void OnDestroy()
    {
        Debug.Log($"[GirlEntrance] OnDestroy - {gameObject.name} (scene: {gameObject.scene.name})");
    }

    [Tooltip("只在该场景名匹配时生效")]
    public string sceneName = "莱茵河";

    [Header("时间")]
    [Tooltip("延迟开始（秒）")]
    public float delay = 2f;
    [Tooltip("移动到目标位置所用时长（秒）")]
    public float moveDuration = 1.2f;

    public enum StartCorner { RightBottom, RightTop, LeftBottom, LeftTop }

    [Header("入场起点（通用）")]
    [Tooltip("起始角落，选择右下/右上/左下/左上任意一角作为画面外起始点")]
    public StartCorner startCorner = StartCorner.RightBottom;
    [Tooltip("起始点在视口上的偏移（UI：基于 Canvas 尺寸；世界物体：基于 ViewportToWorldPoint）")]
    public Vector2 offscreenOffset = new Vector2(0.15f, 0.15f);

    [Header("UI 模式（RectTransform）目标点")]
    [Tooltip("若对象为 UI：启用后使用下面的 anchoredPosition 作为终点（相对于父节点）。否则使用物体初始 anchoredPosition 作为终点。")]
    public bool useSpecifiedUITarget = true;
    [Tooltip("UI 终点（anchoredPosition）。例如希望停在“画面中间偏右”，可以先在场景里摆好位置，再把 useSpecifiedUITarget 关掉；或者在这里填一个偏右的值。")]
    public Vector2 specifiedUITargetAnchoredPosition = new Vector2(260f, 0f);

    [Header("世界坐标（非 UI 时才使用，保留兼容）")]
    [Tooltip("启用后使用下面的手动坐标：manualStartPosition 为起点，manualTargetPosition 为终点（覆盖其他逻辑）")]
    public bool useManualPositions = false;
    [Tooltip("手动起点世界坐标（启用 useManualPositions 时生效）")]
    public Vector3 manualStartPosition = new Vector3(690f, -517f, 0f);
    [Tooltip("手动终点世界坐标（启用 useManualPositions 时生效）；若为 Vector3.zero 则使用物体初始位置")]
    public Vector3 manualTargetPosition = Vector3.zero;

    [Header("到达后显示对话框")]
    [Tooltip("到达目标后显示的对话面板（UI GameObject），应包含 CanvasGroup 或会自动添加")]
    public GameObject dialogPanel;
    [Tooltip("对话框淡入时长（秒）")]
    public float dialogFadeDuration = 0.5f;

    [Header("到达后淡出设置")]
    [Tooltip("到达并淡入对话框后，等待多少秒再开始淡出女孩与对话框（秒）")]
    public float fadeOutDelayAfterArrival = 3f;
    [Tooltip("淡出持续时长（秒），设为0则直接隐藏")]
    public float fadeOutDuration = 0.5f;
    [Tooltip("淡出完成后是否将对象设为 inactive")]
    public bool deactivateAfterFade = true;
    [Tooltip("淡出完成后要跳转到的场景名（留空则不跳转）")]
    public string nextSceneName = "莱茵河选人";
    [Tooltip("到达并淡入对话框后是否自动开始淡出（否则需要外部调用 TriggerFadeOut）")]
    public bool autoFadeOnArrival = false;

    [Tooltip("运行时如果对象是 UI（含 RectTransform），自动为其添加 CanvasGroup 以保证渐隐")]
    public bool autoAddCanvasGroupToGirlIfUI = true;

    [Header("入场行走晃动（模拟走路）")]
    [Tooltip("是否启用入场晃动")]
    public bool enableWalkBob = true;
    [Tooltip("晃动频率（Hz），越大步伐越快")]
    public float walkBobFrequency = 8f;
    [Tooltip("垂直位移幅度（UI：像素；世界：世界单位），小数值推荐")]
    public float walkBobAmount = 6f;
    [Tooltip("左右/旋转晃动角度（度），例如 3 表示在 Z 轴±3度之间摆动")]
    public float walkBobRotationAngle = 3f;

    // runtime
    private Camera mainCam;
    private Vector3 targetPositionWorld;
    private RectTransform rect;
    private RectTransform rectParent;
    private Vector2 targetAnchored;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != sceneName) return;

        mainCam = Camera.main;
        rect = GetComponent<RectTransform>();
        rectParent = rect != null ? rect.parent as RectTransform : null;

        // 确保对话面板有 CanvasGroup
        if (dialogPanel != null)
        {
            var cgPanel = dialogPanel.GetComponent<CanvasGroup>();
            if (cgPanel == null)
            {
                dialogPanel.AddComponent<CanvasGroup>();
                Debug.Log($"GirlEntrance: 自动为 dialogPanel 添加 CanvasGroup（{dialogPanel.name}）");
            }
        }

        // 若对象为 UI，则可添加 CanvasGroup
        if (autoAddCanvasGroupToGirlIfUI && rect != null)
        {
            var cgGirl = GetComponent<CanvasGroup>();
            if (cgGirl == null)
            {
                gameObject.AddComponent<CanvasGroup>();
                Debug.Log($"GirlEntrance: 自动为对象添加 CanvasGroup（{gameObject.name}）");
            }
        }

        // --- UI 路径：使用 anchoredPosition，避免分辨率/全屏变化导致漂移 ---
        if (rect != null && rectParent != null)
        {
            // 终点：默认使用初始 anchoredPosition，或使用指定 UI 终点
            targetAnchored = useSpecifiedUITarget ? specifiedUITargetAnchoredPosition : rect.anchoredPosition;

            // 起点：从右下角（或其他角落）“画面外”进入
            Vector2 startAnchored = ComputeUIOffscreenAnchoredPosition(rectParent, startCorner, offscreenOffset);

            rect.anchoredPosition = startAnchored;
            Debug.Log($"GirlEntrance(UI): startAnchored={startAnchored} -> targetAnchored={targetAnchored}");

            StartCoroutine(DelayedMoveUI());
            return;
        }

        // --- 世界坐标路径（非 UI）保留 ---
        // 优先：完全手动坐标
        if (useManualPositions)
        {
            if (manualTargetPosition == Vector3.zero)
                manualTargetPosition = transform.position;

            targetPositionWorld = manualTargetPosition;
            transform.position = manualStartPosition;
            Debug.Log($"GirlEntrance(World): 使用手动坐标 start={manualStartPosition} -> target={manualTargetPosition}");
            StartCoroutine(DelayedMoveWorld());
            return;
        }

        // 默认终点为当前初始位置
        targetPositionWorld = transform.position;

        // 起点：根据 Viewport 角落算一个在画面外的世界坐标
        Vector3 startPos = targetPositionWorld;
        if (mainCam != null)
        {
            Vector3 dirToTarget = targetPositionWorld - mainCam.transform.position;
            float distance = Mathf.Abs(Vector3.Dot(dirToTarget, mainCam.transform.forward));
            if (distance < 0.1f) distance = 1f;

            Vector2 baseVP = Vector2.zero;
            switch (startCorner)
            {
                case StartCorner.RightBottom: baseVP = new Vector2(1f, 0f); break;
                case StartCorner.RightTop: baseVP = new Vector2(1f, 1f); break;
                case StartCorner.LeftBottom: baseVP = new Vector2(0f, 0f); break;
                case StartCorner.LeftTop: baseVP = new Vector2(0f, 1f); break;
            }

            Vector2 vpPos = new Vector2(
                baseVP.x == 0f ? -offscreenOffset.x : 1f + offscreenOffset.x,
                baseVP.y == 0f ? -offscreenOffset.y : 1f + offscreenOffset.y
            );

            Vector3 vp = new Vector3(vpPos.x, vpPos.y, distance + mainCam.nearClipPlane + 0.5f);
            startPos = mainCam.ViewportToWorldPoint(vp);
        }

        transform.position = startPos;
        Debug.Log($"GirlEntrance(World): 将物体移动到起点 startPos = {startPos}, target = {targetPositionWorld}");

        StartCoroutine(DelayedMoveWorld());
    }

    // 计算 UI 在父容器（RectTransform）中的“画面外起点”（anchoredPosition）
    private Vector2 ComputeUIOffscreenAnchoredPosition(RectTransform parent, StartCorner corner, Vector2 offset01)
    {
        // parent.rect.size 是父容器的本地尺寸（像素）
        Vector2 size = parent.rect.size;

        // offset01 是“以父尺寸比例”为单位的偏移（例如 0.15 表示再外移 15% 的宽/高）
        float ox = Mathf.Abs(offset01.x) * size.x;
        float oy = Mathf.Abs(offset01.y) * size.y;

        // anchoredPosition 的原点与锚点/pivot有关；但只要你的“介绍员”锚点固定（例如居中锚点），这个计算会稳定。
        // 这里按“父容器中心为 (0,0)”的常见 UI 布局来计算：
        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;

        switch (corner)
        {
            case StartCorner.RightBottom:
                return new Vector2(halfW + ox, -halfH - oy);
            case StartCorner.RightTop:
                return new Vector2(halfW + ox, halfH + oy);
            case StartCorner.LeftBottom:
                return new Vector2(-halfW - ox, -halfH - oy);
            case StartCorner.LeftTop:
                return new Vector2(-halfW - ox, halfH + oy);
        }
        return new Vector2(halfW + ox, -halfH - oy);
    }

    private IEnumerator DelayedMoveUI()
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(MoveToTargetUI(moveDuration));
    }

    private IEnumerator MoveToTargetUI(float duration)
    {
        Vector2 from = rect.anchoredPosition;
        Vector2 to = targetAnchored;

        if (duration <= 0f)
        {
            rect.anchoredPosition = to;
            yield break;
        }

        float t = 0f;
        float elapsed = 0f;
        Quaternion startRot = rect.localRotation;

        while (t < duration)
        {
            float dt = Time.deltaTime;
            t += dt;
            elapsed += dt;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            Vector2 basePos = Vector2.Lerp(from, to, k);

            if (enableWalkBob)
            {
                float phase = elapsed * walkBobFrequency * Mathf.PI * 2f;
                float bob = Mathf.Sin(phase) * walkBobAmount;
                float rotZ = Mathf.Sin(phase) * walkBobRotationAngle;

                rect.anchoredPosition = basePos + new Vector2(0f, bob);
                rect.localRotation = startRot * Quaternion.Euler(0f, 0f, rotZ);
            }
            else
            {
                rect.anchoredPosition = basePos;
            }

            yield return null;
        }

        rect.anchoredPosition = to;
        rect.localRotation = startRot;

        if (dialogPanel != null)
        {
            StartCoroutine(ShowDialogCoroutine());
        }
    }

    private IEnumerator DelayedMoveWorld()
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(MoveToTargetWorld(moveDuration));
    }

    private IEnumerator MoveToTargetWorld(float duration)
    {
        Vector3 from = transform.position;
        Vector3 to = targetPositionWorld;
        if (duration <= 0f)
        {
            transform.position = to;
            yield break;
        }

        float t = 0f;
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;

        while (t < duration)
        {
            float dt = Time.deltaTime;
            t += dt;
            elapsed += dt;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            Vector3 basePos = Vector3.Lerp(from, to, k);

            if (enableWalkBob)
            {
                float phase = elapsed * walkBobFrequency * Mathf.PI * 2f;
                float bob = Mathf.Sin(phase) * walkBobAmount;
                float rotZ = Mathf.Sin(phase) * walkBobRotationAngle;
                transform.position = basePos + new Vector3(0f, bob, 0f);
                transform.rotation = startRot * Quaternion.Euler(0f, 0f, rotZ);
            }
            else
            {
                transform.position = basePos;
            }

            yield return null;
        }

        transform.position = to;
        transform.rotation = startRot;

        if (dialogPanel != null)
        {
            StartCoroutine(ShowDialogCoroutine());
        }
    }

    private IEnumerator ShowDialogCoroutine()
    {
        if (dialogPanel == null) yield break;

        dialogPanel.SetActive(true);
        CanvasGroup cg = dialogPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = dialogPanel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < dialogFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / dialogFadeDuration);
            yield return null;
        }
        cg.alpha = 1f;

        if (autoFadeOnArrival)
            StartCoroutine(DelayedFadeOutAfterArrival());
    }

    private IEnumerator DelayedFadeOutAfterArrival()
    {
        yield return StartCoroutine(DoFadeOutAfterDelay(fadeOutDelayAfterArrival));
    }

    public void TriggerFadeOut(float delay = -1f)
    {
        float useDelay = delay >= 0f ? delay : 0f;
        StartCoroutine(DoFadeOutAfterDelay(useDelay));
    }

    private IEnumerator DoFadeOutAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (fadeOutDuration <= 0f)
        {
            if (dialogPanel != null && deactivateAfterFade) dialogPanel.SetActive(false);
            if (gameObject != null && deactivateAfterFade) gameObject.SetActive(false);
            yield break;
        }

        if (dialogPanel != null)
            StartCoroutine(FadeOutGameObject(dialogPanel, fadeOutDuration, deactivateAfterFade));
        StartCoroutine(FadeOutGameObject(this.gameObject, fadeOutDuration, deactivateAfterFade));

        if (!string.IsNullOrEmpty(nextSceneName))
            SpawnPersistentSceneLoader(fadeOutDuration, nextSceneName);

        yield return new WaitForSeconds(fadeOutDuration);
    }

    private IEnumerator FadeOutGameObject(GameObject go, float duration, bool deactivate)
    {
        if (go == null) yield break;

        var cg = go.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float t = 0f;
            float start = cg.alpha;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }
            cg.alpha = 0f;
            if (deactivate) go.SetActive(false);
            yield break;
        }

        var renderers = go.GetComponentsInChildren<Renderer>(true);
        var spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>(true);

        var originalMatColors = new Dictionary<Material, Color>();
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_Color") && !originalMatColors.ContainsKey(mat))
                    originalMatColors[mat] = mat.color;
            }
        }
        var originalSRColors = new Dictionary<SpriteRenderer, Color>();
        foreach (var sr in spriteRenderers)
        {
            originalSRColors[sr] = sr.color;
        }

        if (renderers.Length == 0 && spriteRenderers.Length == 0 && originalMatColors.Count == 0 && originalSRColors.Count == 0)
        {
            Debug.LogWarning($"GirlEntrance: 目标对象 {go.name} 没有 CanvasGroup、Renderer 或 SpriteRenderer，无法平滑淡出（将直接 SetActive(false)）。");
            if (deactivate) go.SetActive(false);
            yield break;
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, time / duration);
            foreach (var kv in originalMatColors)
            {
                var mat = kv.Key;
                var col = kv.Value;
                col.a = a;
                mat.color = col;
            }
            foreach (var kv in originalSRColors)
            {
                var sr = kv.Key;
                var col = kv.Value;
                col.a = a;
                sr.color = col;
            }
            yield return null;
        }

        foreach (var kv in originalMatColors)
        {
            var mat = kv.Key;
            var col = kv.Value;
            col.a = 0f;
            mat.color = col;
        }
        foreach (var kv in originalSRColors)
        {
            var sr = kv.Key;
            var col = kv.Value;
            col.a = 0f;
            sr.color = col;
        }

        if (deactivate) go.SetActive(false);
    }

    private void SpawnPersistentSceneLoader(float delay, string sceneName)
    {
        var loaderGO = new GameObject("GirlEntrance_SceneLoader");
        var helper = loaderGO.AddComponent<SceneLoaderHelper>();
        helper.Init(delay, sceneName);
        DontDestroyOnLoad(loaderGO);
    }

    private class SceneLoaderHelper : MonoBehaviour
    {
        private float delay;
        private string sceneName;

        public void Init(float d, string s)
        {
            delay = d;
            sceneName = s;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            try
            {
                var fm = FadeManager.Instance;
                if (fm != null)
                {
                    fm.TransitionToScene(sceneName);
                    Destroy(this.gameObject);
                    yield break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"GirlEntrance: 直接调用 FadeManager 出现异常：{ex.Message}");
            }

            try
            {
                var fadeManagerType = Type.GetType("FadeManager");
                if (fadeManagerType != null)
                {
                    object instance = null;
                    var prop = fadeManagerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null) instance = prop.GetValue(null);
                    else
                    {
                        var field = fadeManagerType.GetField("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (field != null) instance = field.GetValue(null);
                    }

                    if (instance != null)
                    {
                        var method = fadeManagerType.GetMethod("TransitionToScene", new Type[] { typeof(string) });
                        if (method != null)
                        {
                            method.Invoke(instance, new object[] { sceneName });
                            Destroy(this.gameObject);
                            yield break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"GirlEntrance: SceneLoaderHelper 使用反射调用 FadeManager 跳转异常：{ex.Message}");
            }

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
            Destroy(this.gameObject);
        }
    }
}
