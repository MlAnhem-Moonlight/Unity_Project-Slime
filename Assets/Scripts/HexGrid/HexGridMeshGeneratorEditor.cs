using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGridMeshGenerator))]
public class HexGridMeshGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        // Lấy script gốc
        HexGridMeshGenerator generator = (HexGridMeshGenerator)target;

        // Hiển thị các field mặc định trong Inspector
        DrawDefaultInspector();

        // Thêm nút "Generate Hex Mesh"
        if (GUILayout.Button("Generate Hex Mesh"))
        {
            generator.CreateHexMesh();
        }

        // Thêm nút "Clear Hex Mesh"
        if (GUILayout.Button("Clear Hex Mesh"))
        {
            generator.ClearHex();
        }
    }
}


