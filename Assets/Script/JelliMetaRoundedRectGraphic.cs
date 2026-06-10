using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class JelliMetaRoundedRectGraphic : MaskableGraphic
{
    [Range(0f, 240f)]
    public float radius = 32f;

    [Range(2, 16)]
    public int cornerSegments = 8;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        if (rect.width <= 0f || rect.height <= 0f)
            return;

        float r = Mathf.Min(radius, rect.width * 0.5f, rect.height * 0.5f);
        int seg = Mathf.Max(2, cornerSegments);

        Vector2 center = rect.center;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = center;
        vh.AddVert(vertex);

        List<Vector2> points = new List<Vector2>(seg * 4 + 4);
        AddCorner(points, new Vector2(rect.xMax - r, rect.yMax - r), r, 90f, 0f, seg);
        AddCorner(points, new Vector2(rect.xMax - r, rect.yMin + r), r, 0f, -90f, seg);
        AddCorner(points, new Vector2(rect.xMin + r, rect.yMin + r), r, -90f, -180f, seg);
        AddCorner(points, new Vector2(rect.xMin + r, rect.yMax - r), r, -180f, -270f, seg);

        for (int i = 0; i < points.Count; i++)
        {
            vertex.position = points[i];
            vh.AddVert(vertex);
        }

        for (int i = 1; i <= points.Count; i++)
        {
            int next = i == points.Count ? 1 : i + 1;
            vh.AddTriangle(0, i, next);
        }
    }

    private void AddCorner(List<Vector2> points, Vector2 center, float r, float startDegrees, float endDegrees, int segments)
    {
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(startDegrees, endDegrees, t) * Mathf.Deg2Rad;
            points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
