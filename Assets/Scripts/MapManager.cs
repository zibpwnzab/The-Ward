using System.Collections; // <--- Не забудь, без этого корутины не работают!
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Префаб красной точки (MissionNode)")]
    [SerializeField] private MissionNode nodePrefab;

    [Tooltip("Объект на карте, куда будем складывать точки")]
    [SerializeField] private Transform nodesContainer;

    [Tooltip("Окно диспетчера")]
    [SerializeField] private DispatchWindow dispatchWindow;

    [Header("Spawn Settings")]
    [Tooltip("Список пустых объектов-точек на карте")]
    [SerializeField] private List<Transform> spawnPoints;

    [Tooltip("Список миссий, которые ДОЛЖНЫ появиться на этой смене (по сценарию)")]
    [SerializeField] private List<MissionSO> plannedMissions; // Заменила availableMissions

    // Внутренний список, чтобы отслеживать, какие точки сейчас свободны
    private List<Transform> _availableSpawnPoints;

    private void Start()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("MapManager: Нет точек спавна! Карты нет, диспетчер уволился.");
            return;
        }

        if (plannedMissions == null || plannedMissions.Count == 0)
        {
            Debug.LogWarning("MapManager: Список запланированных миссий пуст. Смена будет тихой.");
            return;
        }

        // Копируем точки, чтобы можно было вычеркивать занятые
        _availableSpawnPoints = new List<Transform>(spawnPoints);

        // Проходимся по каждой бумажке с миссией и запускаем для нее таймер
        foreach (var mission in plannedMissions)
        {
            if (mission != null)
            {
                StartCoroutine(SpawnMissionWithDelay(mission));
            }
        }
    }

    private IEnumerator SpawnMissionWithDelay(MissionSO mission)
    {
        // 1. Диспетчер пьет кофе и ждет нужное количество секунд
        yield return new WaitForSeconds(mission.spawnDelaySeconds);

        // 2. Время пришло! Ищем свободное место на карте
        Transform chosenPoint = null;

        if (_availableSpawnPoints.Count > 0)
        {
            // Берем случайную СВОБОДНУЮ точку и вычеркиваем её
            int randomIndex = Random.Range(0, _availableSpawnPoints.Count);
            chosenPoint = _availableSpawnPoints[randomIndex];
            _availableSpawnPoints.RemoveAt(randomIndex);
        }
        else
        {
            // Если весь город горит и свободных точек нет — спавним куда придется (в любую базовую)
            Debug.LogWarning($"[MapManager] Нет свободных точек для миссии '{mission.title}'! Спавним поверх другой.");
            chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        }

        // 3. Вызываем скорую... то есть, создаем маркер на карте
        SpawnNode(mission, chosenPoint);
    }

    private void SpawnNode(MissionSO mission, Transform spawnPoint)
    {
        MissionNode newNode = Instantiate(nodePrefab, nodesContainer);
        newNode.transform.position = spawnPoint.position;

        newNode.Setup(mission, dispatchWindow);

        Debug.Log($"[Вызов поступил] {mission.title}. Ожидание составило: {mission.spawnDelaySeconds} сек.");
    }

    // Этот метод нужно будет вызывать, когда миссия завершилась, чтобы вернуть точку в список доступных
    public void FreeUpSpawnPoint(Transform pointToFree)
    {
        if (!_availableSpawnPoints.Contains(pointToFree))
        {
            _availableSpawnPoints.Add(pointToFree);
        }
    }
}