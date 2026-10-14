using UnityEngine;
using UnityEngine.UI;

public class MissionBall : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float initialSpeed = 500f;
    [SerializeField] private float friction = 200f; // Торможение
    [SerializeField] private float bounceForce = 0.8f; // Упругость (1 = вечно скачет, 0.5 = гасит скорость)

    private RectTransform _rect;
    private Vector2 _velocity;
    private bool _isMoving = false;

    // Данные для проверки
    private UI_RadarChart _missionChart; // Красный (Стены)
    private UI_RadarChart _teamChart;    // Зеленый (Зона победы)
    private System.Action<bool> _onResult; // Коллбек (true = win, false = loss)

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void Launch(UI_RadarChart mission, UI_RadarChart team, System.Action<bool> callback)
    {
        _missionChart = mission;
        _teamChart = team;
        _onResult = callback;

        // Сброс позиции в центр
        _rect.anchoredPosition = Vector2.zero;

        // Запуск в случайную сторону
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        _velocity = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * initialSpeed;

        _isMoving = true;
    }

    private void Update()
    {
        if (!_isMoving) return;

        // 1. Движение
        _rect.anchoredPosition += _velocity * Time.unscaledDeltaTime; // Используем unscaled, так как игра на паузе!

        // 2. Трение (замедление)
        if (_velocity.magnitude > 0)
        {
            _velocity -= _velocity.normalized * friction * Time.unscaledDeltaTime;

            // Если скорость совсем маленькая - останавливаемся
            if (_velocity.magnitude < 10f)
            {
                StopAndCheckResult();
                return;
            }
        }

        // 3. Проверка удара о стену (Красный график)
        CheckWallCollision();
    }

    private void CheckWallCollision()
    {
        // Переводим текущую позицию в полярные координаты (Угол и Дистанция)
        Vector2 pos = _rect.anchoredPosition;
        float dist = pos.magnitude;

        // Если мы в центре, стен нет
        if (dist < 1f) return;

        float angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg; // -180..180
        if (angle < 0) angle += 360;

        // Спрашиваем у красного графика: "Где стена под этим углом?"
        float wallDist = _missionChart.GetRadiusAtAngle(angle);

        // Если вылетели за стену
        if (dist > wallDist)
        {
            // Возвращаем мяч внутрь (чтобы не пролетел сквозь стену)
            _rect.anchoredPosition = pos.normalized * (wallDist - 2f);

            // ОТСКОК:
            // Для простоты отражаем вектор скорости относительно центра. 
            // (Это не физически точно для многоугольника, но для UI-шарика выглядит норм)
            // Можно усложнить, считая нормаль к грани, но начни с этого.

            Vector2 normal = -pos.normalized; // Вектор к центру
            _velocity = Vector2.Reflect(_velocity, normal) * bounceForce;

            // Иногда добавляем чуть хаоса, чтобы не застрял
            _velocity += Random.insideUnitCircle * 50f;
        }
    }

    private void StopAndCheckResult()
    {
        _isMoving = false;
        _velocity = Vector2.zero;

        // ПРОВЕРКА ПОБЕДЫ
        // Мы остановились. Мы точно внутри Красного (стен). 
        // Вопрос: мы внутри Зеленого?

        Vector2 pos = _rect.anchoredPosition;
        float angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        float teamRadius = _teamChart.GetRadiusAtAngle(angle);
        float currentDist = pos.magnitude;

        // Если наше расстояние от центра МЕНЬШЕ, чем радиус зеленого графика в этой точке
        // Значит, мы внутри зеленой зоны.
        bool isWin = currentDist <= teamRadius;

        Debug.Log($"Ball Stopped. Dist: {currentDist}, TeamLimit: {teamRadius}. Result: {isWin}");

        _onResult?.Invoke(isWin);
    }
}