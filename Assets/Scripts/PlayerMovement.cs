using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Rigidbody2D rb2D;
    public Camera cam;

    public Vector2 movement;
    public Vector2 mousePos;


    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    private void FixedUpdate()
    {
        rb2D.MovePosition(rb2D.position + movement *(moveSpeed * 10)* Time.fixedDeltaTime);

        Vector2 lookDir = mousePos - rb2D.position;
        float angleZ = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb2D.rotation = angleZ;
    }
}