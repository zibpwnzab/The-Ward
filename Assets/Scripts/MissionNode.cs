using UnityEngine;
using UnityEngine.UI;

public class MissionNode : MonoBehaviour
{
    private MissionSO _mission;
    private DispatchWindow _dispatchWindow;
    private Button _button;

    public void Setup(MissionSO mission, DispatchWindow window)
    {
        _mission = mission;
        _dispatchWindow = window;

        // === ФИКС КРИВОГО СПАВНА ===
        // 1. Сбрасываем масштаб. Unity любит ставить Scale != 1 при спавне UI
        transform.localScale = Vector3.one;

        // 2. Обнуляем Z позицию, чтобы точка не висела "над" или "под" картой
        Vector3 localPos = transform.localPosition;
        localPos.z = 0;
        transform.localPosition = localPos;
        // ===========================

        _button = GetComponent<Button>();
        if (_button == null) _button = gameObject.AddComponent<Button>();

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnNodeClicked);
    }

    private void OnNodeClicked()
    {
        if (_dispatchWindow != null)
        {
            // Передаем ссылку на себя (this), чтобы Диспетчер знал, кого потом удалять
            _dispatchWindow.OpenWindow(_mission, this);
        }
        else
        {
            Debug.LogError("MissionNode: Нет ссылки на DispatchWindow! Проверь MapManager.");
        }
    }
}