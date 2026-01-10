using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI淡出控制 : MonoBehaviour
{
    // 防止同一时刻多次触发场景切换/淡出
    private static bool isTransitioning = false;
    // 单例引用，避免多个持久化对象并发存在
    private static UI淡出控制 instance = null;

    public Animator animator;
    [Tooltip("Animator 中播放淡出的状态名，默认 FadeOut")]
    public string stateName = "FadeOut";
    [Tooltip("淡出动画结束后要切换到的场景名")]
    public string nextSceneName = "地图";
    [Tooltip("在动画结束后额外等待的秒数")]
    public float extraDelay = 0f;

    void Start()
    {
        Debug.Log("[UI淡出控制] Start() -> 请求 SceneTransitionManager 切换到: " + nextSceneName);

        if (SceneTransitionManager.Instance == null)
        {
            // 如果项目里没有 SceneTransitionManager，则动态创建一个单例实例
            var mgrGo = new GameObject("SceneTransitionManager");
            mgrGo.AddComponent<SceneTransitionManager>();
        }

        // 如果 Animator 可用，传递给管理器让其优先使用 Animator；否则交由管理器创建遮罩并执行淡出
        SceneTransitionManager.Instance.TransitionToScene(nextSceneName, animator, stateName, null, 0.5f, extraDelay);
        // 本脚本不再负责切换，销毁自己
        Destroy(this.gameObject);
    }

    void Awake()
    {
        // 单例保护：已有实例则销毁自己（防止返回场景时重复存在）
        if (instance != null && instance != this)
        {
            Debug.Log("[UI淡出控制] Awake: 已存在实例，销毁自身");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        // 保持本对象在场景切换时不被销毁
        DontDestroyOnLoad(this.gameObject);
    }

    IEnumerator WaitForAnimationAndLoad()
    {
        // 异步加载场景但先不激活
        var op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

        // 等一帧让 Animator 进入播放状态
        yield return null;

        // 等到 Animator 进入指定状态
        Debug.Log("[UI淡出控制] 等待 Animator 进入状态: " + stateName);
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        // 等待动画播放完成（使用 normalizedTime，以应对不同速度和片段长度）
        Debug.Log("[UI淡出控制] 等待动画播放完成...");
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f || animator.IsInTransition(0))
            yield return null;

        // 额外等待（使用不受 timeScale 影响的时间）
        float elapsed = 0f;
        while (elapsed < extraDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 等异步加载进度到达可激活（通常为 0.9）
        Debug.Log("[UI淡出控制] 等待异步加载进度, 当前: " + op.progress);
        while (op.progress < 0.9f)
            yield return null;

        // 确保再渲染一帧，避免短暂的未淡出闪烁
        yield return new WaitForEndOfFrame();

        // 激活新场景
        Debug.Log("[UI淡出控制] 激活新场景");
        op.allowSceneActivation = true;
        // 等一帧让新场景完成启动，然后销毁持久化的遮罩对象
        yield return null;
        Debug.Log("[UI淡出控制] 场景已激活，销毁自身并重置状态");
        Destroy(this.gameObject);
        isTransitioning = false;
        instance = null;
    }
}

