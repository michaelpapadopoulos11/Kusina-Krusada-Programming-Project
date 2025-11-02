using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SIDE { Left, Mid, Right }

public class Movement : MonoBehaviour
{

    AudioManager audioManager;

    private SIDE m_Side = SIDE.Mid;
    private Vector2 startTouch;
    float NewXPos = 0f;

    private bool SwipeLeft;
    private bool SwipeRight;
    public bool SwipeUp { get; private set; }
    public bool SwipeDown { get; private set; }

    public float XValue = 2;
    public float forwardSpeed = 5f;
    // Store the base speed so we can restore after slow effects
    [HideInInspector] public float baseForwardSpeed;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float laneSwitchSpeed = 5f;

    public float crouchScale = 0.5f;       // How much to shrink visually
    public float crouchDuration = 1.0f;    // How long crouch lasts

    [SerializeField] private CharacterController m_char;

    private float verticalVelocity;
    private Vector3 originalScale;
    public bool IsCrouching { get; private set; } = false;
    public bool IsGrounded => m_char.isGrounded;
    private float crouchTimer = 0f;

    // Store original CharacterController settings
    private float originalHeight;
    private Vector3 originalCenter;

    // MIKE POWER UP
    public bool isInvincible = false;
    public static float invincibilityTimer = 0f;
    public float invincibilityDuration = 5f;
    public float invincibleAlpha = 0.5f;

    // Flicker settings: start flickering this many seconds before the end
    [Tooltip("Seconds before invincibility end when flicker starts (e.g. 2)")]
    public float flickerStart = 2f;
    // Flicker frequency range (Hz)
    public float flickerMinFreq = 4f;
    public float flickerMaxFreq = 12f;

    private Renderer[] _renderers;
    private Color[][] _originalColors;
    private bool _prevInvincible = false;

    public bool isSlowed = false;
    public static float slowTimer = 0f; //global var so buff icons can display
    public float slowDuration = 5f;
    private float slowedSpeed; // Store the slowed speed value

    public int pointsMultiplier = 1; // Multiplier for points collected

    public bool isDoublePoints = false; // Tracks if double points is active
    public static float doublePointsTimer = 0f; // Timer for double points effect
    public float doublePointsDuration = 5f; // Duration of double points effect

    // Simple game over state
    public bool isGameOver = false;

