using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class JelliMetaSolidGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Color32 c = color;

        UIVertex v = UIVertex.simpleVert;
        v.color = c;

        v.position = new Vector3(rect.xMin, rect.yMin);
        vh.AddVert(v);

        v.position = new Vector3(rect.xMin, rect.yMax);
        vh.AddVert(v);

        v.position = new Vector3(rect.xMax, rect.yMax);
        vh.AddVert(v);

        v.position = new Vector3(rect.xMax, rect.yMin);
        vh.AddVert(v);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
