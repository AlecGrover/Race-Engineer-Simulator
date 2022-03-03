using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tutorial
{
    public class TutorialActivatedButton : MonoBehaviour, ITutorialActivated
    {
        public void Activate()
        {
            Button button = GetComponent<Button>();
            if (button)
            {
                Debug.Log("Yo");
                button.interactable = true;
            }
        }
    }
}
