using UnityEngine;

public class UIJumpToMap : MonoBehaviour
{
    // 直接让 UI 按钮跳转到 '地图' 场景（可在 Inspector 的 Button OnClick 指向此方法）
    public void JumpToMap()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("地图");
            return;
        }

        // 如果没有管理器，尝试创建一个临时的管理器后再调用
        var mgrGo = new GameObject("SceneTransitionManager");
        mgrGo.AddComponent<SceneTransitionManager>();
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene("地图");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("地图");
    }
}


