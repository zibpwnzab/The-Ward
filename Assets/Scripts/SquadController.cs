using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    private const float ArrivalThreshold = 1f;
    private const float TailZOffset = -5f;

    [Header("Visuals")]
    [SerializeField] private RectTransform movingIcon;
    [SerializeField] private LineRenderer tailLine;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 400f;

    private readonly List<Vector3> _pathPoints = new List<Vector3>();
    private List<Agent> _assignedAgents;
    private MissionNode _targetNode;
    private MissionSO _missionData;
    private Action<MissionSO, List<Agent>, Action<bool>> _onSquadArrived;
    private Action<List<Agent>, bool> _onSquadReturned;

    public void Initialize(
        List<Vector3> routePoints,
        MissionNode targetNode,
        List<Agent> agents,
        MissionSO mission,
        Action<MissionSO, List<Agent>, Action<bool>> onSquadArrived,
        Action<List<Agent>, bool> onSquadReturned)
    {
        _targetNode = targetNode;
        _assignedAgents = agents;
        _missionData = mission;
        _onSquadArrived = onSquadArrived;
        _onSquadReturned = onSquadReturned;

        if (_targetNode != null)
        {
            _targetNode.SetInteractable(false);
        }

        if (routePoints != null)
        {
            _pathPoints.AddRange(routePoints);
        }

        if (_targetNode != null)
        {
            _pathPoints.Add(_targetNode.transform.position);
        }

        if (_pathPoints.Count > 0)
        {
            transform.position = _pathPoints[0];
            if (movingIcon != null) movingIcon.position = _pathPoints[0];
        }

        DrawTail();
        StartCoroutine(MissionRoutine());
    }

    private void DrawTail()
    {
        if (tailLine == null || _pathPoints.Count == 0) return;

        tailLine.positionCount = _pathPoints.Count;
        for (int i = 0; i < _pathPoints.Count; i++)
        {
            Vector3 point = _pathPoints[i];
            point.z = TailZOffset;
            tailLine.SetPosition(i, point);
        }
    }

    private IEnumerator MissionRoutine()
    {
        foreach (Vector3 point in _pathPoints)
        {
            yield return MoveTo(point);
        }

        bool isMiniGameResolved = false;
        bool isMissionSuccessful = false;

        if (_onSquadArrived != null)
        {
            _onSquadArrived.Invoke(_missionData, _assignedAgents, success =>
            {
                isMissionSuccessful = success;
                isMiniGameResolved = true;
            });

            yield return new WaitUntil(() => isMiniGameResolved);
        }

        if (_missionData != null && _missionData.executionDurationSeconds > 0f)
        {
            yield return new WaitForSeconds(_missionData.executionDurationSeconds);
        }

        if (_targetNode != null)
        {
            _targetNode.CompleteAndDestroy();
        }

        if (tailLine != null)
        {
            tailLine.enabled = false;
        }

        _pathPoints.Reverse();
        foreach (Vector3 point in _pathPoints)
        {
            yield return MoveTo(point);
        }

        if (_assignedAgents != null)
        {
            int fatiguePenalty = _missionData != null ? _missionData.fatigueCost : 20;
            if (!isMissionSuccessful)
            {
                fatiguePenalty = Mathf.RoundToInt(fatiguePenalty * 1.5f);
            }

            foreach (var agent in _assignedAgents)
            {
                agent.AddFatigue(fatiguePenalty);
                agent.SetBusy(false);
            }
        }

        _onSquadReturned?.Invoke(_assignedAgents, isMissionSuccessful);
        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        if (movingIcon == null) yield break;

        while (Vector3.Distance(movingIcon.position, destination) > ArrivalThreshold)
        {
            movingIcon.position = Vector3.MoveTowards(
                movingIcon.position,
                destination,
                moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}