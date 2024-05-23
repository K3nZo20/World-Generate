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
    private Animator animator;
    private enum MovementState {Stay,GoRight,GoLeft,mineRight,mineLeft}
    private BoxCollider2D feet;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        feet = GetComponent<BoxCollider2D>();

    }

    void Update()
    {
        MovementState state;
        dirX = Input.GetAxisRaw("Horizontal");
        myRigidbody.velocity = new Vector2(dirX*MoveSpeed, myRigidbody.velocity.y);
        if (dirX > 0)
        {
                state = MovementState.GoRight;
        }
        else if (dirX < 0)
        {
            state = MovementState.GoLeft;
        }
        else
        {
            state = MovementState.Stay;
        }
        if (Input.GetButtonDown("Jump") && feet.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpForce);
        }
        if (Input.GetMouseButtonDown(0)) // Sprawdzamy czy gracz kliknął myszką
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Pobieramy pozycję kliknięcia myszką
                if (mousePosition.x > transform.position.x) // Jeśli kliknięcie jest po prawej stronie postaci
                {
                    state = MovementState.mineRight;
                }
                else
                {
                    state = MovementState.mineLeft;
                }
            }
        animator.SetInteger("state", (int)state);
    }
    public void MoveUp()
    {
        myRigidbody.transform.position = new Vector2(myRigidbody.transform.position.x, myRigidbody.transform.position.y + 4f); // Przesuń gracza o 10 jednostek w górę
        myRigidbody.simulated = true;
    }
}
