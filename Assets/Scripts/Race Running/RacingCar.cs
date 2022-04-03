using System;
using System.Collections;
using UnityEngine;

public enum TireType
{
    Soft,
    Medium,
    Hard,
    Debug
}


public class RacingCar : MonoBehaviour
{

    // Audio Parameters
    public RandomOneShotAudio OneShotAudio;
    [SerializeField]
    private RandomOneShotAudio _dnfWarningOneShotAudio;

    // Pit Stop Parameters
    private bool _isPitted= false;
    [SerializeField]
    public bool PitFlag= false;

    // Track Information
    private Track _currentTrack;
    private float _projectedIdealLapTime = 30f;

    // Static parameters that determine the effect of various changeable values on the overall pace
    private static float _aggressionModifier= 1.5f;
    private static float _aggressionZeroValue = 25;
    private static float _driverSkillModifier= 1.25f;
    private static float _basePaceModifier= 1.75f;
    private static float _pitStopDuration= 25f;

    // Tire Parameters
    private Tires currentTires;
    public TireType currentTireType { get; private set; }
    public TireType StartingTireTireType = TireType.Hard;
    public TireType nextTireType = TireType.Hard;
    public float ExpectedTireModifer = 0;
    [SerializeField]
    private float _lapsInCurrentStint = 1;
    private float _aggressionWearModifier = 0;

    // Driver Strategy Parameters
    [Header("Driver Statistics")]
    public string DriverName;
    [Tooltip("Aggression sets how intensely a driver is driving, higher intensity decreases lap times and makes overtakes easier, lower intensity increases tyre life.")]
    [SerializeField] [Range(40, 100)]
    public int Aggression= 50;
    [Tooltip("Driver will attempt an overtake only when the gap to the next driver is less than the set strategy value. Larger gaps have a higher failure rate but may save time.")]
    [SerializeField] [Range(0.1f, 1.0f)]
    public float AttemptOvertakeAtGap= 0.5f;

    // Car Capability Parameters
    [Header("Car Modifiers")]
    [Tooltip("A measure of the car's performance, the closer to 100% (1.0f), the closer the car gets to the ideal lap time.")]
    [SerializeField] [Range(0.01f, 1.0f)]
    private float _basePacePercent= 0.75f;
    [SerializeField]
    [Range(0f, 1.0f)]
    private float _driverSkill= 1f;

    // Position Parameters
    private float _rawLapProgress = 0f;
    private int _lapsSinceLastStop = 0;
    private float _tireWearLapsSinceLastStop = 0f;
    private float _tireWearThisLap = 0f;
    private bool _disqualified = false;
    private int _disqualificationWarningIssuedOnLap = int.MaxValue;

    // LapTime and Pace Parameters
    public float LapTimeModifier= 0f;
    private float _lapTimeStart;
    private float _lapTime = 0f;
    private float _lapExtraTime;
    private float _multiplierOverride= 100f;

    public float CurrentLapTime;
    private float _lastLapTime;

    private float _stopStartTime = 0;
    private float _stopDuration = 0f;

    void Awake()
    {

    }



    // Sets the start time for the first lap and applies the selected starting tires
    public void StartRace()
    {
        // Sets the current tires to a new set of the designated starting tire
        currentTires = StartingTireTireType switch
        {
            TireType.Hard => new HardTires(),
            TireType.Medium => new MediumTires(),
            TireType.Soft => new SoftTires(),
            TireType.Debug => new DebugTires(),
            _ => currentTires
        };
        currentTireType = StartingTireTireType;
        _lapTimeStart = Time.time;
        _lapTime = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentTires = new HardTires();
    }

    // Processes car updates based on the duration of the previous frame
    public void Tick()
    {
        // If the car is pitted, no updates should take place
        if (CheckIfStopped())
        {
            if (_isPitted) _stopDuration += Time.deltaTime;
            return;
        }
        // Calculates the progress made during this tick based on the duration of the previous frame
        float newProgress = GetLapTimeModifier() * Time.deltaTime / _currentTrack.GetLapTime();
        // Increments the tire wear based on the current wear multiplier and the percentage of a lap completed
        _tireWearThisLap += newProgress * GetTireWearMultiplier();
        // If the car completes a lap with this new progress, the LapComplete method is called
        if (Mathf.Floor(_rawLapProgress) - Mathf.Floor(_rawLapProgress + newProgress) < 0)
        {
            LapComplete();
        }
        // The raw lap progress, which tracks lap completion fractionally, is incremented
        _rawLapProgress += newProgress;
        _lapTime += Time.deltaTime;
    }
    
