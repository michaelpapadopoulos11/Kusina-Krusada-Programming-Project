using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SIDE { Left, Mid, Right }

public class Movement : MonoBehaviour
{
    private SIDE m_Side = SIDE.Mid;
    private Vector2 startTouch;
    float NewXPos = 0f;

    private bool SwipeLeft;
    private bool SwipeRight;
    private bool SwipeUp;  // For jumping

    public float XValue = 2;
    public float forwardSpeed = 5f;
    public float jumpForce = 8f;     // How strong the jump is
    public float gravity = -20f;     // Custom gravity
    public float laneSwitchSpeed = 5f;  // <-- New: controls how fast you move between lanes

    [SerializeField] private CharacterController m_char;

    private float verticalVelocity;  // Tracks up/down movement

    void Start()
    {
        m_char = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Reset swipes
        SwipeLeft = false;
        SwipeRight = false;
        SwipeUp = false;

        // Keyboard input
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            SwipeLeft = true;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            SwipeRight = true;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            SwipeUp = true;

        // Touch swipe
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                startTouch = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 endTouch = touch.position;
                Vector2 swipe = endTouch - startTouch;

                if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                {
                    if (swipe.x > 50f)
                        SwipeRight = true;
                    else if (swipe.x < -50f)
                        SwipeLeft = true;
                }
                else
                {
                    if (swipe.y > 50f)
                        SwipeUp = true;
                }
            }
        }

        // Lane switching
        if (SwipeLeft)
        {
            if (m_Side == SIDE.Mid)
            {
                NewXPos = -XValue;
                m_Side = SIDE.Left;
            }
            else if (m_Side == SIDE.Right)
            {
                NewXPos = 0;
                m_Side = SIDE.Mid;
            }
        }
        else if (SwipeRight)
        {
            if (m_Side == SIDE.Mid)
            {
                NewXPos = XValue;
                m_Side = SIDE.Right;
            }
            else if (m_Side == SIDE.Left)
            {
                NewXPos = 0;
                m_Side = SIDE.Mid;
            }
        }

        // Jumping
        if (m_char.isGrounded)
        {
            verticalVelocity = -1f;
            if (SwipeUp)
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Movement
        Vector3 move = Vector3.forward * forwardSpeed * Time.deltaTime;

        // Smoothly move to the lane position
        float targetX = Mathf.Lerp(transform.position.x, NewXPos, laneSwitchSpeed * Time.deltaTime);
        move += (targetX - transform.position.x) * Vector3.right;

        // Vertical (jumping + gravity)
        move.y = verticalVelocity * Time.deltaTime;

        m_char.Move(move);
    }
}