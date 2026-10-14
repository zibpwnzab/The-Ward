using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionActiveUI : MonoBehaviour
{
    private const float ChartAnimationDuration = 0.4f;
    private const float InterChartDelay = 0.1f;
    private const float ResultDisplayDuration = 2.0f;

    [Header("UI References")]
    [SerializeField] private GameObject windowObject;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Charts")]
    [SerializeField] private UI_RadarChart teamChart;
    [SerializeField] private UI_RadarChart missionChart;

    [Header("Ball Mini-Game")]
    [SerializeField] private MissionBall ballScript;
    [SerializeField] private Button proceedButton;

    private Action<bool> _onMissionResolved;

    public void Open(MissionSO mission, List<Agent> agents, Action<bool> onMissionResolved)
    {
        _onMissionResolved = onMissionResolved;

        windowObject.SetActive(true);
        Time.timeScale = 0f;

        if (resultText != null) resultText.text = string.Empty;
        if (ballScript != null) ballScript.gameObject.SetActive(false);
        if (titleText != null && mission != null) titleText.text = $"ARRIVED AT: {mission.title}";

        if (proceedButton != null)
        {
            proceedButton.interactable = true;
            proceedButton.gameObject.SetActive(true);
        }

        UpdateCharts(mission, agents);
        StartCoroutine(AnimateChartsSequence());
    }

    private IEnumerator AnimateChartsSequence()
    {
        if (teamChart != null) teamChart.SetAnimationProgress(0f);
        if (missionChart != null) missionChart.SetAnimationProgress(0f);

        if (missionChart != null)
        {
            yield return AnimateChart(missionChart, ChartAnimationDuration);
        }

        yield return new WaitForSecondsRealtime(InterChartDelay);

        if (teamChart != null)
        {
            yield return AnimateChart(teamChart, ChartAnimationDuration);
        }
    }

    private IEnumerator AnimateChart(UI_RadarChart chart, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float eased = Mathf.Sin(progress * Mathf.PI * 0.5f);
            chart.SetAnimationProgress(eased);
            yield return null;
        }
        chart.SetAnimationProgress(1f);
    }

    private void UpdateCharts(MissionSO mission, List<Agent> agents)
    {
        StatType[] statTypes =
        {
            StatType.Weaponry,
            StatType.Coding,
            StatType.Stealth,
            StatType.Cruelty,
            StatType.Charisma
        };

        int count = statTypes.Length;
        float[] missionStats = new float[count];
        float[] teamStats = new float[count];

        for (int i = 0; i < count; i++)
        {
            StatType type = statTypes[i];

            int requiredLevel = mission != null ? mission.GetRequiredLevel(type, defaultBaseline: 2) : 2;
            missionStats[i] = Mathf.Clamp01((float)requiredLevel / Agent.MaxStatValue);

            if (agents != null && agents.Count > 0)
            {
                float sum = 0f;
                foreach (var agent in agents)
                {
                    sum += agent.GetEffectiveStat(type);
                }
                teamStats[i] = Mathf.Clamp01((sum / agents.Count) / Agent.MaxStatValue);
            }
        }

        if (missionChart != null) missionChart.SetStats(missionStats);
        if (teamChart != null) teamChart.SetStats(teamStats);
    }

    public void OnProceedClicked()
    {
        if (proceedButton != null) proceedButton.interactable = false;

        if (ballScript != null)
        {
            ballScript.gameObject.SetActive(true);
            ballScript.Launch(missionChart, teamChart, OnBallStopped);
        }
        else
        {
            OnBallStopped(true);
        }
    }

    private void OnBallStopped(bool isWin)
    {
        StartCoroutine(ShowResultAndClose(isWin));
    }

    private IEnumerator ShowResultAndClose(bool isWin)
    {
        if (resultText != null)
        {
            resultText.text = isWin ? "SUCCESS!" : "FAILURE...";
            resultText.color = isWin ? Color.green : Color.red;
        }

        yield return new WaitForSecondsRealtime(ResultDisplayDuration);
        CloseAndProceed(isWin);
    }

    private void CloseAndProceed(bool isWin)
    {
        Time.timeScale = 1f;
        windowObject.SetActive(false);
        _onMissionResolved?.Invoke(isWin);
    }
}