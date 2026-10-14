using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject emergencyPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Video Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    public void OnCampaignClicked()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void OnEmergencyClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (emergencyPanel != null) emergencyPanel.SetActive(true);
    }

    public void OnBackToMain()
    {
        if (emergencyPanel != null) emergencyPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void PlayTopic(VideoClip clip, string text)
    {
        if (videoPanel != null) videoPanel.SetActive(true);
        if (descriptionText != null) descriptionText.text = text;

        if (videoPlayer != null && clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.Play();
        }
    }

    public void CloseVideo()
    {
        if (videoPlayer != null) videoPlayer.Stop();
        if (videoPanel != null) videoPanel.SetActive(false);
    }
}