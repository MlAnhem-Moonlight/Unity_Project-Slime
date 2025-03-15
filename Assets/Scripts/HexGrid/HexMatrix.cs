using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMatrix
{
    public static float OuterRadius(float hexSize)
    {
        return hexSize;
    }

    public static float InnerRadius(float hexSize)
    {
        return (Mathf.Sqrt(3) / 2) * hexSize;
    }

    public static Vector3[] Corners(HexOrientation orientation, float hexSize)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            corners[i] = Corner(hexSize, orientation, i);
        }
        return corners;
    }

    public static Vector3 Corner(float hexSize, HexOrientation orientation, int i)
    {
        float angle = 60 * i;
        if (orientation == HexOrientation.PointyTop)
        {
            angle += 30f;
        }
        Vector3 corner = new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad),
                            hexSize * Mathf.Sin(angle * Mathf.Deg2Rad),
                            0f);
        return corner;
    }

    public static Vector3 Center(float hexSize, int x, int y, HexOrientation orientation)
    {
        Vector3 centrePosition; // Biến lưu trữ tọa độ trung tâm
        if (orientation == HexOrientation.PointyTop)
        {
            // Trường hợp lục giác có đỉnh nhọn hướng lên
            centrePosition.x = (x + y * 0.5f - y / 2) * (InnerRadius(hexSize) * 2f);
            centrePosition.y = y * (OuterRadius(hexSize) * 1.5f);
            centrePosition.z = 0f; // Luôn cố định trên mặt phẳng 2D
        }
        else
        {
            // Trường hợp lục giác có mặt phẳng hướng lên
            centrePosition.x = (x) * (OuterRadius(hexSize) * 1.5f);
            centrePosition.y = (x + x * 0.5f - x / 2) * (InnerRadius(hexSize) * 2f);
            centrePosition.z = 0f; // Cố định tọa độ trục Z
        }
        return centrePosition; // Trả về tọa độ trung tâm
    }

    public static Vector3 OffsetToCube(int col, int row, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return AxialToCube(OffsetToAxialPointy(col, row));
        }
        else
        {
            return AxialToCube(OffsetToAxialFlat(col, row));
        }
    }
    public static Vector3 AxialToCube(Vector2Int axial)
    {
        float x = axial.x;
        float z = axial.y;
        float y = -x - z;
        return new Vector3(x, z, y);
    }
    public static Vector2Int OffsetToAxialFlat(int col, int row)
    {
        int q = col;
        int r = row - (col + (col & 1)) / 2;
        return new Vector2Int(q, r);
    }

    public static Vector2Int OffsetToAxialPointy(int col, int row)
    {
        int q = col - (row + (row & 1)) / 2;
        int r = row;
        return new Vector2Int(q, r);
    }


}
