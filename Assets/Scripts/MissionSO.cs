using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMission", menuName = "Dispatch/Mission")]
public class MissionSO : ScriptableObject
{
    [Header("Info")]
    public string title;
    [TextArea] public string description;

    [Header("Visuals")]
    public Sprite missionImage;
    public DistrictRoute assignedRoute;

    [Header("Spawn Settings")]
    [Tooltip("„ерез сколько секунд после старта игры по€витс€ этот вызов")]
    public float spawnDelaySeconds = 0f; // <--- ¬ќ“ “¬ќя Ќќ¬јя ѕ≈–≈ћ≈ЌЌјя

    [Header("Team")]
    [Range(1, 4)] public int maxAgents = 2;

    [Header("Configuration")]
    public int durationTurns = 1;

    [System.Serializable]
    public struct StatRequirement
    {
        public StatType stat;
        [Range(1, 8)] public int minLevel;
    }

    [Header("Mission Requirements")]
    public List<StatRequirement> requiredStats;

    public float CalculateSuccessChance(Agent agent)
    {
        if (requiredStats == null || requiredStats.Count == 0) return 1.0f;

        float totalScore = 0f;

        foreach (var req in requiredStats)
        {
            int agentValue = agent.GetEffectiveStat(req.stat);
            int delta = agentValue - req.minLevel;

            if (delta >= 0)
            {
                totalScore += 1.0f + Mathf.Clamp(delta * 0.1f, 0f, 0.5f);
            }
            else
            {
                totalScore += 1.0f + (delta * 0.2f);
            }
        }

        float averageChance = totalScore / requiredStats.Count;
        return Mathf.Clamp(averageChance, 0.05f, 0.95f);
    }
}