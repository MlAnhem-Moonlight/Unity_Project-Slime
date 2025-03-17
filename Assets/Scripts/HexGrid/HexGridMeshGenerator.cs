using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]

public class HexGridMeshGenerator : MonoBehaviour
{
    public LayerMask gridLayer;
    public HexGrid hexGrid;
    public Material hexagonMaterial; // Reference to the hexagon material

    private void Awake()
    {
        if (hexGrid == null)
        {
            hexGrid = GetComponentInParent<HexGrid>();
        }
        if (hexGrid == null)
        {
            Debug.LogError("HexGridMeshGenerator: No HexGrid found in parent");
        }
        if (GetComponent<LineRenderer>() == null)
        {
            gameObject.AddComponent<LineRenderer>();
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
        List<Vector3> borderVertices = new List<Vector3>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(size, x, y, orientation);
                vertices[(y * width + x) * 7] = centrePosition;
                Vector3[] corners = HexMatrix.Corners(orientation, size);
                for (int s = 0; s < corners.Length; s++)
                {
                    Vector3 corner = centrePosition + corners[s];
                    vertices[(y * width + x) * 7 + 1 + s] = corner;
                    borderVertices.Add(corner);
                }
                // Close the loop for the hexagon border
                borderVertices.Add(centrePosition + corners[0]);

                // Create a hexagon GameObject and add the HexagonHover script
                GameObject hexagon = new GameObject($"Hexagon_{x}_{y}");
                hexagon.transform.position = centrePosition;
                hexagon.AddComponent<MeshFilter>().mesh = CreateHexagonMesh(corners);
                MeshRenderer renderer = hexagon.AddComponent<MeshRenderer>();
                renderer.material = hexagonMaterial; // Use the hexagon material
                hexagon.transform.SetParent(transform);
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

        // Configure LineRenderer for borders
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = borderVertices.Count;
            lineRenderer.SetPositions(borderVertices.ToArray());
        }
    }

    private Mesh CreateHexagonMesh(Vector3[] corners)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[7];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < 6; i++)
        {
            vertices[i + 1] = corners[i];
        }

        int[] triangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i == 5 ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerMaskValue = layerMask.value;
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & layerMaskValue) != 0)
            {
                return i;
            }
        }
        return 0;
    }
}
