using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RosterManager rosterManager;
    [SerializeField] private Transform rosterContainer;
    [SerializeField] private AgentUIEntry agentCardPrefab;

    private void OnEnable()
    {
        if (rosterManager != null)
        {
            rosterManager.OnRosterChanged += RefreshRosterUI;
        }
    }

    private void OnDisable()
    {
        if (rosterManager != null)
        {
            rosterManager.OnRosterChanged -= RefreshRosterUI;
        }
    }

    private void Start()
    {
        RefreshRosterUI();
    }

    public void RefreshRosterUI()
    {
        if (rosterContainer == null || rosterManager == null) return;

        foreach (Transform child in rosterContainer)
        {
            Destroy(child.gameObject);
        }

        IReadOnlyList<Agent> activeAgents = rosterManager.GetActiveRoster();

        foreach (var agent in activeAgents)
        {
            if (!agent.IsBusy)
            {
                CreateAgentCard(agent);
            }
        }
    }

    private void CreateAgentCard(Agent agent)
    {
        if (agentCardPrefab == null) return;

        AgentUIEntry newCard = Instantiate(agentCardPrefab, rosterContainer);
        newCard.Initialize(agent, rosterContainer);
    }

    public void OnNextShiftButtonClicked()
    {
        if (rosterManager != null)
        {
            rosterManager.SwapShifts();
        }
    }
}