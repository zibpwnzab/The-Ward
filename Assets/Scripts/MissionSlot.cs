using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MissionSlot : MonoBehaviour, IDropHandler
{
    private AgentUIEntry _assignedCard;
    private Action _onSlotChanged;

    public Agent AssignedAgent => _assignedCard != null ? _assignedCard.LinkedAgent : null;

    public void Initialize(Action onSlotChanged)
    {
        _onSlotChanged = onSlotChanged;
        _assignedCard = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        if (eventData.pointerDrag.TryGetComponent(out AgentUIEntry agentCard))
        {
            AssignAgentCard(agentCard);
        }
    }

    private void AssignAgentCard(AgentUIEntry card)
    {
        if (card == null || card.LinkedAgent == null) return;

        if (_assignedCard != null && _assignedCard != card)
        {
            _assignedCard.ReturnToRoster();
        }

        _assignedCard = card;
        card.transform.SetParent(transform, false);
        card.SetSlotMode(true);

        _onSlotChanged?.Invoke();
    }

    public void ClearSlot()
    {
        _assignedCard = null;
        _onSlotChanged?.Invoke();
    }

    public void ReleaseCardToRoster()
    {
        if (_assignedCard != null)
        {
            AgentUIEntry card = _assignedCard;
            _assignedCard = null;
            card.ReturnToRoster();
        }
    }

    public void ConsumeCardForDispatch()
    {
        if (_assignedCard != null)
        {
            Destroy(_assignedCard.gameObject);
            _assignedCard = null;
        }
    }
}