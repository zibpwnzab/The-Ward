using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Dispatch/Mission")]
public class MissionSO : ScriptableObject
{
    [Serializable]
    public struct StatRequirement
    {
        public StatType stat;
        [Range(1, Agent.MaxStatValue)] public int minLevel;
    }

    [Header("General Info")]
    public string title;
    [TextArea] public string description;
    public Sprite missionImage;

    [Header("Routing & Timing")]
    public DistrictRoute assignedRoute;
    [Min(0f)] public float spawnDelaySeconds = 0f;
    [Min(1f)] public float executionDurationSeconds = 5f;

    [Header("Team & Fatigue")]
    [Range(1, 4)] public int maxAgents = 2;
    [Range(5, 50)] public int fatigueCost = 20;

    [Header("Requirements")]
    public List<StatRequirement> requiredStats = new List<StatRequirement>();

    public int GetRequiredLevel(StatType type, int defaultBaseline = 2)
    {
        if (requiredStats != null)
        {
            foreach (var req in requiredStats)
            {
                if (req.stat == type) return req.minLevel;
            }
        }
        return defaultBaseline;
    }

    public float CalculateSuccessChance(Agent agent)
    {
        if (agent == null) return 0f;
        if (requiredStats == null || requiredStats.Count == 0) return 1.0f;

        const float BonusPerSurplusPoint = 0.1f;
        const float MaxSurplusBonus = 0.5f;
        const float PenaltyPerDeficitPoint = 0.2f;
        const float MinChance = 0.05f;
        const float MaxChance = 0.95f;

        float totalScore = 0f;

        foreach (var req in requiredStats)
        {
            int agentValue = agent.GetEffectiveStat(req.stat);
            int delta = agentValue - req.minLevel;

            if (delta >= 0)
            {
                totalScore += 1.0f + Mathf.Clamp(delta * BonusPerSurplusPoint, 0f, MaxSurplusBonus);
            }
            else
            {
                totalScore += 1.0f + (delta * PenaltyPerDeficitPoint);
            }
        }

        float averageChance = totalScore / requiredStats.Count;
        return Mathf.Clamp(averageChance, MinChance, MaxChance);
    }
}