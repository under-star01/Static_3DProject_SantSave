using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerCtrl : MonoBehaviour
{
    public float moveSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        // W: 앞
        if (Input.GetKey(KeyCode.W))
        {
            moveDir += Vector3.forward;
        }
        // S: 뒤
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += Vector3.back;
        }
        // A: 왼쪽
        if (Input.GetKey(KeyCode.A))
        {
            moveDir += Vector3.left;
        }
        // D: 오른쪽
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += Vector3.right;
        }

        transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
    }
}
