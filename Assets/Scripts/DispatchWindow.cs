using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DispatchWindow : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MissionSlot slotPrefab; // Ссылка на префаб слота!
    [SerializeField] private Transform slotsContainer; // Куда спавнить слоты
    [SerializeField] private GameObject windowObject; // Сама панель (чтобы вкл/выкл)

    [Header("UI Info")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI chanceText;
    [SerializeField] private Image missionImage;

    [Header("Dispatch Settings")]
    [SerializeField] private SquadController squadPrefab; // Ссылка на префаб машинки
    [SerializeField] private Transform mapContainer;      // Где создавать машинку (обычно там же, где карта)
    [SerializeField] private Transform hqTransform;       // Ссылка на объект HQ_Location
    private MissionSO _currentMission;
    private MissionNode _currentMissionNode;
    private List<MissionSlot> _activeSlots = new List<MissionSlot>();

    // Этот метод вызывается красной точкой
    public void OpenWindow(MissionSO mission, MissionNode nodeCaller)
    {
        _currentMission = mission;
        _currentMissionNode = nodeCaller; // Запоминаем ссылку

        windowObject.SetActive(true);

        titleText.text = mission.title.ToUpper();
        descriptionText.text = mission.description;
        if (mission.missionImage != null) missionImage.sprite = mission.missionImage;

        CreateSlots();
        RecalculateChance();
    }
    // Привяжи этот метод к кнопке SEND в инспекторе
    public void OnDispatchClicked()
    {
        // 1. Собираем команду
        List<Agent> team = new List<Agent>();
        foreach (var slot in _activeSlots)
        {
            Agent a = slot.GetAgent();
            if (a != null) team.Add(a);
        }

        if (team.Count == 0)
        {
            Debug.LogWarning("Нельзя отправить пустой отряд!");
            return;
        }

        // 2. Блокируем агентов
        foreach (var agent in team) agent.isBusy = true;

        // 3. Создаем машинку
        SquadController newSquad = Instantiate(squadPrefab, mapContainer);

        // === ИСПРАВЛЕНИЕ ЗДЕСЬ ===

        // Готовим список точек для маршрута
        List<Vector3> routePoints = new List<Vector3>();

        // Если в миссии указан маршрут (DistrictRoute)
        if (_currentMission.assignedRoute != null)
        {
            // Берем точки из маршрута
            routePoints = _currentMission.assignedRoute.GetWaypoints();
        }
        else
        {
            // Фолбэк: Если маршрута нет, просто стартуем с базы
            // Но машинка ждет СПИСОК, поэтому кладем базу в список
            routePoints.Add(hqTransform.position);
            Debug.LogWarning($"У миссии '{_currentMission.title}' нет маршрута! Стартуем с HQ по прямой.");
        }

        // Время миссии (пока хардкод 5 сек, потом бери из _currentMission.durationTurns)
        float travelTime = 5.0f;

        // Передаем теперь routePoints (СПИСОК), а не hqPos
        newSquad.Setup(routePoints, _currentMissionNode, team, travelTime, _currentMission);    

        // =========================

        CloseWindow();
    }



    private void CreateSlots()
    {
        // Удаляем старые слоты
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);
        _activeSlots.Clear();

        // Создаем новые
        for (int i = 0; i < _currentMission.maxAgents; i++)
        {
            MissionSlot newSlot = Instantiate(slotPrefab, slotsContainer);
            newSlot.Initialize(this); // Передаем ссылку на себя (DispatchWindow) в слот
            _activeSlots.Add(newSlot);
        }
    }

    // Этот метод вызывает MissionSlot, когда в него падает агент
    public void OnTeamChanged()
    {
        RecalculateChance();
    }

    private void RecalculateChance()
    {
        float totalChance = 0;
        int agentsCount = 0;

        foreach (var slot in _activeSlots)
        {
            Agent agent = slot.GetAgent();
            if (agent != null)
            {
                totalChance += _currentMission.CalculateSuccessChance(agent);
                agentsCount++;
            }
        }

        // Логика среднего шанса
        float finalChance = agentsCount > 0 ? totalChance / agentsCount : 0;

        int percentage = Mathf.RoundToInt(finalChance * 100);
        chanceText.text = $"CHANCE: {percentage}%";

        if (percentage > 70) chanceText.color = Color.green;
        else if (percentage < 40) chanceText.color = Color.red;
        else chanceText.color = Color.white;
    }

    public void CloseWindow()
    {
        // Тут надо бы возвращать агентов в ростер, но пока просто скроем
        windowObject.SetActive(false);
    }
}