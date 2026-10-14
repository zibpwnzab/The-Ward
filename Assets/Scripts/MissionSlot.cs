using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MissionSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    private Agent _assignedAgent;
    private DispatchWindow _parentWindow;

    public void Initialize(DispatchWindow window)
    {
        _parentWindow = window;
        _assignedAgent = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            var agentCard = eventData.pointerDrag.GetComponent<AgentUIEntry>();
            if (agentCard != null)
            {
                AssignAgent(agentCard);
            }
        }
    }

    private void AssignAgent(AgentUIEntry card)
    {
        // ПРОВЕРКА 1: А есть ли данные в самой карточке?
        if (card.LinkedAgent == null)
        {
            Debug.LogError("ОШИБКА: В перетаскиваемой карточке нет данных об агенте (LinkedAgent == null)!");
            return;
        }

        // Если слот занят — выгоняем старого
        if (_assignedAgent != null && transform.childCount > 0)
        {
            var oldCard = GetComponentInChildren<AgentUIEntry>();
            if (oldCard != null && oldCard != card)
            {
                oldCard.ReturnToRoster();
            }
        }

        // Присваиваем агента
        _assignedAgent = card.LinkedAgent;

        Debug.Log($"Слот принял агента: {_assignedAgent.Name}"); // Лог успеха

        // Визуально забираем карточку
        card.transform.SetParent(transform);
        card.SetSlotMode(true);

        // Сообщаем окну пересчитать
        if (_parentWindow != null)
        {
            _parentWindow.OnTeamChanged();
        }
        else
        {
            Debug.LogError("ОШИБКА: Слот не знает про DispatchWindow (Initialize не был вызван)!");
        }
    }

    public void OnAgentRemoved()
    {
        Debug.Log("Агент убран из слота.");
        _assignedAgent = null;
        if (_parentWindow != null) _parentWindow.OnTeamChanged();
    }

    public Agent GetAgent()
    {
        return _assignedAgent;
    }
}