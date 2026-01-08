using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugForceLoad : MonoBehaviour
{
    // 若勾选，进入场景时立即加载目标场景（用于调试）
    public bool autoLoadOnStart = false;
    // 若不希望自动加载，可设置为某个按键（如 Space）来触发
    public KeyCode loadKey = KeyCode.None;
    // 要跳转的场景名
    public string sceneName = "地图";

    void Start()
    {
        if (autoLoadOnStart)
        {
            Debug.Log("[DebugForceLoad] autoLoadOnStart true，立即加载：" + sceneName);
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TransitionToScene(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }
    }

    void Update()
    {
        if (loadKey != KeyCode.None && Input.GetKeyDown(loadKey))
        {
            Debug.Log("[DebugForceLoad] 按键触发加载：" + sceneName);
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TransitionToScene(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }
    }

    // 可在 Inspector 的 Button OnClick() 中绑定此方法以强制加载
    public void LoadNow()
    {
        Debug.Log("[DebugForceLoad] LoadNow 被调用，加载：" + sceneName);
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}


