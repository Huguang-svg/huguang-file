using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class 场景切换 : MonoBehaviour
{
    // 注意：该脚本被多个场景复用（包括“莱茵河选人”场景的控制根对象上）。
    // 为避免它在运行时做任何可能导致 UI 控制对象被销毁/失效的行为，
    // 这里默认仅提供 OnStartButton 供按钮点击调用，不在生命周期里做自动跳转。
    // 绑定到 UI 的 StartButton 的 OnClick()
    // 安全：仅允许在真正的“开始界面/入口界面”使用该按钮。
// 如果该脚本误挂在其它场景（例如 选人场景的控制根）上，也不会主动运行任何逻辑。
public void OnStartButton()
    {
        Debug.Log("[场景切换] OnStartButton clicked");
        if (SceneTransitionManager.Instance != null)
        {
            Debug.Log("[场景切换] 使用 SceneTransitionManager 跳转 到 UI淡出");
            SceneTransitionManager.Instance.TransitionToScene("UI淡出");
        }
        else if (FadeManager.Instance != null)
        {
            Debug.Log("[场景切换] 使用 FadeManager 跳转 到 UI淡出");
            FadeManager.Instance.TransitionToScene("UI淡出");
        }
        else
        {
            Debug.Log("[场景切换] 未找到 Transition 管理器：直接加载 UI淡出");
            SceneManager.LoadScene("UI淡出");
        }
    }
}
