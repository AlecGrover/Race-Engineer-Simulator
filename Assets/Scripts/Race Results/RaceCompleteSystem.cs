using System.Collections;
using System.Collections.Generic;
using Audio;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerEngineer))]
public class RaceCompleteSystem : MonoBehaviour
{

    public GameObject RaceEndUIGameObject;
    public UISlide CheckeredFlagSlide;
    public AudioClip RaceEndMusic;

    void Awake()
    {
        RaceEndUIGameObject.SetActive(false);
    }

    public void EndRace()
    {

        RaceEndUIGameObject.SetActive(true);
        if (CheckeredFlagSlide)
        {
            CheckeredFlagSlide.gameObject.SetActive(true);
            CheckeredFlagSlide.StartSlide();
        }
        UISlide uiSlide = RaceEndUIGameObject.GetComponent<UISlide>();
        if (uiSlide)
        {
            uiSlide.StartSlide();
        }

        // var oldResults = FindObjectsOfType<RaceResults>();
        // foreach (var result in oldResults) Destroy(result.gameObject);
        var results = Instantiate(new GameObject());
        results.transform.name = "Results";
        results.AddComponent<RaceResults>();
        StartCoroutine(ProcessRaceResults());
    }

    private IEnumerator ProcessRaceResults()
    {
        yield return new WaitForNextFrameUnit();

        RaceEndScreen raceEndScreen = FindObjectOfType<RaceEndScreen>();
        if (raceEndScreen)
        {
            raceEndScreen.UpdateResults();
        }

        if (RaceEndMusic)
        {
            var musicPlayer = FindObjectOfType<MusicController>();
            if (musicPlayer) musicPlayer.QueueSong(RaceEndMusic);
        }
    }

}
