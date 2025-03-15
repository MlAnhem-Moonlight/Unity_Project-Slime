using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] public int hexWidth;
    [SerializeField] public int hexHeight;
    [SerializeField] public int hexSize;
    [SerializeField] public GameObject hexPrefab;
    [SerializeField] public HexOrientation orientation;     
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(hexSize, x, y, orientation) + transform.position;
                for (int s = 0; s < HexMatrix.Corners(orientation, hexSize).Length; s++)
                {
                    Gizmos.DrawLine(
                        centrePosition + HexMatrix.Corners(orientation, hexSize)[s % 6],
                        centrePosition + HexMatrix.Corners(orientation, hexSize)[(s + 1) % 6]
                    );
                }
            }
        }
    }


}

public enum HexOrientation
{
    PointyTop,
    FlatTop
}
