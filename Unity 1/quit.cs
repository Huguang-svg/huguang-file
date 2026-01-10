using UnityEngine;

public class 退出 : MonoBehaviour
{
    // TMP按钮（TextMeshPro 的 Button）点击事件：退出游戏
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 在编辑器里用停止播放来模拟退出
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 打包后真正退出
        Application.Quit();
#endif
    }
}
