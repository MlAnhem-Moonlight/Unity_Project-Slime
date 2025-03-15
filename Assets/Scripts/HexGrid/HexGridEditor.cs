using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGrid))]
public class HexEditor : Editor
{
    void OnSceneGUI()
    {
        HexGrid hexGrid = (HexGrid)target;

        for (int y = 0; y < hexGrid.hexHeight; y++)
        {
            for (int x = 0; x < hexGrid.hexWidth; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(hexGrid.hexSize, x, y, hexGrid.orientation) + hexGrid.transform.position;

                int centerX = x;
                int centerZ = y;


                // Hiển thị tọa độ cube
                Vector3 cubeCoord = HexMatrix.OffsetToCube(centerX, centerZ, hexGrid.orientation);
                // Hiển thị tọa độ offset
                Handles.Label(centrePosition + Vector3.forward * 0.5f, $"[{centerX}, {centerZ}]");
                Handles.Label(centrePosition, $"{cubeCoord.x}, {cubeCoord.y}, {cubeCoord.z}");


            }
        }
    }
}