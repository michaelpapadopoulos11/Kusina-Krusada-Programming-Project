using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SIDE { Left, Mid, Right }

public class Movement : MonoBehaviour
{
    public SIDE m_Side = SIDE.Mid;
    float NewZPos = 0f;

    [HideInInspector] public bool SwipeLeft, SwipeRight, SwipeUp, SwipeDown;

    [Header("Movement Settings")]
    public float ZValue = 3f;       // Lane depth
    public float SpeedDodge = 7f;   // Lane switch speed

    [Header("Jump Settings")]
    public float JumpForce = 5f;    // Initial jump velocity
    public float Gravity = -20f;  // Gravity strength

    private float z; // Z position interpolation
    private float y; // Vertical velocity
    private float fixedX; // Keep X constant

    public bool InJump;

    [SerializeField] private UnityEngine.CharacterController m_char;

    void Start()
    {
        m_char = GetComponent<UnityEngine.CharacterController>();
        fixedX = transform.position.x; // Store starting X position
        NewZPos = transform.position.z;
    }

    void Update()
    {
        // Input detection
        SwipeLeft  = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        SwipeRight = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        SwipeUp    = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        SwipeDown  = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);

        // Handle lane switching along Z
        if (SwipeLeft) // Move towards negative Z
        {
            if (m_Side == SIDE.Mid)
            {
                NewZPos = -ZValue;
                m_Side = SIDE.Left;
            }
            else if (m_Side == SIDE.Right)
            {
                NewZPos = 0;
                m_Side = SIDE.Mid;
            }
        }
        else if (SwipeRight) // Move towards positive Z
        {
            if (m_Side == SIDE.Mid)
            {
                NewZPos = ZValue;
                m_Side = SIDE.Right;
            }
            else if (m_Side == SIDE.Left)
            {
                NewZPos = 0;
                m_Side = SIDE.Mid;
            }
        }

        // Smooth Z movement
        z = Mathf.Lerp(z, NewZPos, Time.deltaTime * SpeedDodge);
        m_char.Move((z - transform.position.z) * Vector3.forward);

        // Jump & gravity handling
        if (m_char.isGrounded)
        {
            if (InJump) InJump = false;
            if (y < 0) y = -1f;

            if (SwipeUp) Jump();
        }
        else
        {
            y += Gravity * Time.deltaTime;
        }

        // Move character: fixed X, variable Z, jump on Y
        Vector3 moveVector = new Vector3(0, y, z - transform.position.z);
        m_char.Move(moveVector * Time.deltaTime);

        // Force bean to stay on fixed X
        Vector3 pos = transform.position;
        pos.x = fixedX;
        transform.position = pos;
    }

    public void Jump()
    {
        if (!InJump && m_char.isGrounded)
        {
            InJump = true;
            y = JumpForce;
        }
    }
}