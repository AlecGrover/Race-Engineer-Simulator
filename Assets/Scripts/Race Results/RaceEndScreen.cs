using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RaceEndScreen : MonoBehaviour
{
    // Tracks the UI objects to change
    public TextMeshProUGUI PlacementText;
    public Image TrophyImage;

    public RandomOneShotAudio PodiumAudio;
    public RandomOneShotAudio OffPodiumAudio;

    // Sets an arbitrary number of position trophy sprites, the functionality changes based on the size of this list
    [Tooltip("Contains the sprites for trophies in order of the intended places that will receive them")]
    public Sprite[] PositionSprites = new Sprite[0];


    // Start is called before the first frame update
    void Start()
    {
        // Locates a RaceResults object created by a RaceRunner after the last car has finished the race and pulls the information it contains
        // UpdateResults();
    }

    public void UpdateResults()
    {
        Debug.Log("Updating Race Results");
        var raceResults = FindObjectOfType<RaceResults>();
        if (raceResults)
        {
            Debug.Log("Found race results object... changing UI");
            // Sets the placement text to the player's position with the correct suffix
            PlacementText.text = raceResults.PlayerPosition switch
            {
                (-1) => "DNF",
                (1) => "1st",
                (2) => "2nd",
                (3) => "3rd",
                _ => $"{raceResults.PlayerPosition}th"
            };
            AudioSource audioSource = this.GameObject().AddComponent<AudioSource>();
            audioSource.volume = 0.15f;
            if (raceResults.PlayerPosition <= 3 && raceResults.PlayerPosition >= 0)
            {
                if (PodiumAudio)
                {
                    audioSource.PlayOneShot(PodiumAudio.GetRanAudioClipFromList());
                }
            }
            else
            {
                if (OffPodiumAudio)
                {
                    audioSource.PlayOneShot(OffPodiumAudio.GetRanAudioClipFromList());
                }
            }
            // If a trophy sprite exists for the player's position, set the trophy image sprite to it, otherwise make the icon clear
            if (raceResults.PlayerPosition <= PositionSprites.Length && raceResults.PlayerPosition > 0)
            {
                TrophyImage.sprite = PositionSprites[raceResults.PlayerPosition - 1];
            }
            else
            {
                TrophyImage.color = Color.clear;
            }
        }
    }

}
