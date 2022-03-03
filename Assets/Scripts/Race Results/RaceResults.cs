using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class RaceResults : MonoBehaviour
{

    public List<string> Results;
    public int PlayerPosition = 10;

    // Removes any lingering previous race results to ensure that only the most recent result survives into the race end scene
    void Awake()
    {
        // var raceResults = FindObjectsOfType<RaceResults>();
        // foreach (var raceResult in raceResults)
        // {
        //     if (raceResult != this)
        //     {
        //         Destroy(raceResult.gameObject);
        //     }
        // }
    }

    // Locates a RaceRunner, rips the position order, locks in the player position, then hangs around to be available for the RaceEnd screen to read
    void Start()
    {
        // DontDestroyOnLoad(this);
        Results = new List<string>();
        RaceRunner raceRunner = FindObjectOfType<RaceRunner>();
        if (!raceRunner) return;
        Debug.Log("New Race Runner Created");
        foreach (var standing in raceRunner.RaceStandings)
        {
            Results.Add(standing.Racer.DriverName);
            Debug.Log($"Result standing found for {standing.Racer.DriverName}");
            if (standing.IsPlayer())
            {
                Debug.Log($"Result Placement {standing.GetPositionNumber()} is Player");
                if (standing.Racer.CheckIfDisqualified())
                {
                    PlayerPosition = -1;
                }
                else
                {
                    PlayerPosition = standing.GetPositionNumber();
                }
            }
        }
    }

}
