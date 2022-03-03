using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    public GameObject AskTutorialGameObject;

    void Start()
    {
        AskTutorialGameObject.SetActive(false);
    }

    public void RaceWithTutorialPrompt()
    {
        AskTutorialGameObject.SetActive(true);
    }

}
