using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class AgentUIEntry : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float HighEfficiencyThreshold = 5.0f;
    private const int CriticalFatigueThreshold = 80;
    private const int WarningFatigueThreshold = 50;
    private const float DragAlpha = 0.6f;

    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsSummaryText;
    [SerializeField] private GameObject fatigueBarContainer;
    [SerializeField] private Image fatigueBarFill;

    private CanvasGroup _canvasGroup;
    private Transform _rosterContainer;
    private Canvas _mainCanvas;
    private RectTransform _rectTransform;
    private float _defaultHeight = 120f;

    private Vector2 _initialPortraitAnchorMin;
    private Vector2 _initialPortraitAnchorMax;
    private Vector2 _initialPortraitPivot;
    private Vector2 _initialPortraitSizeDelta;
    private Vector2 _initialPortraitAnchoredPos;

    public Agent LinkedAgent { get; private set; }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _mainCanvas = GetComponentInParent<Canvas>();

        if (TryGetComponent(out LayoutElement layoutElem) && layoutElem.minHeight > 0f)
        {
            _defaultHeight = layoutElem.minHeight;
        }

        if (portraitImage != null)
        {
            RectTransform prt = portraitImage.rectTransform;
            _initialPortraitAnchorMin = prt.anchorMin;
            _initialPortraitAnchorMax = prt.anchorMax;
            _initialPortraitPivot = prt.pivot;
            _initialPortraitSizeDelta = prt.sizeDelta;
            _initialPortraitAnchoredPos = prt.anchoredPosition;
        }
    }

    public void Initialize(Agent agent, Transform rosterContainer)
    {
        LinkedAgent = agent;
        _rosterContainer = rosterContainer;
        SetSlotMode(false);
        RefreshVisuals();
    }

    public void SetSlotMode(bool isInSlot)
    {
        if (nameText != null) nameText.gameObject.SetActive(!isInSlot);
        if (fatigueBarContainer != null) fatigueBarContainer.SetActive(!isInSlot);

        if (isInSlot)
        {
            StretchToFillParent(_rectTransform);
            if (portraitImage != null) StretchToFillParent(portraitImage.rectTransform);
            if (statsSummaryText != null) statsSummaryText.transform.SetAsLastSibling();
        }
        else
        {
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;
            _rectTransform.anchorMin = new Vector2(0f, 1f);
            _rectTransform.anchorMax = new Vector2(0f, 1f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.sizeDelta = new Vector2(0f, _defaultHeight);

            if (portraitImage != null)
            {
                RectTransform prt = portraitImage.rectTransform;
                prt.localScale = Vector3.one;
                prt.anchorMin = _initialPortraitAnchorMin;
                prt.anchorMax = _initialPortraitAnchorMax;
                prt.pivot = _initialPortraitPivot;
                prt.sizeDelta = _initialPortraitSizeDelta;
                prt.anchoredPosition = _initialPortraitAnchoredPos;
            }
        }
    }

    private void StretchToFillParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (LinkedAgent == null) return;

        if (_mainCanvas == null)
        {
            _mainCanvas = GetComponentInParent<Canvas>();
        }

        if (transform.parent != null && transform.parent.TryGetComponent(out MissionSlot oldSlot))
        {
            oldSlot.ClearSlot();
        }
        else if (_rosterContainer == null)
        {
            _rosterContainer = transform.parent;
        }

        if (_mainCanvas != null)
        {
            transform.SetParent(_mainCanvas.transform, true);
        }

        SetSlotMode(false);
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = DragAlpha;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_mainCanvas != null)
        {
            _rectTransform.anchoredPosition += eventData.delta / _mainCanvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        if (_mainCanvas != null && transform.parent == _mainCanvas.transform)
        {
            ReturnToRoster();
        }
    }

    public void ReturnToRoster()
    {
        if (_rosterContainer != null)
        {
            transform.SetParent(_rosterContainer, false);
            SetSlotMode(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rosterContainer as RectTransform);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void RefreshVisuals()
    {
        if (LinkedAgent == null) return;

        if (nameText != null) nameText.text = LinkedAgent.Name;
        if (portraitImage != null && LinkedAgent.Portrait != null) portraitImage.sprite = LinkedAgent.Portrait;

        if (fatigueBarFill != null && fatigueBarFill.type == Image.Type.Filled)
        {
            fatigueBarFill.fillAmount = (float)LinkedAgent.Fatigue / Agent.MaxFatigue;
        }

        if (statsSummaryText != null)
        {
            float score = LinkedAgent.CalculateEfficiencyScore();
            string colorHex = score > HighEfficiencyThreshold ? "#FFFFFF" : "#AAAAAA";
            statsSummaryText.text = $"EFF: <color={colorHex}>{score:0.0}</color>";
        }

        UpdateStatusColor();
    }

    private void UpdateStatusColor()
    {
        if (nameText == null || LinkedAgent == null) return;

        if (LinkedAgent.Fatigue >= CriticalFatigueThreshold)
            nameText.color = new Color(1f, 0.2f, 0.2f);
        else if (LinkedAgent.Fatigue >= WarningFatigueThreshold)
            nameText.color = new Color(1f, 0.6f, 0f);
        else
            nameText.color = Color.white;
    }
}