    // Returns the theoretical lap time modifer if no modifier restrictions were applied
    public float GetRawLapTimeModifier()
    {
        // Modifier is calculated by determining small percentage modifiers from various tweakable attributes (both +ve and -ve), then adding them to the base modifier of 1.0f
        float lapTimeModifier= 1;
        float aggressionMultiplier= (Aggression - _aggressionZeroValue) / (100 - 25f) * (_aggressionModifier / 30f);
        float driverSkillMultiplier= _driverSkillModifier * (_aggressionModifier / 30f);
        float carMultiplier= _basePacePercent * (_basePaceModifier / 30f);
        float tireMultiplier= -currentTires.GetPaceModifier(_tireWearLapsSinceLastStop) / 30f;
        float totalMultiplier= aggressionMultiplier + driverSkillMultiplier + carMultiplier + tireMultiplier;
        lapTimeModifier += totalMultiplier;
        return lapTimeModifier;
    }

    // Returns the effective lap time modifier, determined as the lower of any applied restrictions and the theoretical modifier
    public float GetLapTimeModifier()
    {
        return Mathf.Min(GetRawLapTimeModifier(), _multiplierOverride);
    }

    // Returns a wear multiplier based on aggression, high aggression will cause tires to wear more rapidly, while very low aggression will extend tire life
    public float GetTireWearMultiplier()
    {
        float tireWearMultiplier= (Aggression - 60f) / 200f + 1f;
        return tireWearMultiplier;
    }

    // Used to get default target lap times
    public void SetTrack(Track currentTrack)
    {
        _currentTrack = currentTrack;
        _projectedIdealLapTime = currentTrack.GetLapTime();
    }

    // Triggers pit stops if necessary, increments tire wear, increments lap counts, sets lap time information, plays an audio clip for a car passing the pit wall
    private void LapComplete()
    {
        // _lastLapTime = Time.time - _lapTimeStart;
        _lastLapTime = _lapTime;
        // Debug.Log($"{DriverName} lap {Mathf.FloorToInt(GetRawProgress()):f2} complete, current modifier is {GetLapTimeModifier():f2}");
        if (!PitFlag)
        {
            _tireWearLapsSinceLastStop += _tireWearThisLap;
            _tireWearThisLap = 0;
            _lapsInCurrentStint++;
            _lapsSinceLastStop++;
            _lapTimeStart = Time.time;
            _lapTime = 0f;
            if (GetRawLapTimeModifier() <= 0.5f)
            {
                if (_disqualificationWarningIssuedOnLap > Mathf.RoundToInt(_rawLapProgress))
                {
                    IssueDisqualificationWarning();
                }
                else if (_disqualificationWarningIssuedOnLap < Mathf.RoundToInt(_rawLapProgress))
                {
                    Disqualify();
                }
            }
        }
        else
        {
            TriggerPitStop();
        }

        if (OneShotAudio)
        {
            var audioSource = FindObjectOfType<AudioSource>();
            var audioClip = OneShotAudio.GetRanAudioClipFromList();
            if (audioSource && audioClip)
            {
                audioSource.PlayOneShot(audioClip, 0.15f);
            }
        }
    }

    // TODO: Make this not fire if the car is not a player, best bet would be to move this into PlayerEngineer  
    private void IssueDisqualificationWarning()
    {
        _disqualificationWarningIssuedOnLap = Mathf.RoundToInt(_rawLapProgress);
        var tempObject = new GameObject();
        tempObject.transform.name = "DNF Warning Audio Source";
        Debug.Log("Created AudioSource");
        Destroy(tempObject, 10f);
        var audioSource = tempObject.AddComponent<AudioSource>();
        var audioClip = _dnfWarningOneShotAudio.GetRanAudioClipFromList();
        if (audioSource && audioClip)
        {
            audioSource.PlayOneShot(audioClip, 1f);
            Debug.Log("Playing DNF warning");
        }
    }

    // Sets the multiplier to an override value that will be used in place of the calculated one if it is worse
    public void RestrictModifier(float multiplierOverride)
    {
        _multiplierOverride = multiplierOverride;
    }

    // Removes the modifier restriction
    public void UnlockModifier()
    {
        _multiplierOverride = 100;
    }

    // Returns the last lap time
    public float GetLastLapTime()
    {
        return _lastLapTime;
    }

