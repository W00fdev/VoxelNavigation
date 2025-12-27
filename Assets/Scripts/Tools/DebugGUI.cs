using UnityEngine;

namespace Tools
{
    public class DebugGUI : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 200));

            if (GUILayout.Button("Показать границы SVO"))
                GizmosTools.ShowBoundaries = !GizmosTools.ShowBoundaries;

            if (GUILayout.Button("Показать ребра графа пути"))
                GizmosTools.ShowGraphEdges = !GizmosTools.ShowGraphEdges;

            if (GUILayout.Button("Показать вершины графа пути"))
                GizmosTools.ShowGraphNodes = !GizmosTools.ShowGraphNodes;

            GUILayout.Label($"Дальность отрисовки гизмо: {GizmosTools.DrawDistance:F2}");
            GizmosTools.DrawDistance = GUILayout.HorizontalSlider(GizmosTools.DrawDistance, 0f, 50000f);

            GUILayout.EndArea();
        }
    }
}
