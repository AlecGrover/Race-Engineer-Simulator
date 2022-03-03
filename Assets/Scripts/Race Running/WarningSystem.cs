using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningSystem : MonoBehaviour
{

    public TextMeshProUGUI WarningSpeakerLabel;
    public TextMeshProUGUI WarningMessageLabel;
    public GameObject WarningMessageGameObject;


    void Start()
    {
        
    }

    public void SendWarning(string messageSenderName, string warningMessage)
    {
        WarningSpeakerLabel.text = messageSenderName;
        WarningMessageLabel.text = warningMessage;
        WarningMessageGameObject.SetActive(true);
    }

    public void CloseWarning()
    {
        WarningMessageGameObject.SetActive(false);
    }

}
