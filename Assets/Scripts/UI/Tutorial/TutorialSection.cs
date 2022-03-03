using System;
using TMPro;
using UnityEngine;
using Assets.Scripts.UI.Tutorial;

namespace Assets.Scripts.UI.Tutorial
{
    [Serializable]
    public struct TextAndHighlight
    {
        public string TutorialString;
        public Vector3 MaskPosition;
        public Vector3 MaskScale;
        public TutorialActivatedButton TutorializedButton;
    }

    public class TutorialSection : MonoBehaviour
    {
        
        public TextAndHighlight[] Tutorials = new TextAndHighlight[1];
        public MovingUIObject TutorialMaskImage;
        public TutorialSection NextTutorialSection;
        public TextMeshProUGUI TutorialText;
        private int _tutorialIndex;

        public void Activate()
        {
            this.gameObject.SetActive(true);
            if (Tutorials.Length > 0)
            {
                _tutorialIndex = 0;
                WriteNextTutorialMessage();
            }
        }

        private void WriteNextTutorialMessage()
        {
            TutorialMaskImage.SendToNewTransform(Tutorials[_tutorialIndex].MaskPosition, Tutorials[_tutorialIndex].MaskScale);
            TutorialText.text = Tutorials[_tutorialIndex].TutorialString;
            if (Tutorials[_tutorialIndex].TutorializedButton != null)
            {
                Tutorials[_tutorialIndex].TutorializedButton.Activate();
            }
        }

        public void NextText()
        {
            _tutorialIndex++;
            if (_tutorialIndex < Tutorials.Length)
            {
                WriteNextTutorialMessage();
            }
            else
            {
                if (NextTutorialSection)
                {
                    NextTutorialSection.Activate();
                    this.gameObject.SetActive(false);
                }
                else
                {
                    SceneController sceneController= FindObjectOfType<SceneController>();
                    if (sceneController)
                    {
                        sceneController.LoadScene(sceneController.GameSceneName);
                    }

                    gameObject.SetActive(false);
                }
            }
        }


    }
}