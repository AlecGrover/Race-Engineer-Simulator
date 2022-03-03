using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum Scenes
{
    HomePage,
    Game,
    RaceEndScreen,
    TutorialScene
}

// Simplified scene loader
// TODO: This needs to be re-engineered, the current version isn't really a good idea
public class SceneController : MonoBehaviour
{

    [Header("Scene Names")]
    public string HomePageSceneName = "HomePage";
    public string GameSceneName = "GameScene";
    public string RaceEndSceneName = "RaceEndScreen";
    public string TutorialSceneName = "TutorialScene";

    public bool HasTransitionEffect = false;
    public UISlide TransitionUISlide;


    void Awake()
    {
        SceneController[] sceneControllers = FindObjectsOfType<SceneController>();
        if (sceneControllers.Length > 1)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (HasTransitionEffect)
        {
            DontDestroyOnLoad(TransitionUISlide.transform.parent);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (HasTransitionEffect)
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        TransitionUISlide.DestroyWithParentAfterMoving = true;
        TransitionUISlide.StartSlide();
        while (!TransitionUISlide.IsHalfway())
        {
            yield return new WaitForEndOfFrame();
        }
        SceneManager.LoadScene(sceneName);
    }


    public void QuitGame()
    {
        Application.Quit(1);
    }

}
