using UnityEngine;

public class MapButtonHandler : MonoBehaviour
{
    // 绑定到地图场景中 Button1 的 OnClick()
    public void OnButton1()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("亚马逊河");
        }
        else if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("亚马逊河");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("亚马逊河");
        }
    }

    // 绑定到地图场景中 Button2 的 OnClick()
    public void OnButton2()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("密西西比河");
        }
        else if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("密西西比河");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("密西西比河");
        }
    }

    // 绑定到地图场景中 Button3 的 OnClick()
    public void OnButton3()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("莱茵河");
        }
        else if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("莱茵河");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("莱茵河");
        }
    }
}


