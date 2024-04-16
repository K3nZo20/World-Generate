using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Moving : MonoBehaviour
{
    private float dirX = 0f;
    private Rigidbody2D myRigidbody;
    public float MoveSpeed = 7f;
    public float jumpForce = 7f;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        myRigidbody.velocity = new Vector2(dirX*MoveSpeed, myRigidbody.velocity.y);
        if (Input.GetButtonDown("Jump"))
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpForce);
        }
    }

    public Vector3 getPosition()
    {
        return transform.position;
    }
}