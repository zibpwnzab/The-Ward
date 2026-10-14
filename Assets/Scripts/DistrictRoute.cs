using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways] // Магия: скрипт работает прямо в редакторе!
public class DistrictRoute : MonoBehaviour
{
    [Tooltip("Кинь сюда LineRenderer, чтобы скрипт сам его заполнил")]
    [SerializeField] private LineRenderer lineRenderer;

    [Tooltip("Цвет линии в редакторе")]
    [SerializeField] private Color debugColor = Color.yellow;

    // В Unity редакторе: Когда ты что-то меняешь, линия перерисуется сама
    private void Update()
    {
        if (!Application.isPlaying) // Только в режиме редактирования
        {
            BakeLineFromChildren();
        }
    }

    // Кнопка для ручного обновления (на всякий случай)
    [ContextMenu("Bake Route")]
    public void BakeLineFromChildren()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        // Берем всех детей этого объекта
        int childCount = transform.childCount;
        lineRenderer.positionCount = childCount;

        for (int i = 0; i < childCount; i++)
        {
            // Ставим точку линии туда, где стоит ребенок
            lineRenderer.SetPosition(i, transform.GetChild(i).position);
        }
    }

    // Этот метод вызывает SquadController в игре
    public List<Vector3> GetWaypoints()
    {
        List<Vector3> points = new List<Vector3>();
        foreach (Transform child in transform)
        {
            points.Add(child.position);
        }
        return points;
    }

    // Рисуем линии в сцене, чтобы ты видел связь даже без LineRenderer
    private void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
            }
        }
    }
}