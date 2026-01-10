using UnityEngine;

public class RhineInteraction : MonoBehaviour
{
    // 当莱茵河场景中的所有对话与图片变化完成时调用此方法
    public void OnInteractionComplete()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("莱茵河选人");
        }
        else if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("莱茵河选人");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("莱茵河选人");
        }
    }
}