    private void Awake() {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start() {
        m_char = GetComponent<CharacterController>();
        originalScale = transform.localScale;

        originalHeight = m_char.height;
        originalCenter = m_char.center;
        isInvincible = false;
        baseForwardSpeed = forwardSpeed;
        isSlowed = false;
        isDoublePoints = false;
        isGameOver = false;
        
        // Reset the life system when the game starts
        LifeManager.ResetLives();

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

    void Update() {
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
                audioManager.playSFX(audioManager.switch_lanes, 0.3f);
                NewXPos = -XValue;
                m_Side = SIDE.Left;
            }
            else if (m_Side == SIDE.Right)
            {
                audioManager.playSFX(audioManager.switch_lanes, 0.3f);
                NewXPos = 0;
                m_Side = SIDE.Mid;
            }
        }
        else if (SwipeRight)
        {
            if (m_Side == SIDE.Mid)
            {
                audioManager.playSFX(audioManager.switch_lanes, 0.3f);
                NewXPos = XValue;
                m_Side = SIDE.Right;
            }
            else if (m_Side == SIDE.Left)
            {
                audioManager.playSFX(audioManager.switch_lanes, 0.3f);
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
                audioManager.playSFX(audioManager.jump, 0.3f);
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Crouching (only when grounded)
        if (m_char.isGrounded && SwipeDown && !IsCrouching)
        {
            // Shrink CharacterController
            m_char.height = originalHeight * crouchScale;
            m_char.center = new Vector3(originalCenter.x, originalCenter.y * crouchScale, originalCenter.z);

            IsCrouching = true;
            audioManager.playSFX(audioManager.slide, 0.3f);
            crouchTimer = crouchDuration;
        }

        if (IsCrouching)
        {
            crouchTimer -= Time.deltaTime;
            if (crouchTimer <= 0f)
            {
                // Reset model
                transform.localScale = originalScale;

                // Reset CharacterController
                m_char.height = originalHeight;
                m_char.center = originalCenter;

                IsCrouching = false;
            }
        }

        if (UIScore.gameIsPaused)
        {
            verticalVelocity = -1f;
            m_char.Move(Vector3.zero);
        }
        else if (!UIScore.gameIsPaused)
        {
             // Movement - optimized to use cached deltaTime
            Vector3 move = Vector3.forward * forwardSpeed * Time.deltaTime;

            float targetX = Mathf.Lerp(transform.position.x, NewXPos, laneSwitchSpeed * Time.deltaTime);
            move += (targetX - transform.position.x) * Vector3.right;

            move.y = verticalVelocity * Time.deltaTime;

            m_char.Move(move);
        }

        // Invincibility powerup handling
        if (isInvincible) {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f) {
                isInvincible = false;
                Invincibility_Powerup.invincibilityActive = false;
                invincibilityTimer = 0f;
                Debug.Log("Invincibility powerup expired");
            }
        }

        // If invincibility state changed, apply immediate visual (sets base alpha)
        if (isInvincible != _prevInvincible)
        {
            ApplyInvincibilityVisual(isInvincible);
            _prevInvincible = isInvincible;
        }

        // Update per-frame visuals (handles flicker when near expiry)
        if (isInvincible)
        {
            UpdateInvincibilityFlicker();
        }

        // Handle slow effect
        if (isSlowed) {
            // Use the stored slowed speed (set when powerup is picked up)
            forwardSpeed = slowedSpeed;
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) {
                isSlowed = false;
                Slowdown_Glove.slowActive = false;
                forwardSpeed = baseForwardSpeed; // Restore to current base speed
                Debug.Log("Slow effect worn off");
            }
        }

        // Handle double points powerup
        if (isDoublePoints) {
            pointsMultiplier = 2; // x2 fruit points value
            doublePointsTimer -= Time.deltaTime;
            if (doublePointsTimer <= 0f) {
                isDoublePoints = false;
                Double_Points.doublePointsActive = false; //used for setting UI icons
                pointsMultiplier = 1; // normal points per fruit collected
                Debug.Log("Double points powerup expired");
            }
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

    private void UpdateInvincibilityFlicker()
    {
        if (_renderers == null || _originalColors == null) return;

        float timeLeft = invincibilityTimer;
        if (timeLeft <= 0f) return;

        // If not yet in flicker window, ensure base invincible alpha is applied
        if (timeLeft > flickerStart)
        {
            // base alpha already applied by ApplyInvincibilityVisual when state changed
            return;
        }

        // Map timeLeft in [flickerStart..0] to frequency range [flickerMinFreq..flickerMaxFreq]
        float t = Mathf.Clamp01(1f - (timeLeft / flickerStart));
        float freq = Mathf.Lerp(flickerMinFreq, flickerMaxFreq, t);

        // Create a 0..1 pingpong value at 'freq' Hz
        float signal = Mathf.PingPong(Time.time * freq, 2f);

        // When signal > 0.5 show the invincibleAlpha, otherwise show fully visible (or original alpha)
        float targetAlpha = signal > 0.5f ? Mathf.Clamp01(invincibleAlpha) : 1f;

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            var mats = renderer.materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (!mats[j].HasProperty("_Color")) continue;

                Color baseCol = _originalColors[i][j];
                baseCol.a = targetAlpha;
                mats[j].color = baseCol;
            }
        }
    }

    /// <summary>
    /// Apply slowdown effect using current speed as reference
    /// Called by slowdown powerups
    /// </summary>
    public void ApplySlowdownEffect(float duration, float slowFactor = 0.5f)
    {
        isSlowed = true;
        slowTimer = duration;
        // Calculate slowed speed based on current base speed
        slowedSpeed = baseForwardSpeed * slowFactor;
        Debug.Log($"Slowdown applied: {baseForwardSpeed} -> {slowedSpeed}");
    }

    /// <summary>
    /// Trigger immediate game over when hitting obstacles
    /// </summary>
    public void TriggerGameOver()
    {
        Debug.Log($"TriggerGameOver called! isInvincible: {isInvincible}, isGameOver: {isGameOver}");
        if (isInvincible || isGameOver) 
        {
            Debug.Log("TriggerGameOver blocked - player is invincible or game already over");
            return; // Can't trigger game over if invincible or already game over
        }
        
        Debug.Log("Setting isGameOver = true and pausing game");
        isGameOver = true;
        UIScore.gameIsPaused = true;
        Debug.Log("Game Over - Player hit obstacle!");
    }
}