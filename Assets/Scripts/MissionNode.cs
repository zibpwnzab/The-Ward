using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MissionNode : MonoBehaviour
{
    private MissionSO _mission;
    private Transform _spawnPoint;
    private Action<MissionNode> _onNodeClicked;
    private Action<Transform> _onNodeReleased;
    private Button _button;

    public MissionSO Mission => _mission;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleClick);
        }
    }

    public void Initialize(
        MissionSO mission,
        Transform spawnPoint,
        Action<MissionNode> onNodeClicked,
        Action<Transform> onNodeReleased)
    {
        _mission = mission;
        _spawnPoint = spawnPoint;
        _onNodeClicked = onNodeClicked;
        _onNodeReleased = onNodeReleased;

        transform.localScale = Vector3.one;
        Vector3 localPos = transform.localPosition;
        localPos.z = 0f;
        transform.localPosition = localPos;
    }

    public void SetInteractable(bool interactable)
    {
        if (_button != null) _button.interactable = interactable;
    }

    public void CompleteAndDestroy()
    {
        _onNodeReleased?.Invoke(_spawnPoint);
        Destroy(gameObject);
    }

    private void HandleClick()
    {
        _onNodeClicked?.Invoke(this);
    }
}