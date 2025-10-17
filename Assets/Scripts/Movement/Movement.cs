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
    private bool SwipeUp;
    private bool SwipeDown;

    public float XValue = 2;
    public float forwardSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float laneSwitchSpeed = 5f;

    public float crouchScale = 0.5f;       // How much to shrink visually
    public float crouchDuration = 1.0f;    // How long crouch lasts

    [SerializeField] private CharacterController m_char;

    private float verticalVelocity;
    private Vector3 originalScale;
    private bool isCrouching = false;
    private float crouchTimer = 0f;

    // Store original CharacterController settings
    private float originalHeight;
    private Vector3 originalCenter;

    // MIKE POWER UP
    public bool isInvincible = false;
    public float invincibilityTimer = 0f;
    public float invincibilityDuration = 5f;
    public float invincibleAlpha = 0.5f;

    private Renderer[] _renderers;
    private Color[][] _originalColors;
    private bool _prevInvincible = false;

    void Start()
    {
        m_char = GetComponent<CharacterController>();
        originalScale = transform.localScale;

        originalHeight = m_char.height;
        originalCenter = m_char.center;
        isInvincible = false;

        // Cache renderers and original colors for visual feedback
        _renderers = GetComponentsInChildren<Renderer>(true);
        _originalColors = new Color[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
        {
            var mats = _renderers[i].materials;
            _originalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty("_Color"))
                    _originalColors[i][j] = mats[j].color;
                else
                    _originalColors[i][j] = Color.white;
            }
        }
    }

    void Update()
    {
        // Reset swipes
        SwipeLeft = false;
        SwipeRight = false;
        SwipeUp = false;
        SwipeDown = false;

        // Keyboard input
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            SwipeLeft = true;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            SwipeRight = true;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            SwipeUp = true;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            SwipeDown = true;

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
                    else if (swipe.y < -50f)
                        SwipeDown = true;
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

        // Crouching (only when grounded)
        if (m_char.isGrounded && SwipeDown && !isCrouching)
        {
            // Shrink model
            transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);

            // Shrink CharacterController
            m_char.height = originalHeight * crouchScale;
            m_char.center = new Vector3(originalCenter.x, originalCenter.y * crouchScale, originalCenter.z);

            isCrouching = true;
            crouchTimer = crouchDuration;
        }

        if (isCrouching)
        {
            crouchTimer -= Time.deltaTime;
            if (crouchTimer <= 0f)
            {
                // Reset model
                transform.localScale = originalScale;

                // Reset CharacterController
                m_char.height = originalHeight;
                m_char.center = originalCenter;

                isCrouching = false;
            }
        }

        // Movement
        Vector3 move = Vector3.forward * forwardSpeed * Time.deltaTime;

        float targetX = Mathf.Lerp(transform.position.x, NewXPos, laneSwitchSpeed * Time.deltaTime);
        move += (targetX - transform.position.x) * Vector3.right;

        move.y = verticalVelocity * Time.deltaTime;

        m_char.Move(move);

        // Invincibility timer handling
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                invincibilityTimer = 0f;
                Debug.Log("Invincibility expired");
            }
        }

        if (isInvincible != _prevInvincible)
        {
            ApplyInvincibilityVisual(isInvincible);
            _prevInvincible = isInvincible;
        }
    }

    private void ApplyInvincibilityVisual(bool on)
    {
        if (_renderers == null || _originalColors == null) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            var mats = renderer.materials; // creates instances if needed
            for (int j = 0; j < mats.Length; j++)
            {
                if (!mats[j].HasProperty("_Color")) continue;

                Color col = _originalColors[i][j];
                if (on)
                {
                    col.a = Mathf.Clamp01(invincibleAlpha);
                }
                // when off, restore original alpha
                mats[j].color = col;
            }
        }
    }
}