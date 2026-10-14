using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAgentProfile", menuName = "Dispatch/Agent Profile")]
public class AgentProfileSO : ScriptableObject
{
    [Serializable]
    public struct StatConfig
    {
        public StatType statType;
        [Range(Agent.MinStatValue, Agent.MaxStatValue)] public int baseValue;
    }

    [Header("Bio")]
    [SerializeField] private string agentName;
    [SerializeField] private Sprite portrait;

    [Header("Base Stats")]
    [SerializeField] private List<StatConfig> stats = new List<StatConfig>();

    public string AgentName => agentName;
    public Sprite Portrait => portrait;

    public Dictionary<StatType, int> GetStatsDictionary()
    {
        var dict = new Dictionary<StatType, int>();

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            dict[type] = Agent.MinStatValue;
        }

        if (stats != null)
        {
            foreach (var config in stats)
            {
                dict[config.statType] = Mathf.Clamp(config.baseValue, Agent.MinStatValue, Agent.MaxStatValue);
            }
        }

        return dict;
    }
}