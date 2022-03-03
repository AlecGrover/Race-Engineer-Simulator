using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AI_Engineer : MonoBehaviour
{

    private class Node
    {
        private float _lapsRemaining;
        private Tires _tires;
        private Node _parentNode = null;

        public Node(float lapsRemaining)
        {
            _lapsRemaining = lapsRemaining;
        }

        public Node(Tires tires, Node parent)
        {
            _parentNode = parent;
            _tires = tires;
            _lapsRemaining = _parentNode.GetLapsRemaining() - _tires.ExpectedLifetime;
        }

        public float GetLapsRemaining()
        {
            return _lapsRemaining;
        }

        public Tires GetTires()
        {
            return _tires;
        }

        public Node GetParent()
        {
            return _parentNode;
        }

        public int GetStops()
        {
            int stops = 0;
            Node node = this;
            while (node.GetParent() != null)
            {
                stops++;
                node = node.GetParent();
            }

            return stops;
        }

        public Tires GetFirstTires()
        {
            Node node = this;
            Tires tires = null;
            while (node.GetParent() != null)
            {
                tires = node.GetTires();
                node = node.GetParent();
            }

            if (tires == null)
            {
                tires = new MediumTires();
                Debug.LogError("ERROR: Attempted to get tires on a node with none registered.");
            }
            return tires;
        }

        public string GetStrategyString()
        {
            if (_tires == null) return "ERROR: Strategy call on empty node";
            string strategy = $"{_tires.GetTireType().ToString()}";
            var node = GetParent();
            while (node?.GetTires() != null)
            {
                strategy = $"{node.GetTires().GetTireType().ToString()} -> {strategy}";
                node = node.GetParent();
            }

            return strategy;
        }

    }


    // TODO: Make the AI not dumb as rocks.
    // In theory, this class will control the AI decisions mid race, right now they just put on new hard tires when their tire modifiers get really bad

    public RacingCar RaceCar;
    public bool HardAI = false;

    private RaceRunner _raceRunner;
    private int _lap = 0;

    // Start is called before the first frame update
    void Start()
    {
        _raceRunner = FindObjectOfType<RaceRunner>();
    }

    // Update is called once per frame
    public void AITick()
    {
        if (RaceCar.GetRawProgress() - 0.15f > _lap)
        {
            _lap++;
            if (RaceCar.GetTireWearLaps() + 1 >= RaceCar.GetExpectedTireLife() && !RaceCar.CheckIfStopped())
            {
                DetermineStrategy();
            }
        }
    }

    private void DetermineStrategy()
    {
        float currentTireWearLaps = RaceCar.GetTireWearLaps();
        int currentLap = RaceCar.GetLap();
        float lapsRemaining = _raceRunner.RaceTrack.LapCount - currentLap - 1;
        Debug.Log($"{RaceCar.DriverName} is determining a strategy on lap {_lap} with {currentTireWearLaps} wear laps on their tires, projected next lap modifier was {RaceCar.GetProjectedTireModifier():f2}");
        if (HardAI)
        {
            if (RaceCar.Aggression < 25)
            {
                lapsRemaining *= 0.9f;
            }
            else if (RaceCar.Aggression < 50)
            {
                
            }
            else if (RaceCar.Aggression < 75)
            {
                lapsRemaining *= 1.1f;
            }
            else
            {
                lapsRemaining *= 1.2f;
            }
        }

        Node rootStrategy = new Node(lapsRemaining);
        Queue<Node> fringe = new Queue<Node>();
        List<Node> strategies = new List<Node>();
        fringe.Enqueue(new Node(new SoftTires(), rootStrategy));
        fringe.Enqueue(new Node(new MediumTires(), rootStrategy));
        fringe.Enqueue(new Node(new HardTires(), rootStrategy));
        while (fringe.Count > 0)
        {
            Node currentNode = fringe.Dequeue();
            if (currentNode.GetLapsRemaining() > 0)
            {
                fringe.Enqueue(new Node(new SoftTires(), currentNode));
                fringe.Enqueue(new Node(new MediumTires(), currentNode));
                fringe.Enqueue(new Node(new HardTires(), currentNode));
            }
            strategies.Add(currentNode);
        }

        strategies.Sort(
            (strategyA, strategyB) => (Mathf.Abs(strategyA.GetLapsRemaining()) + strategyA.GetStops() * 2)
                .CompareTo(Mathf.Abs(strategyB.GetLapsRemaining()) + strategyB.GetStops() * 2));
        var viableStrategies = strategies.GetRange(0, Mathf.Min(5, strategies.Count));
        Debug.Log($"{RaceCar.DriverName} found {strategies.Count} usable strategies and has selected the {viableStrategies.Count} most viable.");
        Node strategy = viableStrategies[Random.Range(0, viableStrategies.Count)];
        if (strategy.GetLapsRemaining() < 0 && strategy.GetStops() == 1)
        {
            Debug.Log($"Possible low quality strategy chosen by {RaceCar.DriverName}, investigating...");
            float tireWearLaps = RaceCar.GetTireWearLaps();
            float timeWithCurrentTires = RaceCar.currentTireType switch
            {
                (TireType.Soft) => new SoftTires().GetIntegralBetweenLaps(tireWearLaps + 1,
                    tireWearLaps + lapsRemaining),
                (TireType.Medium) => new MediumTires().GetIntegralBetweenLaps(tireWearLaps + 1,
                    tireWearLaps + lapsRemaining),
                (TireType.Hard) => new HardTires().GetIntegralBetweenLaps(tireWearLaps + 1,
                    tireWearLaps + lapsRemaining),
                _ => float.MaxValue
            };
            float timeWithChange = strategy.GetTires().GetIntegralBetweenLaps(0, lapsRemaining) + RaceCar.GetPitTime();
            if (timeWithCurrentTires > timeWithChange)
            {
                Debug.Log($"{RaceCar.DriverName} strategy cleared, time with change {timeWithChange:f2} is less than continuing {timeWithCurrentTires:f2}");
                RaceCar.nextTireType = strategy.GetFirstTires().GetTireType();
                RaceCar.PitFlag = true;
            }
            else
            {
                Debug.Log($"{RaceCar.DriverName} strategy rejected, time with change {timeWithChange:f2} is worse than continuing {timeWithCurrentTires:f2}");
            }
        }
        else
        {
            Debug.Log(
                $"{RaceCar.DriverName} has made a strategy call, {strategy.GetStrategyString()}");
            RaceCar.nextTireType = strategy.GetFirstTires().GetTireType();
            Debug.Log($"{RaceCar.DriverName} next tires called as {RaceCar.nextTireType}");
            RaceCar.PitFlag = true;
        }
    }



}
