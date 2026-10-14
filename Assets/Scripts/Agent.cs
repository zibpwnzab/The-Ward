using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Agent
{
    public const int MinStatValue = 0;
    public const int MaxStatValue = 8;
    public const int MaxFatigue = 100;
    private const int FatiguePenaltyStep = 20;

    private readonly Dictionary<StatType, int> _baseStats;

    public string Name { get; }
    public Sprite Portrait { get; }
    public bool IsBusy { get; private set; }
    public int Fatigue { get; private set; }

    public Agent(AgentProfileSO profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));

        Name = profile.AgentName;
        Portrait = profile.Portrait;
        Fatigue = 0;
        IsBusy = false;
        _baseStats = profile.GetStatsDictionary();
    }

    public void SetBusy(bool isBusy)
    {
        IsBusy = isBusy;
    }

    public int GetEffectiveStat(StatType type)
    {
        if (!_baseStats.TryGetValue(type, out int baseValue))
        {
            return MinStatValue;
        }

        int penalty = Fatigue / FatiguePenaltyStep;
        return Mathf.Clamp(baseValue - penalty, MinStatValue, MaxStatValue);
    }

    public float CalculateEfficiencyScore()
    {
        if (_baseStats.Count == 0) return 0f;

        float sum = 0f;
        foreach (StatType type in _baseStats.Keys)
        {
            sum += GetEffectiveStat(type);
        }

        float average = sum / _baseStats.Count;
        return (float)Math.Round(average, 1);
    }

    public void AddFatigue(int amount)
    {
        if (amount <= 0) return;
        Fatigue = Mathf.Clamp(Fatigue + amount, 0, MaxFatigue);
    }

    public void Rest(int amount)
    {
        if (amount <= 0) return;
        Fatigue = Mathf.Clamp(Fatigue - amount, 0, MaxFatigue);
    }
}