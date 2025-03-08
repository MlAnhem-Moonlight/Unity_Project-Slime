using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTowardsMouse : MonoBehaviour
{
    public float jumpForce = 10f; // Lực nhảy
    private Rigidbody2D rb;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main; // Lấy camera chính
    }

    void Update()
    {
        // Kiểm tra nếu nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            Jump();
        }
    }

    void Jump()
    {
        // Lấy vị trí chuột trong không gian thế giới
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Đảm bảo vị trí z là 0

        // Tính vector hướng từ nhân vật đến vị trí chuột
        Vector2 direction = (mousePosition - transform.position).normalized;

        // Áp dụng lực nhảy
        rb.velocity = direction * jumpForce;
    }
}
