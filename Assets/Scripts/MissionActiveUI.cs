using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Нужно для корутин
using System.Collections.Generic;

public class MissionActiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject windowObject;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image missionImage; // Теперь это поле просто висит, код его не трогает при открытии
    [SerializeField] private TextMeshProUGUI resultText; // <--- ДОБАВЬ ЭТОТ ТЕКСТ В ПРЕФАБ (WIN/LOSE)

    [Header("Charts")]
    [SerializeField] private UI_RadarChart teamChart;
    [SerializeField] private UI_RadarChart missionChart;

    [Header("Ball Mini-Game")]
    [SerializeField] private MissionBall ballScript;
    [SerializeField] private Button proceedButton;

    private System.Action _onMissionSuccess;

    public void Open(MissionSO mission, List<Agent> agents, System.Action onSuccess)
    {
        _onMissionSuccess = onSuccess;

        windowObject.SetActive(true);
        Time.timeScale = 0f; // Пауза

        // Сброс текстов и мячика
        if (resultText != null) resultText.text = "";
        ballScript.gameObject.SetActive(false); // Скрываем мяч пока не нажали старт

        if (titleText != null) titleText.text = $"ARRIVED AT: {mission.title}";

        // === ИСПРАВЛЕНИЕ ЗДЕСЬ ===
        // Мы просто комментируем (или удаляем) этот блок.
        // Код больше не пытается подставить картинку из MissionSO.
        /*
        if (missionImage != null && mission.missionImage != null)
            missionImage.sprite = mission.missionImage;
        */
        // =========================

        UpdateCharts(mission, agents);
        // === ЗАПУСК АНИМАЦИИ ===
        StartCoroutine(AnimateChartsSequence()); proceedButton.interactable = true;
        proceedButton.gameObject.SetActive(true);
    }
    // ... остальной код оставляем без изменений ...
    private IEnumerator AnimateChartsSequence()
    {
        float durationPerChart = 0.4f; // Время на каждый график
        float timer = 0f;

        // 1. Сброс в ноль
        if (teamChart != null) teamChart.SetAnimationProgress(0f);
        if (missionChart != null) missionChart.SetAnimationProgress(0f);

        // 2. ЭТАП А: Растет Красный (Миссия)
        if (missionChart != null)
        {
            timer = 0f;
            while (timer < durationPerChart)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / durationPerChart);
                // Эффект пружины (BackOut) для сочности
                float swing = progress * progress * (2.7f * progress - 1.7f);
                // Или просто Sin: Mathf.Sin(progress * Mathf.PI * 0.5f);

                missionChart.SetAnimationProgress(progress); // Используем progress для плавности
                yield return null;
            }
            missionChart.SetAnimationProgress(1f);
        }

        // Небольшая пауза между графиками
        yield return new WaitForSecondsRealtime(0.1f);

        // 3. ЭТАП Б: Растет Зеленый (Команда) поверх
        if (teamChart != null)
        {
            timer = 0f;
            while (timer < durationPerChart)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / durationPerChart);
                float eased = Mathf.Sin(progress * Mathf.PI * 0.5f);

                teamChart.SetAnimationProgress(eased);
                yield return null;
            }
            teamChart.SetAnimationProgress(1f);
        }
    }
    private void UpdateCharts(MissionSO mission, List<Agent> agents)
    {
        // Порядок статов: 0:Weaponry, 1:Coding, 2:Stealth, 3:Cruelty, 4:Charisma
        float[] missionStats = new float[5];
        float[] teamStats = new float[5];

        // --- А. Считаем Требования Миссии ---
        // Если миссия требует навык уровня 5, мы делим на 8 (максимум), получаем ~0.6
        if (mission.requiredStats != null)
        {
            foreach (var req in mission.requiredStats)
            {
                int index = GetStatIndex(req.stat);
                if (index != -1)
                {
                    missionStats[index] = Mathf.Clamp01((float)req.minLevel / 8.0f);
                }
            }
        }

        // --- Б. Считаем Силу Команды ---
        // Суммируем статы всех агентов и делим на их количество
        if (agents != null && agents.Count > 0)
        {
            foreach (var agent in agents)
            {
                teamStats[0] += agent.GetEffectiveStat(StatType.Weaponry);
                teamStats[1] += agent.GetEffectiveStat(StatType.Coding);
                teamStats[2] += agent.GetEffectiveStat(StatType.Stealth);
                teamStats[3] += agent.GetEffectiveStat(StatType.Cruelty);
                teamStats[4] += agent.GetEffectiveStat(StatType.Charisma);
            }

            // Усредняем и нормализуем (делим на 8.0 - макс уровень)
            for (int i = 0; i < 5; i++)
            {
                teamStats[i] /= agents.Count; // Среднее арифметическое
                teamStats[i] /= 8.0f;         // Перевод в 0.0 - 1.0
            }
        }

        // Отправляем данные в "рисовалку"
        if (missionChart != null) missionChart.SetStats(missionStats);
        if (teamChart != null) teamChart.SetStats(teamStats);
    }

    // Помощник: переводим enum в номер индекса массива
    private int GetStatIndex(StatType type)
    {
        switch (type)
        {
            case StatType.Weaponry: return 0;
            case StatType.Coding: return 1;
            case StatType.Stealth: return 2;
            case StatType.Cruelty: return 3;
            case StatType.Charisma: return 4;
            default: return -1;
        }
    }

    // ЭТОТ МЕТОД ПРИВЯЖИ К КНОПКЕ "START OPERATION"
    public void OnProceedClicked()
    {
        // 1. Выключаем кнопку, чтобы не нажали дважды
        proceedButton.interactable = false;

        // 2. Включаем мячик
        ballScript.gameObject.SetActive(true);

        // 3. ЗАПУСКАЕМ МЯЧИК!
        // Передаем ему графики, чтобы он знал границы
        ballScript.Launch(missionChart, teamChart, OnBallStopped);
    }
    // Этот метод вызовется, когда мячик остановится
    private void OnBallStopped(bool isWin)
    {
        // Запускаем задержку перед закрытием
        StartCoroutine(ShowResultAndClose(isWin));
    }

    // === НОВАЯ КОРУТИНА ЗАДЕРЖКИ ===
    private IEnumerator ShowResultAndClose(bool isWin)
    {
        if (resultText != null)
        {
            resultText.text = isWin ? "SUCCESS!" : "FAILURE...";
            resultText.color = isWin ? Color.green : Color.red;
        }

        Debug.Log($"Result: {isWin}. Closing in 2 seconds...");

        // Ждем 2 секунды РЕАЛЬНОГО времени (игнорируя паузу)
        yield return new WaitForSecondsRealtime(2.0f);

        CloseAndProceed();
    }

    private void CloseAndProceed()
    {
        // 1. Снимаем паузу (ВАЖНО!)
        Time.timeScale = 1f;

        // 2. Закрываем окно
        windowObject.SetActive(false);

        // 3. Продолжаем миссию
        _onMissionSuccess?.Invoke();
    }
}