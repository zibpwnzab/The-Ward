using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAgentProfile", menuName = "Dispatch/Agent Profile")]
public class AgentProfileSO : ScriptableObject
{
    [Header("Bio")]
    public string agentName;
    public Sprite portrait;

    [Header("Base Stats (Max 8)")]
    [Range(0, 8)] public int weaponry;
    [Range(0, 8)] public int coding;
    [Range(0, 8)] public int charisma;
    [Range(0, 8)] public int stealth;
    [Range(0, 8)] public int cruelty;

    // ћетод дл€ удобного получени€ статов в виде словар€
    public Dictionary<StatType, int> GetStatsDict()
    {
        return new Dictionary<StatType, int>
        {
            { StatType.Weaponry, weaponry },
            { StatType.Coding, coding },
            { StatType.Charisma, charisma },
            { StatType.Stealth, stealth },
            { StatType.Cruelty, cruelty }
        };
    }
}