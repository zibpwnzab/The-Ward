using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MissionNode nodePrefab;
    [SerializeField] private Transform nodesContainer;
    [SerializeField] private DispatchWindow dispatchWindow;

    [Header("Spawn Settings")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<MissionSO> plannedMissions = new List<MissionSO>();

    private readonly List<Transform> _availableSpawnPoints = new List<Transform>();

    private void Start()
    {
        if (spawnPoints == null || spawnPoints.Count == 0 || plannedMissions == null) return;

        _availableSpawnPoints.AddRange(spawnPoints);

        foreach (var mission in plannedMissions)
        {
            if (mission != null)
            {
                StartCoroutine(SpawnMissionRoutine(mission));
            }
        }
    }

    private IEnumerator SpawnMissionRoutine(MissionSO mission)
    {
        if (mission.spawnDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(mission.spawnDelaySeconds);
        }

        Transform chosenPoint = GetRandomSpawnPoint();
        if (chosenPoint != null)
        {
            SpawnNode(mission, chosenPoint);
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        if (_availableSpawnPoints.Count > 0)
        {
            int index = Random.Range(0, _availableSpawnPoints.Count);
            Transform point = _availableSpawnPoints[index];
            _availableSpawnPoints.RemoveAt(index);
            return point;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    private void SpawnNode(MissionSO mission, Transform spawnPoint)
    {
        MissionNode newNode = Instantiate(nodePrefab, nodesContainer);
        newNode.transform.position = spawnPoint.position;
        newNode.Initialize(mission, spawnPoint, HandleNodeClicked, FreeUpSpawnPoint);
    }

    private void HandleNodeClicked(MissionNode node)
    {
        if (dispatchWindow != null)
        {
            dispatchWindow.OpenWindow(node.Mission, node);
        }
    }

    private void FreeUpSpawnPoint(Transform pointToFree)
    {
        if (pointToFree != null && !_availableSpawnPoints.Contains(pointToFree))
        {
            _availableSpawnPoints.Add(pointToFree);
        }
    }
}