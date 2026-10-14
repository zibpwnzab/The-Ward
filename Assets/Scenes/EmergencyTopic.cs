using UnityEngine;
using UnityEngine.Video;

public class EmergencyTopic : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private VideoClip videoClip; // Перетащи сюда видео файл
    [TextArea(5, 10)]
    [SerializeField] private string instructionText; // Напиши тут текст

    private MainMenuManager _manager;

    private void Start()
    {
        // Ищем главного босса (менеджер меню)
        _manager = FindObjectOfType<MainMenuManager>();
    }

    // Эту функцию привяжи к кнопке OnClick()
    public void OnClick()
    {
        if (_manager != null)
        {
            _manager.PlayTopic(videoClip, instructionText);
        }
        else
        {
            Debug.LogError("Где MainMenuManager? Я его не вижу!");
        }
    }
}