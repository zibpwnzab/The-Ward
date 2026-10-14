using System.Collections.Generic;
using UnityEngine;

public class RosterManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Сколько усталости восстанавливается за одну ночь сна")]
    [SerializeField] private int restAmountPerShift = 30;

    [Header("Initial Data")]
    [Tooltip("Все доступные профили агентов для старта игры")]
    [SerializeField] private List<AgentProfileSO> allAgentProfiles;

    // Списки живых агентов
    private List<Agent> _shiftAlpha = new List<Agent>();
    private List<Agent> _shiftBeta = new List<Agent>();

    // Текущее состояние: true = работает Альфа, false = работает Бета
    public bool IsAlphaShiftActive { get; private set; } = true;

    private void Awake()
    {
        InitializeRoster();
    }

    public void InitializeRoster()
    {
        // Раскидываем агентов по сменам. 
        // Четные в Альфу, нечетные в Бету. Можно сделать умнее, но пока так.

        for (int i = 0; i < allAgentProfiles.Count; i++)
        {
            Agent newAgent = new Agent(allAgentProfiles[i]);

            if (i % 2 == 0)
            {
                _shiftAlpha.Add(newAgent);
            }
            else
            {
                _shiftBeta.Add(newAgent);
            }
        }

        Debug.Log($"Roster initialized. Alpha: {_shiftAlpha.Count}, Beta: {_shiftBeta.Count}");
    }

    // Метод, который возвращает список тех, кто сейчас доступен для работы
    public List<Agent> GetActiveRoster()
    {
        return IsAlphaShiftActive ? _shiftAlpha : _shiftBeta;
    }

    // Главная кнопка "Закончить смену"
    public void SwapShifts()
    {
        // 1. Определяем, кто сейчас СПАЛ (то есть НЕ активная смена)
        List<Agent> sleepingShift = IsAlphaShiftActive ? _shiftBeta : _shiftAlpha;

        // 2. Определяем, кто сейчас РАБОТАЛ
        List<Agent> workingShift = IsAlphaShiftActive ? _shiftAlpha : _shiftBeta;

        // 3. Восстанавливаем силы тем, кто спал (они готовятся выйти)
        foreach (var agent in sleepingShift)
        {
            agent.Rest(restAmountPerShift);
        }

        // 4. (Опционально) Здесь можно добавить логику усталости для тех, кто работал,
        // если они просто сидели в офисе, но обычно усталость капает от миссий.

        // 5. Переключаем смену
        IsAlphaShiftActive = !IsAlphaShiftActive;

        Debug.Log($"Shift Swapped! Now working: {(IsAlphaShiftActive ? "Alpha" : "Beta")}");
    }
}