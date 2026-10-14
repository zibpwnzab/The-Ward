using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Agent
{
    public string Name { get; private set; }
    public bool isBusy = false; // Занят ли агент прямо сейчас
    public Sprite Portrait { get; private set; }

    // Текущая усталость (0 - свеж как огурчик, 100 - труп)
    [Range(0, 100)] public int fatigue;

    // Базовые статы (без штрафов)
    private Dictionary<StatType, int> _baseStats;

    // Константа: сколько усталости снимает 1 очко навыка.
    // Например, если penaltyStep = 20, то при усталости 21 агент получит -1 ко всем статам.
    private const int FatiguePenaltyStep = 20;

    // Конструктор: создаем живого агента из "Личного дела"
    public Agent(AgentProfileSO profile)
    {
        Name = profile.agentName;
        Portrait = profile.portrait;
        fatigue = 0; // Новый сотрудник всегда свежий
        _baseStats = profile.GetStatsDict();
    }

    // Главный метод: получить ЭФФЕКТИВНЫЙ стат (с учетом усталости)
    public int GetEffectiveStat(StatType type)
    {
        if (!_baseStats.ContainsKey(type)) return 0;

        int baseValue = _baseStats[type];

        // Считаем штраф. (int) деление отбрасывает дробную часть.
        int penalty = fatigue / FatiguePenaltyStep;

        // Стат не может быть ниже 0, даже если агент умирает от усталости
        return Mathf.Clamp(baseValue - penalty, 0, 8);
    }
    // Внутри класса Agent.cs добавь этот метод:

    public float CalculateEfficiencyScore()
    {
        // 1. Считаем сумму эффективных статов (которые уже уменьшены усталостью)
        float sum = 0;
        sum += GetEffectiveStat(StatType.Weaponry);
        sum += GetEffectiveStat(StatType.Coding);
        sum += GetEffectiveStat(StatType.Stealth);
        sum += GetEffectiveStat(StatType.Cruelty);
        sum += GetEffectiveStat(StatType.Charisma); // Если нужна харизма

        // 2. Среднее значение (максимум 8.0)
        float average = sum / 5.0f;

        // 3. Можно умножить на 10, чтобы было число типа "65", "82" (как рейтинг в FIFA)
        // Или оставить дробным "6.5". Давай сделаем дробным, стиль Superhot любит минимализм.

        return (float)System.Math.Round(average, 1); // Округляем до 1 знака (6.4)
    }
    // Метод добавления усталости после миссии
    public void AddFatigue(int amount)
    {
        fatigue = Mathf.Clamp(fatigue + amount, 0, 100);
        Debug.Log($"{Name} устал. Текущая усталость: {fatigue}. Штраф к статам: -{fatigue / FatiguePenaltyStep}");
    }

    // Метод отдыха
    public void Rest(int amount)
    {
        fatigue = Mathf.Clamp(fatigue - amount, 0, 100);
    }
}