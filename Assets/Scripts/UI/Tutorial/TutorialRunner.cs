using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.UI.Tutorial;
using UnityEngine;

public class TutorialRunner : MonoBehaviour
{

    public TutorialSection StartingTutorialSection;


    // Start is called before the first frame update
    void Start()
    {
        if (StartingTutorialSection)
        {
            StartingTutorialSection.Activate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
