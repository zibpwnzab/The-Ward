using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RosterManager rosterManager;
    [SerializeField] private Transform rosterContainer;
    [SerializeField] private AgentUIEntry agentCardPrefab;

    private void Start()
    {
        Invoke(nameof(RefreshRosterUI), 0.1f);
    }

    public void RefreshRosterUI()
    {
        // 1. Чистим список
        if (rosterContainer != null)
        {
            foreach (Transform child in rosterContainer) Destroy(child.gameObject);
        }

        // 2. Получаем данные
        RosterManager roster = FindFirstObjectByType<RosterManager>(); // Обновила на новый метод Unity
        if (roster == null) return;

        List<Agent> activeAgents = roster.GetActiveRoster();

        // 3. Рисуем
        foreach (var agent in activeAgents)
        {
            if (!agent.isBusy)
            {
                CreateAgentCard(agent); // <--- Теперь этот метод существует внизу!
            }
        }
    }

    // === ВОТ ТОТ САМЫЙ МЕТОД, КОТОРОГО НЕ ХВАТАЛО ===
    private void CreateAgentCard(Agent agent)
    {
        AgentUIEntry newCard = Instantiate(agentCardPrefab, rosterContainer);
        newCard.Setup(agent);
    }
    // ===============================================

    public void OnNextShiftButtonClicked()
    {
        if (rosterManager != null)
        {
            rosterManager.SwapShifts();
            RefreshRosterUI();
        }
    }
}