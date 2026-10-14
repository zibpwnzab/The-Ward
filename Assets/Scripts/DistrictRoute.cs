using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DistrictRoute : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Color gizmoColor = Color.yellow;

    private void OnValidate()
    {
        BakeLineFromChildren();
    }

    private void OnTransformChildrenChanged()
    {
        BakeLineFromChildren();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying && transform.hasChanged)
        {
            BakeLineFromChildren();
            transform.hasChanged = false;
        }
    }
#endif

    [ContextMenu("Bake Route")]
    public void BakeLineFromChildren()
    {
        if (lineRenderer == null && !TryGetComponent(out lineRenderer))
        {
            return;
        }

        int childCount = transform.childCount;
        lineRenderer.positionCount = childCount;

        for (int i = 0; i < childCount; i++)
        {
            lineRenderer.SetPosition(i, transform.GetChild(i).position);
        }
    }

    public List<Vector3> GetWaypoints()
    {
        var points = new List<Vector3>(transform.childCount);
        foreach (Transform child in transform)
        {
            points.Add(child.position);
        }
        return points;
    }

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count < 2) return;

        Gizmos.color = gizmoColor;
        for (int i = 0; i < count - 1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
    }
}