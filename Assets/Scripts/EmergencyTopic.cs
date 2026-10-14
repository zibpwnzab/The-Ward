using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(Button))]
public class EmergencyTopic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuManager menuManager;

    [Header("Content")]
    [SerializeField] private VideoClip videoClip;
    [TextArea(5, 10)]
    [SerializeField] private string instructionText;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);

        if (menuManager == null)
        {
            menuManager = GetComponentInParent<MainMenuManager>();
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }

    public void OnClick()
    {
        if (menuManager == null)
        {
            menuManager = FindFirstObjectByType<MainMenuManager>();
        }

        if (menuManager != null)
        {
            menuManager.PlayTopic(videoClip, instructionText);
        }
    }
}