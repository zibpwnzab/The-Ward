using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RectTransform))]
public class MissionBall : MonoBehaviour
{
    private const float StopSpeedThreshold = 10f;
    private const float WallPadding = 2f;
    private const float BounceJitter = 50f;

    [Header("Physics Settings")]
    [SerializeField] private float initialSpeed = 500f;
    [SerializeField] private float friction = 200f;
    [SerializeField] private float bounceForce = 0.8f;

    private RectTransform _rect;
    private Vector2 _velocity;
    private bool _isMoving;

    private UI_RadarChart _missionChart;
    private UI_RadarChart _teamChart;
    private Action<bool> _onResult;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void Launch(UI_RadarChart mission, UI_RadarChart team, Action<bool> callback)
    {
        _missionChart = mission;
        _teamChart = team;
        _onResult = callback;

        _rect.anchoredPosition = Vector2.zero;

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        _velocity = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * initialSpeed;
        _isMoving = true;
    }

    private void Update()
    {
        if (!_isMoving) return;

        float dt = Time.unscaledDeltaTime;
        _rect.anchoredPosition += _velocity * dt;

        if (_velocity.magnitude > 0f)
        {
            _velocity -= _velocity.normalized * (friction * dt);

            if (_velocity.magnitude < StopSpeedThreshold)
            {
                StopAndCheckResult();
                return;
            }
        }

        CheckWallCollision();
    }

    private void CheckWallCollision()
    {
        if (_missionChart == null) return;

        Vector2 pos = _rect.anchoredPosition;
        float dist = pos.magnitude;
        if (dist < 1f) return;

        float angle = GetNormalizedAngle(pos);
        float wallDist = Mathf.Max(_missionChart.GetRadiusAtAngle(angle), WallPadding * 2f);

        if (dist > wallDist)
        {
            _rect.anchoredPosition = pos.normalized * (wallDist - WallPadding);
            Vector2 normal = -pos.normalized;
            _velocity = Vector2.Reflect(_velocity, normal) * bounceForce;
            _velocity += Random.insideUnitCircle * BounceJitter;
        }
    }

    private void StopAndCheckResult()
    {
        _isMoving = false;
        _velocity = Vector2.zero;

        if (_teamChart == null)
        {
            _onResult?.Invoke(false);
            return;
        }

        Vector2 pos = _rect.anchoredPosition;
        float angle = GetNormalizedAngle(pos);
        float teamRadius = _teamChart.GetRadiusAtAngle(angle);
        bool isWin = pos.magnitude <= teamRadius;

        _onResult?.Invoke(isWin);
    }

    private static float GetNormalizedAngle(Vector2 position)
    {
        float angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        return angle < 0f ? angle + 360f : angle;
    }
}