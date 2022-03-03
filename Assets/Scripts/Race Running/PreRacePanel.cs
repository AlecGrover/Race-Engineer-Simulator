using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreRacePanel : MonoBehaviour
{

    private RaceRunner _raceRunner;
    private PlayerEngineer _player;

    public TextMeshProUGUI TireTypeText;
    public TextMeshProUGUI TireDescriptionText;
    public Image TireImage;

    private TireUI _currentTireUI;


    // Start is called before the first frame update
    void Start()
    {
        _raceRunner = FindObjectOfType<RaceRunner>();
        _player = FindObjectOfType<PlayerEngineer>();
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentTireUI.TireType != _player.pitPanel.GetStartingTireUI().TireType)
        {
            UpdateUI();
        }
    }

    public void CycleTire(int direction)
    {
        _player.pitPanel.CycleStartingTireSelection(direction);
        UpdateUI();
    }

    private void UpdateUI()
    {
        _currentTireUI = _player.pitPanel.GetStartingTireUI();
        TireTypeText.text = _currentTireUI.GetName();
        TireTypeText.color = _currentTireUI.TireColor;
        TireDescriptionText.text = _currentTireUI.DescriptionString;
        TireImage.sprite = _currentTireUI.TireSprite;
    }

    public void StartRace()
    {
        _raceRunner.StartRace();
        this.gameObject.SetActive(false);
    }


}
