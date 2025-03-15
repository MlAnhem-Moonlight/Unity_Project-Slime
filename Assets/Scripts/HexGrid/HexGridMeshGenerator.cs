using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]

public class HexGridMeshGenerator : MonoBehaviour
{
    public LayerMask gridLayer;
    public HexGrid hexGrid;

    private void Awake()
    {
        if(hexGrid == null)
        {
            hexGrid = GetComponentInParent<HexGrid>();
        }
        if(hexGrid == null)
        {
            Debug.LogError("HexGridMeshGenerator: No HexGrid found in parent");
        }
    }

    public void CreateHexMesh()
    {
        CreateHexMesh(hexGrid.hexWidth, hexGrid.hexHeight, hexGrid.hexSize, hexGrid.orientation, gridLayer);
    }

    public void CreateHexMesh(HexGrid hexGrid, LayerMask layer)
    {
        this.hexGrid = hexGrid;
        this.gridLayer = layer;
        CreateHexMesh(hexGrid.hexWidth, hexGrid.hexHeight, hexGrid.hexSize, hexGrid.orientation, gridLayer);
    }
    public void ClearHex()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
        {
            return;
        }
        GetComponent<MeshFilter>().sharedMesh.Clear();
        GetComponent<MeshCollider>().sharedMesh.Clear();
    }
    public void CreateHexMesh(int width, int height, float size, HexOrientation orientation, LayerMask layer)
    {
        ClearHex();
        Vector3[] vertices = new Vector3[width * height * 7];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(size, x, y, orientation);
                vertices[(y * width + x) * 7] = centrePosition;
                for (int s = 0; s < HexMatrix.Corners(orientation, size).Length; s++)
                {
                    vertices[(y * width + x) * 7 + 1 + s] = centrePosition + HexMatrix.Corners(orientation, size)[s % 6];
                }
            }
        }

        int[] triangles = new int[width * height * 18];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int s = 0; s < HexMatrix.Corners(orientation, size).Length; s++)
                {
                    int cornerIndex = s + 2 > 6 ? s + 2 - 6 : s + 2;
                    // Adjust the winding order to counter-clockwise
                    triangles[3 * 6 * (z * width + x) + s * 3 + 0] = (z * width + x) * 7;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 2] = (z * width + x) * 7 + s + 1;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 1] = (z * width + x) * 7 + cornerIndex;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Hex Mesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateUVDistributionMetrics();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        int gridLayerIndex = GetLayerIndex(layer);
        Debug.Log("Layer Index: " + gridLayerIndex);

        gameObject.layer = gridLayerIndex;
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerMaskValue = layerMask.value;
        Debug.Log("Layer Mask Value: " + layerMaskValue);
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & layerMaskValue) != 0)
            {
                Debug.Log("Layer Index Loop: " + i);
                return i;
            }
        }
        return 0;
    }

}
