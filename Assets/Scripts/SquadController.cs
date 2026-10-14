using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquadController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private RectTransform movingIcon;
    [SerializeField] private LineRenderer tailLine;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 400f;

    private List<Vector3> _pathPoints = new List<Vector3>();
    private List<Agent> _assignedAgents;
    private MissionNode _targetNode; // Та самая красная точка
    private MissionSO _missionData;

    public void Setup(List<Vector3> routePoints, MissionNode targetNode, List<Agent> agents, float duration, MissionSO mission)
    {
        _targetNode = targetNode; // <--- ВАЖНО: Запоминаем точку тут
        _assignedAgents = agents;
        _missionData = mission;

        // Формируем путь
        if (routePoints != null) _pathPoints.AddRange(routePoints);

        // Добавляем координаты самой цели в конец пути
        if (targetNode != null)
        {
            _pathPoints.Add(targetNode.transform.position);
        }

        // Ставим на старт
        if (_pathPoints.Count > 0)
        {
            transform.position = _pathPoints[0];
            movingIcon.position = _pathPoints[0];
        }

        DrawTail();
        StartCoroutine(MissionRoutine(duration));
    }

    private void DrawTail()
    {
        if (tailLine != null && _pathPoints.Count > 0)
        {
            tailLine.positionCount = _pathPoints.Count;
            for (int i = 0; i < _pathPoints.Count; i++)
            {
                Vector3 p = _pathPoints[i];
                p.z = -5; // Чтобы было видно поверх карты
                tailLine.SetPosition(i, p);
            }
        }
    }

    private IEnumerator MissionRoutine(float duration)
    {
        // 1. Едем ТУДА
        foreach (Vector3 point in _pathPoints)
        {
            yield return MoveTo(point);
        }

        Debug.Log("Прибыли на место! Вызываем окно...");

        // 2. ОТКРЫВАЕМ ОКНО СИТУАЦИИ
        // Ищем окно, даже если оно выключено
        var activeWindow = FindFirstObjectByType<MissionActiveUI>(FindObjectsInactive.Include);

        // Флаг, который переключит кнопка в окне
        bool isMissionStarted = false;

        if (activeWindow != null)
        {
            // Передаем данные и коллбек
            activeWindow.Open(_missionData, _assignedAgents, () => { isMissionStarted = true; });

            // Ждем, пока игрок нажмет кнопку PROCEED
            yield return new WaitUntil(() => isMissionStarted);
        }
        else
        {
            Debug.LogError("Окно MissionActiveUI не найдено! Пропускаем этап окна.");
        }

        // 3. РАБОТАЕМ (Имитация бурной деятельности)
        yield return new WaitForSeconds(duration);

        // 4. УДАЛЯЕМ МИССИЮ (ФИКС)
        if (_targetNode != null)
        {
            Debug.Log($"Уничтожаем точку: {_targetNode.name}");
            

        }
        else
        {
            // Если этот лог вылезет — значит в Setup пришел null!
            Debug.LogWarning("Красная точка не удалилась, так как ссылка пустая!");
        }

        // Убираем хвост
        if (tailLine != null) tailLine.enabled = false;

        // 5. ЕДЕМ ДОМОЙ (В обратном порядке)
        _pathPoints.Reverse();
        foreach (Vector3 point in _pathPoints)
        {
            yield return MoveTo(point);
        }

        // 6. ОСВОБОЖДАЕМ АГЕНТОВ
        foreach (var agent in _assignedAgents) agent.isBusy = false;

        // Обновляем список в UI
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null) ui.RefreshRosterUI();

        // Удаляем машинку
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        while (Vector3.Distance(movingIcon.position, destination) > 1f)
        {
            movingIcon.position = Vector3.MoveTowards(movingIcon.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}