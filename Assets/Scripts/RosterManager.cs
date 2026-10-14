using System;
using System.Collections.Generic;
using UnityEngine;

public class RosterManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int restAmountPerShift = 30;
    [SerializeField] private List<AgentProfileSO> allAgentProfiles = new List<AgentProfileSO>();

    private readonly List<Agent> _shiftAlpha = new List<Agent>();
    private readonly List<Agent> _shiftBeta = new List<Agent>();

    public bool IsAlphaShiftActive { get; private set; } = true;
    public event Action OnRosterChanged;

    private void Awake()
    {
        InitializeRoster();
    }

    private void InitializeRoster()
    {
        _shiftAlpha.Clear();
        _shiftBeta.Clear();

        for (int i = 0; i < allAgentProfiles.Count; i++)
        {
            if (allAgentProfiles[i] == null) continue;

            var newAgent = new Agent(allAgentProfiles[i]);
            if (i % 2 == 0)
            {
                _shiftAlpha.Add(newAgent);
            }
            else
            {
                _shiftBeta.Add(newAgent);
            }
        }

        NotifyRosterChanged();
    }

    public IReadOnlyList<Agent> GetActiveRoster()
    {
        return IsAlphaShiftActive ? _shiftAlpha : _shiftBeta;
    }

    public void SwapShifts()
    {
        List<Agent> incomingShift = IsAlphaShiftActive ? _shiftBeta : _shiftAlpha;

        foreach (var agent in incomingShift)
        {
            agent.Rest(restAmountPerShift);
        }

        IsAlphaShiftActive = !IsAlphaShiftActive;
        NotifyRosterChanged();
    }

    public void NotifyRosterChanged()
    {
        OnRosterChanged?.Invoke();
    }
}