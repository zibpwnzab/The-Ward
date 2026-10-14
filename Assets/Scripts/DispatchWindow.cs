using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DispatchWindow : MonoBehaviour
{
    private const int HighChanceThreshold = 70;
    private const int LowChanceThreshold = 40;

    [Header("System References")]
    [SerializeField] private RosterManager rosterManager;
    [SerializeField] private MissionActiveUI missionActiveUI;

    [Header("Window Configuration")]
    [SerializeField] private MissionSlot slotPrefab;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject windowObject;

    [Header("UI Info")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI chanceText;
    [SerializeField] private Image missionImage;

    [Header("Dispatch Settings")]
    [SerializeField] private SquadController squadPrefab;
    [SerializeField] private Transform mapContainer;
    [SerializeField] private Transform hqTransform;

    private MissionSO _currentMission;
    private MissionNode _currentMissionNode;
    private readonly List<MissionSlot> _activeSlots = new List<MissionSlot>();

    public void OpenWindow(MissionSO mission, MissionNode nodeCaller)
    {
        if (mission == null) return;

        ReturnSlottedAgentsToRoster();

        _currentMission = mission;
        _currentMissionNode = nodeCaller;

        windowObject.SetActive(true);

        if (titleText != null) titleText.text = mission.title.ToUpperInvariant();
        if (descriptionText != null) descriptionText.text = mission.description;
        if (missionImage != null && mission.missionImage != null) missionImage.sprite = mission.missionImage;

        CreateSlots();
        RecalculateChance();
    }

    public void OnDispatchClicked()
    {
        var team = new List<Agent>();
        foreach (var slot in _activeSlots)
        {
            if (slot.AssignedAgent != null)
            {
                team.Add(slot.AssignedAgent);
            }
        }

        if (team.Count == 0) return;

        foreach (var agent in team)
        {
            agent.SetBusy(true);
        }

        foreach (var slot in _activeSlots)
        {
            slot.ConsumeCardForDispatch();
        }

        var routePoints = new List<Vector3>();
        if (_currentMission.assignedRoute != null)
        {
            routePoints = _currentMission.assignedRoute.GetWaypoints();
        }
        else if (hqTransform != null)
        {
            routePoints.Add(hqTransform.position);
        }

        SquadController newSquad = Instantiate(squadPrefab, mapContainer);
        newSquad.Initialize(
            routePoints,
            _currentMissionNode,
            team,
            _currentMission,
            HandleSquadArrived,
            HandleSquadReturned);

        windowObject.SetActive(false);
    }

    private void HandleSquadArrived(MissionSO mission, List<Agent> agents, System.Action<bool> onMiniGameComplete)
    {
        if (missionActiveUI != null)
        {
            missionActiveUI.Open(mission, agents, onMiniGameComplete);
        }
        else
        {
            onMiniGameComplete?.Invoke(true);
        }
    }

    private void HandleSquadReturned(List<Agent> agents, bool isSuccess)
    {
        if (rosterManager != null)
        {
            rosterManager.NotifyRosterChanged();
        }
    }

    private void CreateSlots()
    {
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        _activeSlots.Clear();

        for (int i = 0; i < _currentMission.maxAgents; i++)
        {
            MissionSlot newSlot = Instantiate(slotPrefab, slotsContainer);
            newSlot.Initialize(RecalculateChance);
            _activeSlots.Add(newSlot);
        }
    }

    private void RecalculateChance()
    {
        if (_currentMission == null || chanceText == null) return;

        float totalChance = 0f;
        int agentsCount = 0;

        foreach (var slot in _activeSlots)
        {
            Agent agent = slot.AssignedAgent;
            if (agent != null)
            {
                totalChance += _currentMission.CalculateSuccessChance(agent);
                agentsCount++;
            }
        }

        float finalChance = agentsCount > 0 ? totalChance / agentsCount : 0f;
        int percentage = Mathf.RoundToInt(finalChance * 100f);

        chanceText.text = $"CHANCE: {percentage}%";

        if (percentage >= HighChanceThreshold) chanceText.color = Color.green;
        else if (percentage < LowChanceThreshold) chanceText.color = Color.red;
        else chanceText.color = Color.white;
    }

    public void CloseWindow()
    {
        ReturnSlottedAgentsToRoster();
        windowObject.SetActive(false);
    }

    private void ReturnSlottedAgentsToRoster()
    {
        foreach (var slot in _activeSlots)
        {
            if (slot != null)
            {
                slot.ReleaseCardToRoster();
            }
        }
    }
}