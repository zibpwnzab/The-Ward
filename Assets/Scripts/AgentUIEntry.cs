using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AgentUIEntry : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsSummaryText;
    [SerializeField] private GameObject fatigueBarContainer;
    [SerializeField] private Image fatigueBarFill;

    private CanvasGroup _canvasGroup;
    private Transform _originalParent;
    private Canvas _mainCanvas;
    private RectTransform _rectTransform;

    // Сюда мы запомним, какой высоты должен быть элемент в списке
    private float _defaultHeight = 120f;

    public Agent LinkedAgent { get; private set; }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _mainCanvas = GetComponentInParent<Canvas>();

        // Пытаемся найти LayoutElement и взять высоту оттуда
        var layoutElem = GetComponent<LayoutElement>();
        if (layoutElem != null && layoutElem.minHeight > 0)
            _defaultHeight = layoutElem.minHeight;
    }

    public void Setup(Agent agent)
    {
        LinkedAgent = agent;
        SetSlotMode(false);
        RefreshVisuals();
    }

    // === ГЛАВНЫЙ МЕТОД ПРЕВРАЩЕНИЯ ===
    public void SetSlotMode(bool isInSlot)
    {
        // 1. Включаем/выключаем текстовые поля
        if (nameText != null) nameText.gameObject.SetActive(!isInSlot);
        if (fatigueBarContainer != null) fatigueBarContainer.SetActive(!isInSlot);

        if (isInSlot)
        {
            // === РЕЖИМ СЛОТА (Растянуться) ===
            ResetRectToFillParent(_rectTransform);
            if (portraitImage != null) ResetRectToFillParent(portraitImage.rectTransform);

            // Убедимся, что статы поверх картинки
            if (statsSummaryText != null) statsSummaryText.transform.SetAsLastSibling();
        }
        else
        {
            // === РЕЖИМ СПИСКА (Вернуться в норму) ===

            // Сброс масштаба (ВАЖНО! Иначе может стать гигантским)
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;

            // Якоря в левый верхний угол (стандарт для Vertical Layout)
            _rectTransform.anchorMin = new Vector2(0, 1);
            _rectTransform.anchorMax = new Vector2(0, 1);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Размер: Ширину ставим 0 (пусть LayoutGroup сам растянет), Высоту - фиксируем
            _rectTransform.sizeDelta = new Vector2(0, _defaultHeight);

            // Портрет: возвращаем в квадрат слева
            if (portraitImage != null)
            {
                RectTransform prt = portraitImage.rectTransform;
                prt.localScale = Vector3.one;
                prt.anchorMin = new Vector2(0, 0.5f); // Слева, Центр по Y
                prt.anchorMax = new Vector2(0, 0.5f);
                prt.pivot = new Vector2(0, 0.5f);
                prt.sizeDelta = new Vector2(100, 100); // 100x100 пикселей
                prt.anchoredPosition = new Vector2(60, 0); // Отступ слева (чуть больше половины ширины)
            }
        }
    }

    // Утилита для растягивания на весь родительский объект
    private void ResetRectToFillParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one; // Сброс масштаба
        rt.localRotation = Quaternion.identity;
    }

    // --- DRAG & DROP ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (LinkedAgent == null) return;

        MissionSlot oldSlot = transform.parent.GetComponent<MissionSlot>();
        if (oldSlot != null)
        {
            oldSlot.OnAgentRemoved();
            // Если тащим ИЗ слота, _originalParent уже должен быть ссылкой на список.
            // Если она потерялась, найдем её в ReturnToRoster
        }
        else
        {
            // Если тащим ИЗ списка, запоминаем его
            _originalParent = transform.parent;
        }

        // Выносим на главный канвас
        transform.SetParent(_mainCanvas.transform, true);

        // Включаем вид "Списка" (чтобы видеть имя при перетаскивании)
        SetSlotMode(false);

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_mainCanvas != null)
            _rectTransform.anchoredPosition += eventData.delta / _mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        // Если родитель всё еще канвас (значит, не попали в слот) -> Домой!
        if (transform.parent == _mainCanvas.transform)
        {
            ReturnToRoster();
        }
    }

    public void ReturnToRoster()
    {
        // Аварийный поиск списка, если ссылка потерялась
        if (_originalParent == null)
        {
            GameObject contentObj = GameObject.Find("Content"); // Имя твоего объекта в ScrollView
            if (contentObj != null) _originalParent = contentObj.transform;
        }

        if (_originalParent != null)
        {
            transform.SetParent(_originalParent);

            // ВАЖНО: Сначала возвращаем вид
            SetSlotMode(false);

            // Потом пинаем Unity пересчитать список
            LayoutRebuilder.ForceRebuildLayoutImmediate(_originalParent as RectTransform);
        }
        else
        {
            Debug.LogError("Некуда возвращать агента! Уничтожаю.");
            Destroy(gameObject);
        }
    }

    private void RefreshVisuals()
    {
        if (LinkedAgent == null) return;
        if (nameText != null) nameText.text = LinkedAgent.Name;
        if (portraitImage != null && LinkedAgent.Portrait != null) portraitImage.sprite = LinkedAgent.Portrait;
        if (fatigueBarFill != null && fatigueBarFill.type == Image.Type.Filled) fatigueBarFill.fillAmount = (float)LinkedAgent.fatigue / 100f;
        if (statsSummaryText != null)
        {
            float score = LinkedAgent.CalculateEfficiencyScore();
            string colorHex = score > 5.0f ? "#FFFFFF" : "#AAAAAA";
            statsSummaryText.text = $"EFF: <color={colorHex}>{score}</color>";
        }
        UpdateStatusColor();
    }

    private void UpdateStatusColor()
    {
        if (nameText == null || LinkedAgent == null) return;
        if (LinkedAgent.fatigue >= 80) nameText.color = new Color(1f, 0.2f, 0.2f);
        else if (LinkedAgent.fatigue >= 50) nameText.color = new Color(1f, 0.6f, 0f);
        else nameText.color = Color.white;
    }
}