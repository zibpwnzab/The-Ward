using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject emergencyPanel;
    [SerializeField] private GameObject videoPanel;

    [Header("Video Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Settings")]
    [Tooltip("Точное имя сцены с игрой")]
    [SerializeField] private string gameSceneName = "GameScene";

    // === ГЛАВНОЕ МЕНЮ ===
    public void OnCampaignClicked()
    {
        // Грузим игру. Если имя неправильное, Unity ругнется в консоль.
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Настройки? Пф, в этой игре только хардкор, никаких настроек.");
        // Тут можно открыть панель настроек, если совесть проснется.
    }

    public void OnEmergencyClicked()
    {
        mainPanel.SetActive(false);
        emergencyPanel.SetActive(true);
    }

    // === ЭКСТРЕННОЕ МЕНЮ ===
    public void OnBackToMain()
    {
        emergencyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // Этот метод будут вызывать кнопки тем (Жгут, СЛР...)
    public void PlayTopic(VideoClip clip, string text)
    {
        videoPanel.SetActive(true);

        descriptionText.text = text;

        if (clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.Play();
        }
    }

    public void CloseVideo()
    {
        videoPlayer.Stop();
        videoPanel.SetActive(false);
    }
}