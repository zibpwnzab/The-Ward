using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UI_RadarChart : Graphic
{
    [Header("Settings")]
    public float thickness = 0f;
    public float chartSize = 100f;

    // === НОВОЕ: Прогресс анимации (0 = точка, 1 = полный размер) ===
    [Range(0, 1)] public float animationProgress = 1f;

    private float[] _stats;

    public float GetRadiusAtAngle(float angleDegrees)
    {
        if (_stats == null || _stats.Length < 3) return 0;

        angleDegrees = Mathf.Repeat(angleDegrees - 90, 360);
        float step = 360f / _stats.Length;
        int index = Mathf.FloorToInt(angleDegrees / step);
        int nextIndex = (index + 1) % _stats.Length;
        float t = (angleDegrees % step) / step;

        float r1 = _stats[index];
        float r2 = _stats[nextIndex];

        // Умножаем на animationProgress, чтобы физика мяча тоже учитывала анимацию (если вдруг мяч полетит раньше)
        return Mathf.Lerp(r1, r2, t) * chartSize * animationProgress;
    }

    public void SetStats(float[] normalizedStats)
    {
        _stats = normalizedStats;
        SetVerticesDirty();
    }

    // Метод для управления анимацией извне
    public void SetAnimationProgress(float progress)
    {
        animationProgress = progress;
        SetVerticesDirty(); // Принудительно перерисовать
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_stats == null || _stats.Length < 3) return;

        int amount = _stats.Length;
        float angleStep = 360f / amount;

        vh.AddVert(Vector3.zero, color, Vector2.zero);

        for (int i = 0; i < amount; i++)
        {
            float statValue = _stats[i];
            float angle = (i * angleStep + 90) * Mathf.Deg2Rad;

            // === ВОТ ЗДЕСЬ МАГИЯ АНИМАЦИИ ===
            // Умножаем позицию на animationProgress
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * chartSize * statValue * animationProgress;

            vh.AddVert(pos, color, Vector2.zero);
        }

        for (int i = 0; i < amount; i++)
        {
            int nextIndex = (i + 1) % amount;
            vh.AddTriangle(0, i + 1, nextIndex + 1);
        }
    }
}