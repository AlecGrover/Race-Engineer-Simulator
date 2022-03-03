using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEngineer : MonoBehaviour
{
    // The Player's interaction interface with their racer
    public RacingCar RaceCar;
    public PitPanel pitPanel;
    private Track _currentTrack;
    public TextMeshProUGUI AggressionReadoutText;
    public WarningSystem WarningUISystem;
    private bool _warnedOfTireFailure = false;

    // Start is called before the first frame update
    void Start()
    {
        // Locates the race runner
        RaceRunner raceRunner = FindObjectOfType<RaceRunner>();
        if (!raceRunner) return;
        // Reads the current track for use in telemetry and feeds it to the pit panel to link the engineer with the UI
        _currentTrack = raceRunner.RaceTrack;
        pitPanel.SetTrack(_currentTrack);
        // Sets the readout text to the racer's aggression
        // TODO: Fix the -01 Aggression underflow error, only reason this hasn't been done is that I don't want to make a 100 Aggression overflow error. Bug is purely visual.
        AggressionReadoutText.text = (RaceCar.Aggression).ToString("D2");
        SetAggressionColor();
    }

    // Updates the UI elements for the tire life
    public void PlayerTick()
    {
        pitPanel.UpdateTireLife(RaceCar.GetLapsSinceLastStop());
        pitPanel.UpdateTireProjection(RaceCar.GetProjectedTireModifier());
        if (RaceCar.CheckIfPitted())
        {
            _warnedOfTireFailure = false;
        }
    }

    // Sets the next tire type for the RacingCar
    public void SetNextTireType(TireType tireType)
    {
        RaceCar.nextTireType = tireType;
    }

    // Toggles the racing car's PitFlag
    public void PitToggle()
    {
        RaceCar.TogglePitFlag();
    }

    // Increases Aggression by 5 to a max of 100
    public bool IncrementAggression()
    {
        RaceCar.Aggression = Mathf.Clamp(RaceCar.Aggression + 5, 40, 100);
        AggressionReadoutText.text = (Mathf.Clamp(RaceCar.Aggression, 40, 99)).ToString("D2");
        SetAggressionColor();
        return RaceCar.Aggression < 100;
    }

    // Decrements Aggression by 5 to a min of 100
    public bool DecrementAggression()
    {
        RaceCar.Aggression = Mathf.Clamp(RaceCar.Aggression - 5, 40, 100);
        AggressionReadoutText.text = (Mathf.Clamp(RaceCar.Aggression, 40, 99)).ToString("D2");
        SetAggressionColor();
        return RaceCar.Aggression > 40;
    }

    public void SetAggressionColor()
    {
        float aggression = RaceCar.Aggression;
        if (aggression < 60)
        {
            AggressionReadoutText.color = new Color((aggression - 40) / 30f + 0.25f, 1, (aggression - 40) / 30f + 0.25f);
        }
        else if (aggression < 80)
        {
            AggressionReadoutText.color = new Color(1, 1, 0.75f - (aggression - 60) / 30f + 0.25f);
        }
        else
        {
            AggressionReadoutText.color = new Color(1, 0.75f - (aggression - 80) / 30f + 0.25f, 0.25f);
        }
    }

    public void TireFailureWarning()
    {
        if (_warnedOfTireFailure || RaceCar.CheckIfPitted()) return;
        _warnedOfTireFailure = true;
        WarningUISystem.SendWarning("Driver", "I think my tires are about to fail.");
    }

    public void TireFailure()
    {
        if (RaceCar.CheckIfDisqualified()) return;
        WarningUISystem.SendWarning("Driver", "Tires failed, we're out of this one.");
        RaceCar.Disqualify();
    }

    public float GetPitTimeRemaining()
    {
        float pitTimeRemaining = RaceCar.GetPitTime() - (Time.time - RaceCar.GetStopStartTime());
        return Mathf.Clamp(pitTimeRemaining, 0, RaceCar.GetPitTime());
    }

    public TireType GetNextTireType()
    {
        return RaceCar.nextTireType;
    }

    public bool IsInPit()
    {
        return RaceCar.CheckIfPitted();
    }
}
