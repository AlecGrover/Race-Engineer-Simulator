using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaceStanding : MonoBehaviour
{
    // This is a weird class, and will likely need a future refactor as it currently does too many things
    // Tracks the RacingCar currently in a given position, its split to the car in front of it, its position number, and the UI elements to display that information
    public RacingCar Racer;
    public float Split = 0f;
    public TextMeshProUGUI PositionLabel;
    private int _positionNumber = 1;
    private string _positionString = "01";
    public TextMeshProUGUI LapLabel;
    private PlayerEngineer _player;
    public GameObject PlayerHighlight;
    private Color _defaultColor;
    public bool RaceComplete = false;
    public Image TireImage;


    // Checks to see if the position starts with a player controlled Racer
    void Start()
    {
        _player = FindObjectOfType<PlayerEngineer>();
        PlayerHighlight.SetActive(Racer == _player.RaceCar);
        _defaultColor = PositionLabel.color;
    }
    
    // Updates the UI
    void Update()
    {
        
    }

    public void SetName()
    {
        PositionLabel.text = $"{_positionString} {Racer.DriverName.Substring(0, 3)}  *0.00";
    }

    public void StandingsBoardTick()
    {
        // Caches a string value for the split's text
        string splitText;

        // Sets the lap counter to the car's current lap or blank if the car has finished
        string lapsCompletedString = $"{Racer.GetLap()}";
        LapLabel.text = RaceComplete ? "**" : lapsCompletedString;

        // If the race is complete, the split time is removed and replaced with " FIN "
        if (RaceComplete)
        {
            splitText = "*FIN*";
        }
        // If the car is pitted, the split time is removed and replaced with "  PIT""
        else if (Racer.CheckIfPitted())
        {
            splitText = "**PIT";
        }
        else if (Racer.CheckIfDisqualified())
        {
            splitText = "**DNF";
        }
        // If the car has managed to fall more than a minute behind, the split timer will change to track minutes:seconds
        // TODO: Remove corner case issue for splits somehow greater than 99:59
        else if (Split >= 60)
        {
            int minutes = (int) (Split / 60);
            int seconds = (int) Math.Round(Split - (minutes * 60));
            // string secondsString = seconds < 10 ? string.Format("0{0}", seconds) : seconds.ToString();
            splitText = $"{minutes}:{seconds:D2}";
        }
        // If the split is simply a normal split, print a float to two decimal places
        else
        {
            splitText = Split.ToString("f2");
        }

        // If the split does not take up the full allotted space, prepend a blank space
        if (splitText.Length < 5)
        {
            splitText = "*" + splitText;
        }

        // Split text is printed as [Position Number] [First Three Characters of Driver Name]  [splitText]
        // TODO: Fix corner case where DriverName is less than 3 characters
        splitText = $"{_positionString} {Racer.DriverName.Substring(0, 3)}  {splitText}";

        PositionLabel.text = splitText;

        TireImage.sprite = _player.pitPanel.GetTireSprite(Racer.currentTireType);


    }

    // Sets the position number
    public void SetPositionNumber(int positionNumber)
    {
        _positionNumber = positionNumber;
        _positionString = $"{positionNumber:D2}";
        //_positionString = positionNumber < 10 ? $"0{positionNumber}" : positionNumber.ToString();
    }

    // Interrupts any currently running FlashColor coroutine and starts the FlashColor coroutine with a green flash
    public void PositionGained()
    {
        StopCoroutine("FlashColor");
        StartCoroutine(FlashColor(Color.green));
    }

    // Interrupts any currently running FlashColor coroutine and starts the FlashColor coroutine with a red flash
    public void PositionLost()
    {
        StopCoroutine("FlashColor");
        StartCoroutine(FlashColor(Color.red));
    }

    // Interrupts any currently running FlashColor coroutine and starts the FlashColor coroutine with a yellow flash
    public void PassFailed()
    {
        StopCoroutine("FlashColor");
        StartCoroutine(FlashColor(Color.yellow));
    }

    private IEnumerator FlashColor(Color color)
    {
        // Waits for the next frame so the driver names also update
        yield return new WaitForEndOfFrame();
        StandingsBoardTick();
        // Positions have switched so the player highlight may have moved, checks if it needs to be applied or removed
        PlayerHighlight.SetActive(Racer == _player.RaceCar);
        // Starts a count of elapsed time
        float elapsedTime = 0;
        // Caches the original color
        Color startingColor = PositionLabel.color;
        // Determines the differences between the target color and the original in all color channels
        float[] differences = new float[3];
        differences[0] = startingColor.r - color.r;
        differences[1] = startingColor.g - color.g;
        differences[2] = startingColor.b - color.b;
        // For the first 0.5 seconds, increment towards the target color each frame proportionately to the frame's duration in each color channel
        while (elapsedTime < 0.5f)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;
            startingColor = new Color(
                Mathf.Clamp(startingColor.r - differences[0] * Time.deltaTime / 0.25f, 0, 1),
                Mathf.Clamp(startingColor.g - differences[1] * Time.deltaTime / 0.25f, 0, 1),
                Mathf.Clamp(startingColor.b - differences[2] * Time.deltaTime / 0.25f, 0, 1)
                );
            // Set the text's color to the current midpoint value
            PositionLabel.color = startingColor;
            // Debug.Log($"New Color {startingColor.r}-{startingColor.g}-{startingColor.b}");
            yield return new WaitForEndOfFrame();
        }

        // Determines the differences between the current color and the default in all color channels
        differences[0] = startingColor.r - _defaultColor.r;
        differences[1] = startingColor.g - _defaultColor.g;
        differences[2] = startingColor.b - _defaultColor.b;
        // For the second 0.5 seconds, increment towards the default color each frame proportionately to the frame's duration in each color channel
        // TODO: This could likely be refactored into an extracted method, questionable necessity, good practice
        while (elapsedTime < 1f)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;
            startingColor = new Color(
                Mathf.Clamp(startingColor.r - differences[0] * Time.deltaTime / 0.25f, 0, 1),
                Mathf.Clamp(startingColor.g - differences[1] * Time.deltaTime / 0.25f, 0, 1),
                Mathf.Clamp(startingColor.b - differences[2] * Time.deltaTime / 0.25f, 0, 1)
            );
            // Set the text's color to the current midpoint value
            PositionLabel.color = startingColor;
            yield return new WaitForEndOfFrame();
        }
        // Hard resets the color to the default to avoid a long term rounding error danger
        PositionLabel.color = _defaultColor;
    }

    // Returns true if the position contains the player's car
    public bool IsPlayer()
    {
        bool isPlayer= Racer == _player.RaceCar;
        return isPlayer;
    }

    // Returns the position number
    public int GetPositionNumber()
    {
        return _positionNumber;
    }
}
