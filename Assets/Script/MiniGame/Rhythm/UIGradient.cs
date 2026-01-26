using UnityEngine;
using UnityEngine.UI;

// 이 스크립트는 UI Image에 그라데이션 효과를 추가합니다.
[AddComponentMenu("UI/Effects/UI Gradient")]
[RequireComponent(typeof(Image))]
public class UIGradient : BaseMeshEffect
{
    public Color topColor = Color.white;    // 위쪽 색상
    public Color bottomColor = Color.black; // 아래쪽 색상

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int count = vh.currentVertCount;
        if (count == 0) return;

        UIVertex vertex = new UIVertex();
        float bottomY = float.MaxValue;
        float topY = float.MinValue;

        // 1. 전체 높이 범위 계산
        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            bottomY = Mathf.Min(bottomY, vertex.position.y);
            topY = Mathf.Max(topY, vertex.position.y);
        }

        float uiHeight = topY - bottomY;

        // 2. 각 정점의 위치에 따라 색상 혼합 (Lerp)
        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // 현재 정점의 Y 위치가 전체 높이에서 몇 % 지점인지 계산 (0~1)
            float normalizedY = (vertex.position.y - bottomY) / uiHeight;

            // 기존 Image 색상과 곱해서 최종 색상 결정
            Color originalColor = GetComponent<Graphic>().color;
            vertex.color = Color.Lerp(bottomColor, topColor, normalizedY) * originalColor;

            vh.SetUIVertex(vertex, i);
        }
    }
}