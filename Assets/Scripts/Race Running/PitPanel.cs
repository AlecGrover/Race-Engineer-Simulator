using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// This struct is used to hold the components for a tire type to be displayed in the UI
// TODO: Collapse the TireUI struct and the Tire interface into a singular datatype, then switch things out to a single reference to reduce dependencies
[System.Serializable]
public struct TireUI
{
    public Sprite TireSprite;
    public TireType TireType;
    public Color TireColor;
    public string DescriptionString;

    public string GetName()
    {
        return TireType switch
        {
            (TireType.Hard) => "HARD",
            (TireType.Medium) => "MEDIUM",
            (TireType.Soft) => "SOFT",
            (TireType.Debug) => "DEBUG?",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Tires GetNewTires()
    {
        return TireType switch
        {
            (TireType.Hard) => new HardTires(),
            (TireType.Medium) => new MediumTires(),
            (TireType.Soft) => new SoftTires(),
            (TireType.Debug) => new DebugTires(),
            _ => new HardTires()
        };
    }

    public static string GetSpeedRating(TireType tireType)
    {
        return tireType switch
        {
            (TireType.Hard) => "Low",
            (TireType.Medium) => "Medium",
            (TireType.Soft) => "Fast",
            _ => "Balanced"
        };
    }

    public static int GetExpectedTireLife(TireType tireType)
    {
        return tireType switch
        {
            (TireType.Hard) => new HardTires().ExpectedLifetime,
            (TireType.Medium) => new MediumTires().ExpectedLifetime,
            (TireType.Soft) => new SoftTires().ExpectedLifetime,
            (TireType.Debug) => new DebugTires().ExpectedLifetime,
            _ => new HardTires().ExpectedLifetime
        };
    }

}
public class PitPanel : MonoBehaviour
{

    public bool IsStaticDisplay = false;

    // Used to show the different types of tires available to the player
    [Header("Tire Parameters")]
    public TireUI[] TireOptions = new TireUI[3];
    public TextMeshProUGUI NextTireTypeLabel;

    // Used to set the reference to the player's engineer to link the AI and the engineering controls
    [Header("Player Parameters")]
    public PlayerEngineer Player;

    // Used to display readout information on the telemetry screen
    [Header("Telemetry Parameters")]
    public Image TireImageRenderer;
    public TextMeshProUGUI ExpectedLapModiferText;
    public TextMeshProUGUI TireLife;
    private int _currentLap = 0;
    private float[] _lastLapSectorTimes;
    private float _lastLapSplit;
    private int _lastSector = 1;
    public TextMeshProUGUI LastLapReadoutText;
    public TextMeshProUGUI ThisLapReadoutText;
    public Slider TireWearPercentageSlider;
    public Image TireWearPercentageSliderFillArea;
    public TextMeshProUGUI TireWearPercentageText;
    public bool MainTelemetryActive = true;

    [Header("Pit Stop Telemetry Parameters")]
    public TextMeshProUGUI SpeedReadoutText;
    public TextMeshProUGUI ExpectedDurabilityText;
    public TextMeshProUGUI PitTimeRemainingText;
    public TextMeshProUGUI ExpectedNextStopText;
    public GameObject PitstopReadoutsGameObject;
    public GameObject RegularTelemetryGameObject;

    // Used to display the track map on the telemetry screen
    [Header("Track Parameters")]
    private Track _currentTrack;
    public Image SectorSprite;
    public TextMeshProUGUI SectorLabel;

    // Used to call pitstops and toggle the pitstop light
    [Header("Pit Stop Panel UI Parameters")]
    public Button PitButton;
    public Sprite PitConfirmSprite;
    public Sprite NoPitSprite;
    public Image PitLight;

    // Used to alter the aggression display
    public Button IncrementAggressionButton;
    public Button DecrementAggressionButton;

    // Used to cycle through tire options on the UI
    [SerializeField][Range(0, 2)]
    private int _tireUISelection = 0;

    private int _currentTireType;


    void Awake()
    {
        // Immediately sets the starting tire on the player's car to the option set in the UI
        // TODO: This will need to be modified once the start of the race is no longer coupled to the scene load
        Player.RaceCar.StartingTireTireType = TireOptions[_tireUISelection].TireType;
        // Sets the current tire type internally on the UI
        // TODO: This system is almost certainly leading to the error causing the tire display to occasionally show the wrong tire
        _currentTireType = _tireUISelection;
        TireImageRenderer.sprite = TireOptions[_currentTireType].TireSprite;
    }

    // Start is called before the first frame update
    void Start()
    {
        _lastLapSectorTimes = new float[3];
        UpdateNextTire();
    }


    void FixedUpdate()
    {
        // Sets the pit light based on the player's PitFlag
        PitLight.sprite = Player.RaceCar.PitFlag ? PitConfirmSprite : NoPitSprite;
        PitstopReadoutsGameObject.SetActive(!MainTelemetryActive);
        RegularTelemetryGameObject.SetActive(MainTelemetryActive);
        TireType displayTireType = MainTelemetryActive ? Player.RaceCar.currentTireType : Player.GetNextTireType();
        SetTireWearSlider();
        UpdatePitDisplay();
        foreach (var tireOption in TireOptions)
        {
            if (tireOption.TireType != displayTireType) continue;
            if (tireOption.TireSprite == TireImageRenderer.sprite) return;
            TireImageRenderer.sprite = tireOption.TireSprite;
            break;
        }
        // Updates the current location for the UI
        if (IsStaticDisplay) return;
        UpdateRacerLocation();
        // TODO: I could fix the wrong tire display bug in here by increasing this part's complexity, but it would be better to change the whole system further up the chain
        
    }

    private void UpdatePitDisplay()
    {
        float timeRemaining = Player.GetPitTimeRemaining();
        int expectedLife = TireUI.GetExpectedTireLife(Player.GetNextTireType());
        string speedString = TireUI.GetSpeedRating(Player.GetNextTireType());
        int nextStopLap = _currentLap + expectedLife;
        if (!Player.IsInPit())
        {
            nextStopLap += 1;
        }

        PitTimeRemainingText.text = Player.IsInPit() ? $"{timeRemaining:f2}" : "25.00";
        ExpectedDurabilityText.text = $"{expectedLife} laps";
        SpeedReadoutText.text = speedString;
        ExpectedNextStopText.text = nextStopLap < _currentTrack.LapCount ? $"Lap {nextStopLap}" : "No More Stops";
    }

    private void UpdateRacerLocation()
    {

        // Caches raw lap progress and the current lap time thus far
        float rawLapProgress = Player.RaceCar.GetRawProgress();
        float activeLapTime = Player.RaceCar.GetActiveLapTime();

        if (rawLapProgress >= _currentLap && _currentLap >= 1)
        {
            LastLapReadoutText.text = $"Last Lap: {Player.RaceCar.GetLastLapTime():f2}";
        }
        _currentLap = Mathf.CeilToInt(rawLapProgress);

        // Sets the split text to the placeholder if the lap count is less than zero
        string splitText = _currentLap <= 1 ? "(**.**)" : $"({_lastLapSplit:f2})";
        // Sets the current lap telemetry readout to the active laptime plus the last known split
        ThisLapReadoutText.text = $"This Lap: {activeLapTime:f2} {splitText}";

        // Sets the sector label and sprite
        // If stopped, the sector information will be incorrect, thus it needs a corner case handler
        if (Player.RaceCar.CheckIfStopped())
        {
            SectorLabel.text = "In Pit";
            SectorSprite.sprite = _currentTrack.NoSectorSprite;
        }
        // Otherwise, the track can convert the raw position into a current sector location
        else
        {
            int currentSector = _currentTrack.GetSectorNumber(rawLapProgress);
            UpdateSplit(currentSector, activeLapTime);
            switch (currentSector)
            {
                case (1):
                    SectorLabel.text = "Sector 1";
                    SectorSprite.sprite = _currentTrack.Sector1Sprite;
                    break;
                case (2):
                    SectorLabel.text = "Sector 2";
                    SectorSprite.sprite = _currentTrack.Sector2Sprite;
                    break;
                case (3):
                    SectorLabel.text = "Sector 3";
                    SectorSprite.sprite = _currentTrack.Sector3Sprite;
                    break;
                default:
                    SectorLabel.text = "";
                    SectorSprite.sprite = _currentTrack.NoSectorSprite;
                    break;
            }
        }
        
    }

    // Updates the splits array which stores the most recent laptimes for each sector. When a sector is completed the lap split is calculated and the new time is stored.
    private void UpdateSplit(int sectorNumber, float activeLapTime)
    {

        if (_currentLap < 1) return;

        if (_lastSector != sectorNumber)
        {
            float lastSplit = _lastLapSectorTimes[_lastSector - 1];
            _lastLapSectorTimes[_lastSector - 1] = activeLapTime;
            _lastSector = sectorNumber;
            if (_currentLap != 1)
            {
                _lastLapSplit = activeLapTime - lastSplit;
            }
        }
    }

    // TODO: This is wildly not null safe
    private void UpdateNextTire()
    {
        Player.SetNextTireType(TireOptions[_tireUISelection].TireType);
        NextTireTypeLabel.text = TireOptions[_tireUISelection].GetName();
        NextTireTypeLabel.color = TireOptions[_tireUISelection].TireColor;
    }

    // Used by the UI to cycle the next tire selection
    public void CycleTireSelection()
    {
        _tireUISelection++;
        if (_tireUISelection >= TireOptions.Length)
        {
            _tireUISelection = 0;
        }
        UpdateNextTire();
    }

    // Used by the UI to toggle the pit light
    public void PitToggle()
    {
        Player.PitToggle();
        // PitLight.sprite = Player.RaceCar.PitFlag ? PitConfirmSprite : NoPitSprite;
    }

    // Used by the UI to increment the player's aggression stat
    public void IncrementAggression()
    {
        bool belowMaximum = Player.IncrementAggression();
        IncrementAggressionButton.interactable = belowMaximum;
        DecrementAggressionButton.interactable = true;
    }

    // Used by the UI to decrement the player's aggression stat
    public void DecrementAggression()
    {
        bool aboveMinimum = Player.DecrementAggression();
        DecrementAggressionButton.interactable = aboveMinimum;
        IncrementAggressionButton.interactable = true;
    }

    public void UpdateTireProjection(float tireModifier)
    {
        ExpectedLapModiferText.text = tireModifier.ToString("f2");
    }

    public void UpdateTireLife(float lapCount)
    {
        TireLife.text = $"{Mathf.FloorToInt(lapCount)}/{GetTireExpectedLife(Player.RaceCar.currentTireType)} laps";
    }

    public void SetTrack(Track currentTrack)
    {
        _currentTrack = currentTrack;
    }

    public TireUI GetStartingTireUI()
    {
        return TireOptions[_tireUISelection];
    }

    public void CycleStartingTireSelection(int direction)
    {
        _tireUISelection += (int) Mathf.Sign(direction);
        if (_tireUISelection >= TireOptions.Length)
        {
            _tireUISelection = 0;
        }
        else if (_tireUISelection < 0)
        {
            _tireUISelection = TireOptions.Length - 1;
        }
        _currentTireType = _tireUISelection;
        UpdateNextTire();
        Player.RaceCar.StartingTireTireType = TireOptions[_tireUISelection].TireType;

    }

    private void SetTireWearSlider()
    {
        float rawTireWearPercentage = Player.RaceCar.GetTireUsagePercentage() * 100;
        float tireWearPercentage = Mathf.Clamp(rawTireWearPercentage, 0 , 100);
        TireWearPercentageSlider.value = 1 - tireWearPercentage/100;
        if (tireWearPercentage < 50)
        {
            Color wearColor = new Color(Mathf.Clamp(((tireWearPercentage / 50f)), 0, 1), 1, 0);
            TireWearPercentageSliderFillArea.color = wearColor;
            TireWearPercentageText.color = wearColor;

        }
        else
        {
            Color wearColor = new Color(1, Mathf.Clamp(1 - ((tireWearPercentage - 50) / 50f), 0, 1), 0);
            TireWearPercentageSliderFillArea.color = wearColor;
            TireWearPercentageText.color = wearColor;
        }

        if (rawTireWearPercentage >= 90)
        {
            Player.TireFailureWarning();
        }

        if (rawTireWearPercentage >= 103)
        {
            Player.TireFailure();
        }

        TireWearPercentageText.text = $"{Mathf.RoundToInt(100 - tireWearPercentage)}%";
    }


    public Sprite GetTireSprite(TireType racerCurrentTireType)
    {
        return (from tireUI in TireOptions where tireUI.TireType == racerCurrentTireType select tireUI.TireSprite).FirstOrDefault();
    }

    public int GetTireExpectedLife(TireType racerCurrentTireType)
    {
        return TireUI.GetExpectedTireLife(racerCurrentTireType);
    }

    public void SwitchTelemetry()
    {
        MainTelemetryActive = !MainTelemetryActive;
    }

}