    // Returns the time since the start of the lap
    public float GetActiveLapTime()
    {
        float currentLapTime;
        if (_isPitted) {
            currentLapTime= 0;
        }
        else
        {
            // currentLapTime= Time.time - _lapTimeStart;
            currentLapTime = _lapTime;
        }

        return currentLapTime;
    }

    // Jumps the distance that would be covered in timeSkip seconds based on *current* laptime modifier, if this is overriden you must first call UnlockModifier() to jump based on ideal pace
    public void JumpDistance(float timeSkip)
    {
        _rawLapProgress += (timeSkip / _currentTrack.GetLapTime()) * GetLapTimeModifier();
    }

    // Returns the fractional raw value for lap progress, a car halfway through the 14th lap will return 14.5
    public float GetRawProgress()
    {
        return _rawLapProgress;
    }

    // Manually override the raw progress, used for passes to switch the positions of two cars
    public void SetRawProgress(float newRawProgress)
    {
        _rawLapProgress = newRawProgress;
    }

    // Returns the number of completed laps since the last pit stop, used for tire telemetry
    public int GetLapsSinceLastStop()
    {
        return _lapsSinceLastStop;
    }

    // Provides the lap time modifier for the next lap *if* the driver were to have maintained a 1.0 wear modifier during the lap
    // TODO: Find a way to update this value after sector completions
    public float GetProjectedTireModifier()
    {
        float nextLapModifier = currentTires.GetPaceModifier(_tireWearLapsSinceLastStop + 1);
        return nextLapModifier;
    }

    // Toggles the pit flag
    public void TogglePitFlag()
    {
        PitFlag = !PitFlag;
    }

    // If the pit flag is toggled and the car is not already pitted, start the pitstop coroutine
    private void TriggerPitStop(float pitStopModifier = 0)
    {
        if (_isPitted) return;
        StartCoroutine(PitStop(_pitStopDuration + pitStopModifier));
    }

    // Timed coroutine that freezes the car, performs the required scripts for a pitstop, then releases the car
    private IEnumerator PitStop(float stopDuration)
    {
        PitFlag = false;
        _disqualificationWarningIssuedOnLap = int.MaxValue;
        // Sets a flag to alert the race runner that this car is off track and stopped
        _isPitted = true;
        // Being pitted prevents the race runner from adjusting the pace based on proximity, and the car needs to be released at full pace, so any modifiers are cleared
        UnlockModifier();
        _stopStartTime = Time.time;
        _stopDuration = 0f;
        while (_stopDuration < _pitStopDuration)
        {
            yield return new WaitForEndOfFrame();
        }
        // Switches old tires for new ones based on the set next tire type
        currentTires = nextTireType switch
        {
            TireType.Hard => new HardTires(),
            TireType.Medium => new MediumTires(),
            TireType.Soft => new SoftTires(),
            _ => currentTires
        };
        currentTireType = nextTireType;
        // Resets the pitted flag
        _isPitted = false;
        // Resets all values determining the time since the last pitstop to properly apply fresh wear values to the tires and provide correct telemetry information
        _lapsInCurrentStint = 0;
        _lapsSinceLastStop = 0;
        _tireWearLapsSinceLastStop = 0;
        _lapTimeStart = Time.time;
        _lapTime = 0f;
        Debug.Log($"{DriverName} has completed a pitstop, now using {currentTireType.ToString()}");
    }

    // Returns the lap count as a minimum two character string for printing on the split tracker
    public int GetLap()
    {
        return Mathf.Max(Mathf.FloorToInt(_rawLapProgress) + 1, 1);
    }

    // Returns true if the car is stopped
    public bool CheckIfStopped()
    {
        return _isPitted || _disqualified;
    }

    public float GetTireWearLaps()
    {
        return _tireWearLapsSinceLastStop;
    }

    public int GetExpectedTireLife()
    {
        return currentTires.ExpectedLifetime;
    }

    public float GetPitTime()
    {
        return _pitStopDuration;
    }

    public bool CheckIfDisqualified()
    {
        return _disqualified;
    }

    public void Disqualify()
    {
        _disqualified = true;
    }

    public bool CheckIfPitted()
    {
        return _isPitted;
    }

    public float GetTireUsagePercentage()
    {
        float usagePercentage = (_tireWearLapsSinceLastStop + _tireWearThisLap) / (currentTires.ExpectedLifetime * 1.2f);
        return usagePercentage;
    }

    public float GetStopStartTime()
    {
        return _stopStartTime;
    }

    public float GetStopTimerValue()
    {
        return _stopDuration;
    }

    public float GetExpectedPitDuration()
    {
        return _pitStopDuration;
    }
}
