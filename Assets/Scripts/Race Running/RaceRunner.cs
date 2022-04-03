using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RaceRunner : MonoBehaviour
{

    // Overtaking Parameters
    [Header("Allowable Overtaking Ranges")]
    [Tooltip("The smallest gap that a car can have between it and the car in front, don't touch this without also changing information in RacingCar.cs")]
    [SerializeField] [Range(0.1f, 0.5f)]
    private float _lowestPossibleGap= 0.2f;
    [Tooltip("Defines the maximum overtaking range relative to the lowest possible gap, don't touch this without also changing information in RacingCar.cs and the AttemptOvertake method in RaceRunner.cs")]
    [SerializeField] [Range(0.1f, 1f)]
    private float _overtakingGapRange= 0.8f;
    private float _overtakeLeap= 1.5f;

    public RaceStanding[] RaceStandings = new RaceStanding[10];
    private AI_Engineer[] _ai;
    private PlayerEngineer _player;

    public Track RaceTrack;

    private float _raceStartTime;

    private bool _raceComplete = false;
    public bool WaitForAllRacers = false;


    void Awake()
    {
        // Iterate through each standing to pass on the track
        foreach (var standing in RaceStandings)
        {
            standing.Racer.SetTrack(RaceTrack);
        }

        _ai = FindObjectsOfType<AI_Engineer>();
        _player = FindObjectOfType<PlayerEngineer>();
    }

    void Start()
    {
        // Iterate through the race standings to further initialize parameters
        // Split from the awake method to ensure tasks are executed in a safe order
        for (var i = 0; i < RaceStandings.Length; i++)
        {
            // Set standing position
            RaceStandings[i].SetPositionNumber(i + 1);
            RaceStandings[i].SetName();
        }
        // Debugging Print to Console Systems
        // PrintStandingsToConsole();
        // StartCoroutine(PrintStandings());

        // Looks for a MusicController and queues the Track's provided soundtrack
        MusicController musicController = FindObjectOfType<MusicController>();
        if (musicController)
        {
            musicController.QueueSong(RaceTrack.Soundtrack);
        }
    }

    void Update()
    {
        // Calls the RaceTick method in which all updates to the game state are processed
        // if (!_raceComplete) RaceTick();
    }

    // Debugging method used to print raw racer information to the console
    private void PrintProgressToConsole()
    {
        foreach (var standing in RaceStandings) 
        {
            Debug.Log(String.Format("{0}\t{1:f2}\t{2:f2}\t{3:f2}", standing.Racer.DriverName.Substring(0, 3), standing.Racer.GetRawProgress(), standing.Split, standing.Racer.GetLapTimeModifier()));
        }
    }

    private IEnumerator RunRace()
    {

        foreach (var standing in RaceStandings)
        {
            standing.Racer.StartRace();
        }

        // Split from the awake method to ensure tasks are executed in a safe order
        for (var i = 0; i < RaceStandings.Length; i++)
        {
            // Staggers starting grid apart by 1 second (first car remains at start/finish line)
            if (i == 0) continue;
            RaceStandings[i].Split = 1f;
            RaceStandings[i].Racer.JumpDistance(-1f * i);
        }

        while (!_raceComplete)
        {
            RaceTick();
            foreach (var standing in RaceStandings)
            {
                standing.StandingsBoardTick();
            }
            foreach (var engineer in _ai)
            {
                engineer.AITick();
            }
            _player.PlayerTick();
            yield return new WaitForEndOfFrame();
        }
    }

    // The main update sequence, checks the state of each racer relative to the state of the racer in front of them and updates based on that data
    private void RaceTick()
    {

        // The leader does not have a car in front of it and is thus specially handled to set the pace for the rest of the field
        RacingCar leader= RaceStandings[0].Racer;
        leader.UnlockModifier();
        // Calls the leading car's tick function iff the leader is on track and has not finished the race
        if (!RaceStandings[0].RaceComplete)
        {
            leader.Tick();
            // Checks if the leader has crossed the finish line of the last lap, if so, marks the position as having completed the race
            if (leader.GetRawProgress() >= RaceTrack.LapCount)
            {
                RaceStandings[0].RaceComplete = true;
            }
        }
        else if (leader.CheckIfDisqualified())
        {
            if (RaceStandings.All(standing => standing.Racer.CheckIfDisqualified()))
            {
                _raceComplete = true;
            }
        }

        // Iterates through positions 2 and up with a two position wide window (i & i-1) updating track positions, splits, and calling the race tick functions of each
        for (int i = 1; i < RaceStandings.Length; i++)
        {
            // Caches references to position i and i-1 as well as their currently tracked cars
            RaceStanding thisPosition = RaceStandings[i];
            RaceStanding nextPosition = RaceStandings[i - 1];
            RacingCar thisCar = thisPosition.Racer;
            RacingCar nextCar = nextPosition.Racer;

            // If a car is stopped or the race is complete, no splits or positions should change, so simply continue. If the car is pitted it still will need its tick function called.
            if (thisCar.CheckIfStopped() || thisPosition.RaceComplete)
            {
                if (thisCar.CheckIfDisqualified())
                {
                    if (RaceStandings.ToList().GetRange(i, RaceStandings.Length - i)
                        .All(standing => standing.Racer.CheckIfDisqualified()))
                    {
                        thisPosition.RaceComplete = true;
                    }
                }
                else if (thisCar.CheckIfPitted())
                {
                    thisCar.Tick();
                }

                continue;
            }

            // Get new split distance
            if (nextPosition.RaceComplete)
            {
                // If the car in front has completed the race, the current car has its modifier unlocked.
                    // If the cars are extremely close this could cause some unexpected functionality, but it's better than the alternative solution
                thisCar.UnlockModifier();
                // With its modifier unlocked, the car has its tick update
                thisCar.Tick();
                // The gap is now based on the end of the race, not on the car in front (although they should be about equal)
                float gap = RaceTrack.GetLapCount() - thisCar.GetRawProgress();
                // If the gap has closed, the car has completed the race, otherwise the split counter can be updated
                if (gap <= 0)
                {
                    thisPosition.RaceComplete = true;
                }
                else
                {
                    float split = gap * RaceTrack.GetLapTime() / thisCar.GetLapTimeModifier();
                    thisPosition.Split = split;
                }
            }
            // If the car in front is stopped in the pitlane then proximity slowdowns and passing challenges are ignored, but progress still continues as otherwise expected
            else if (nextCar.CheckIfStopped())
            {
                // If the car in front has pitted, any lingering proximity slowdowns should be cancelled
                    // This may cause quirks when cars are packed into groups of 3 or more inside a 1 second split and a middle car pits, but it should be minor enough to ignore for now
                thisCar.UnlockModifier();
                thisCar.Tick();
                // As passing challenges are not in use against a stopped car, the cars simply swap when one's track location exceeds the other
                if (thisCar.GetRawProgress() > nextCar.GetRawProgress())
                {
                    thisPosition.Racer = nextCar;
                    nextPosition.Racer = thisCar;
                    thisPosition.PositionLost();
                    nextPosition.PositionGained();
                }
                // If they have not swapped, their split gets adjusted as normal
                else
                {
                    // Calculate fractional lap count between the car and the car in front of it
                    float gap = nextCar.GetRawProgress() - thisCar.GetRawProgress();
                    // Determines the split by the gap, the track's predicted lap time, and the car's multiplier (seconds of progress per second of realtime, i.e. a 1.2 multiplier means the car will travel 1.2 seconds out of the expected lap time every real second)
                    float split = gap * RaceTrack.GetLapTime() / thisCar.GetLapTimeModifier();
                    // Updates the split
                    thisPosition.Split = split;
                }
            }
            // If both the current car and the car in front are on track racing, many other factors come into play
            else
            {
                // Updates the current car's state and determines the gap and split
                thisCar.Tick();
                thisCar.UnlockModifier();
                float gap = nextCar.GetRawProgress() - thisCar.GetRawProgress();
                float split = gap * RaceTrack.GetLapTime() / thisCar.GetLapTimeModifier();

                // Process overtake if needed (split is under the car's internal overtaking range value and the car behind is faster)
                if (split <= thisCar.AttemptOvertakeAtGap &&
                    thisCar.GetRawLapTimeModifier() > nextCar.GetLapTimeModifier())
                {
                    bool overtakeSuccess = NewAttemptOvertake(thisCar, nextCar, split);
                    // Swaps the raw positions of the two cars using a classic little trick, this may need to be adjusted to just use a temp variable if it's found to cause an error
                    if (overtakeSuccess)
                    {
                        // If the overtake succeeds, the current car no longer needs to have its pace proximity restricted by the car it just passed
                        thisCar.UnlockModifier();

                        thisCar.SetRawProgress(thisCar.GetRawProgress() + nextCar.GetRawProgress());
                        nextCar.SetRawProgress(thisCar.GetRawProgress() - nextCar.GetRawProgress());
                        thisCar.SetRawProgress(thisCar.GetRawProgress() - nextCar.GetRawProgress());

                        thisPosition.Racer = nextCar;
                        nextPosition.Racer = thisCar;

                        // Calls the RaceStanding methods for the positions on the split board to flash
                        thisPosition.PositionLost();
                        nextPosition.PositionGained();
                    }
                    // If the current car fails a pass attempt it suffers a distance loss and a slowdown penalty
                    else
                    {
                        // Calls the RaceStanding method for the position on the split board to flash
                        thisPosition.PassFailed();
                        // Applies a slowdown penalty
                        thisCar.RestrictModifier(thisCar.GetLapTimeModifier() - 0.0125f);
                        // Determines the time loss as up to 0.5 seconds, less if the car behind the current car is under 0.7 seconds behind
                        float timeLoss = -(i < RaceStandings.Length - 1
                            ? Mathf.Min(0.5f, RaceStandings[i + 1].Split - 0.2f)
                            : 0.5f);
                        thisCar.JumpDistance(timeLoss);
                    }

                    // As the current car closes to within 1 second of the car in front of it, the car loses speed until it matches the front car's speed at a gap of 0.2 seconds
                    if (thisPosition.Split < 1f)
                    {
                        thisCar.RestrictModifier(thisCar.GetRawLapTimeModifier() + (nextCar.GetLapTimeModifier() - thisCar.GetRawLapTimeModifier() * (1 - ((thisPosition.Split - 0.2f) / 0.8f))));
                    }
                    // If the car is further than 1.2 seconds behind, its modifier is unlocked, the 0.2 second overlap ensures the slowdown applied by a failed pass is punishing
                    else if (thisPosition.Split > 1.2f)
                    {
                        thisCar.UnlockModifier();
                    }

                }

                // Applies split changes
                thisPosition.Split = split;
            }

        }

        // If every position has set its RaceComplete flag, the RaceComplete coroutine is invoked
        if (WaitForAllRacers)
        {
            _raceComplete = RaceStandings.All(standing => standing.RaceComplete);
        }
        else
        {
            _raceComplete = RaceStandings.Any(standing => standing.RaceComplete && standing.IsPlayer());
        }
        if (_raceComplete)
        {
            StartCoroutine(CompleteRace());
        }
    }

    // Waits 0.25 seconds for the scoreboard to finish updating, then loads the race complete screen
    private IEnumerator CompleteRace()
    {
        yield return new WaitForSeconds(0.25f);
        _raceComplete = true;
        RaceCompleteSystem raceCompleteSystem = FindObjectOfType<RaceCompleteSystem>();
        if (raceCompleteSystem)
        {
            raceCompleteSystem.EndRace();
        }

        // var results = Instantiate(new GameObject());
        // results.AddComponent<RaceResults>();
        // yield return new WaitForSeconds(0.25f);
        // RaceEndScreen raceEndScreen = FindObjectOfType<RaceEndScreen>();
        // if (raceEndScreen)
        // {
        //     raceEndScreen.UpdateResults();
        // }


        // var sc = FindObjectOfType<SceneController>();
        // if (sc != null)
        // {
        //     Debug.Log("Loading Race End Scene");
        //     sc.LoadScene(sc.RaceEndSceneName);
        // }
        // else
        // {
        //     Debug.Log("Race End Scene Not Found");
        //     SceneManager.LoadScene(0);
        // }
    }

    // Creates an "Overtaking DC" based on splits, pace differences, and driver aggression, then rolls on a scale of 1-10 to determine overtake success
        // The equation here is relatively arbitrary, having been constructed to "seem right" in Excel, see the reference document "Race Runner Algorithmic Demo.xlsx" for details
    private bool NewAttemptOvertake(RacingCar overtaker, RacingCar defender, float split)
    {

        var overtakeDC = (split * 15) + (defender.GetLapTimeModifier() - overtaker.GetRawLapTimeModifier()) * 4 + (defender.Aggression - overtaker.Aggression)/30f;
        var overtakeSuccess = Random.Range(1, 10) >= overtakeDC;

        return overtakeSuccess;

    }


    // The debug print system for writing raw race information to the console
    private IEnumerator PrintStandings()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            PrintStandingsToConsole();
        }
    }

    private void PrintStandingsToConsole()
    {
        // Debug.Log("Pos\tRacer\t\tSplit\tCurrentPace");
        // for (var i= 0; i < RaceStandings.Length; i++)
        // {
        //     RacingCar racer = RaceStandings[i].Racer;
        //     Debug.Log(i + "\t" + racer.DriverName.Substring(0, 5) + "\t\t" + RaceStandings[i].Split.ToString("f2") + "\t" + racer.GetCurrentPace().ToString("f2"));
        // }
        foreach (var standing in RaceStandings)
        {
            Debug.Log(String.Format("{0}\t{1:f2}\t{2:f2}\t{3:f2}", standing.Racer.DriverName.Substring(0, 3), standing.Racer.GetRawProgress(), standing.Split, standing.Racer.GetLapTimeModifier()));
        }
    }

    public void StartRace()
    {
        StartCoroutine(RunRace());
    }

}
