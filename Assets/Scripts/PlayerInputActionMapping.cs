using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
public class PlayerInputActionMapping : MonoBehaviour
{
    public const string KeyboardControlScheme = "Keyboard&Mouse";

    public static readonly int AnimatorMovementSpeedKey = Animator.StringToHash("MovementSpeed");


    // Player movement values determined by player stats
    // These values are updated externally
    [HideInInspector] public float MovementSpeed = 1f;
    [HideInInspector] public float ProjectileRate = 1f;
    [HideInInspector] public float ProjectileSpeed = 1f;
    [HideInInspector] public float ProjectileRange = 1f;

    // Cache variables
    private Vector2 m_MoveCache;    // 2D vector from Input system for player movement
    private Vector2 m_LookCache;    // 2D vector from Input system for 

    private Vector3 m_LocalForward; // Normalized forward vector for player, relative to camera
    private Vector3 m_LocalLeft;    // Normalized left vector for player, relative to camera

    private Vector3 m_Target;       // Projectile target, probably need to separate movement and shooting
    private Plane m_TargetPlane;    // In-memory plane used for calculating raycasts from player in world space


    [Header("Projectile Settings")]
    [SerializeField] private bool m_AutoFire = false;
    private bool m_ActiveFire = false;
    private bool m_QueueProjectile = false; // Should a single projectile be fired?
    private bool m_MarkDirty = false;       // Should recalculate local direction vectors?
    private bool m_CanFire = true;          // Can a projectile be generated?
    private bool m_MouseControl = true;     // Indicates whether the mouse is providing input.
                                            // If false, inputs are coming from joystick/controller

    private float m_ProjectileElapsed;      // Time since last projectile

    [SerializeField] private CharacterController m_Character;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private ProjectilePool m_ProjectilePool;

    [Header("Debug Settings")]
    [SerializeField, Tooltip("Only shown in Scene view")] private bool m_ShowDebugRays = true;

    // Properties
    public Vector3 MovementVector { get; private set; }         // Converted 3D vector defining player movement

    private void OnValidate()
    {
        if (!m_Character) { m_Character = GetComponent<CharacterController>(); }

        // This will likely need to be modified when new projectile pools
        // for NPCs or networked clients are added
        if (!m_ProjectilePool) { m_ProjectilePool = FindAnyObjectByType<ProjectilePool>(); }
    }

    private void Start()
    {
        OnValidate();
        // Define the XZ raycast plane at the height of the player
        m_TargetPlane = new(Vector3.up, m_Character.center);
    }


    private void Update()
    {
        if (m_MarkDirty) { CalculateDirectionVectors(); }

        ApplyMovement();
        CheckProjectile();

        if (m_ShowDebugRays)
        {
            Debug.DrawRay(transform.position + m_Character.center, m_LocalForward, Color.red);
            Debug.DrawLine(gameObject.transform.position, m_Target, Color.blue);
        }
    }

    public void CheckProjectile()
    {
        if (m_CanFire && (m_ActiveFire || m_QueueProjectile))
        {
            LaunchProjectile();
            m_ProjectileElapsed = 0f;
            m_QueueProjectile = false;
            m_CanFire = false;
            return;
        }

        m_ProjectileElapsed += Time.deltaTime;

        if (m_ProjectileElapsed >= (1 / ProjectileRate))
        {
            m_CanFire = true;
        }
    }

    private Vector3 GetProjectileDirection(Vector3 start)
    {
        Vector3 dir;

        if (m_MouseControl)
        {
            Vector3 pos = Vector3.zero;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (m_TargetPlane.Raycast(ray, out float distance))
            {
                pos = ray.GetPoint(distance);
            }
            dir = Vector3.Normalize(pos - start);
        }
        else
        {
            // LocalForward * LookCache.y defines forward direction
            // -LocalLeft * LookCache.x defines the sideways direction
            dir = Vector3.Normalize(m_LocalForward * m_LookCache.y
                - m_LocalLeft * m_LookCache.x);
        }

        return dir;
    }

    /// <summary>
    ///     Casts a ray from the mouse position onto the XZ plane passing through the player
    /// </summary>
    public void LaunchProjectile()
    {
        // Initial projectile position, based on character
        Vector3 start = transform.position + m_Character.center;

        Vector3 dir = GetProjectileDirection(start);

        // Projectile destination, based on cursor direction and projectile range
        Vector3 end = start + dir * ProjectileRange;

        // debugging
        m_Target = end;

        m_ProjectilePool
            .Projectiles.Get()
            .SetTrajectory(start, end, ProjectileSpeed)
            .Activate();
    }

    private void ApplyMovement()
    {
        m_Character.Move(MovementSpeed * Time.deltaTime * MovementVector);
        if (MovementVector != Vector3.zero) { gameObject.transform.forward = MovementVector; }

        // animate movement
        m_Animator.SetFloat(AnimatorMovementSpeedKey, MovementVector.magnitude);
    }

    /// <summary>
    ///     Calculates normalized 'Forward' and 'Left' vectors based on <see cref="Camera.main"/>
    /// </summary>
    private void CalculateDirectionVectors()
    {
        // Calculate forward
        m_LocalForward = gameObject.transform.position - Camera.main.transform.position;
        m_LocalForward = Vector3.Normalize(new Vector3(m_LocalForward.x, 0, m_LocalForward.z));
        m_LocalLeft = Vector3.Cross(m_LocalForward, Vector3.up).normalized;

        // m_MoveCache.y is Z-forward, m_MoveCache.x is right
        MovementVector = (m_LocalForward * m_MoveCache.y) + (m_LocalLeft * -m_MoveCache.x);
    }



    #region Input System Callbacks

    /// <summary>
    ///     Toggle auto-fire when the 'Fire' action is received.
    ///     Auto-fire is enabled when the button is first pressed, then
    ///     when the button is released, auto-fire is disabled.
    /// </summary>
    public void OnFire(CallbackContext context)
    {
        if (context.started)
        {
            m_ActiveFire = true;
        }

        if (context.canceled)
        {
            m_ActiveFire = m_AutoFire;
        }
    }

    /// <summary>
    ///     Toggles auto-fire when the 'ToggleFire' action is received.
    ///     State changes each time the button is pressed.
    /// </summary>
    public void OnToggleFire(CallbackContext context)
    {
        if (context.performed)
        {
            m_AutoFire = !m_AutoFire;
            m_ActiveFire = m_ActiveFire || m_AutoFire;
            m_ProjectileElapsed = 0f;
        }
    }

    /// <summary>
    ///     Watches for the 'Rotate' action which is one of the cases
    ///     where the local direction vectors must be recalculated
    /// </summary>
    public void OnRotate(CallbackContext context)
    {
        float activeRotation = context.ReadValue<float>();
        m_MarkDirty = activeRotation != 0f;
    }

    /// <summary>
    ///     Handle movement input
    /// </summary>
    public void OnMove(CallbackContext context)
    {
        // Get input from InputSystem
        m_MoveCache = context.ReadValue<Vector2>();

        CalculateDirectionVectors();
    }

    /// <summary>
    ///     Reads a <see cref="Vector2"/> from the <paramref name="context"/>
    ///     and caches the direction vector for projectile direction
    /// </summary>
    public void OnLook(CallbackContext context)
    {
        Vector2 look = context.ReadValue<Vector2>();

        // Update "look" cache from joystick input
        if (!look.Equals(Vector2.zero))
        {
            m_LookCache = look;
        }
    }

    public void ControlsChanged(PlayerInput input)
    {
        switch (input.currentControlScheme)
        {
            case null:
                Debug.LogWarning("No input control scheme");
                break;
            case KeyboardControlScheme:
                m_MouseControl = true;
                break;
            default:
                m_MouseControl = false;
                break;
        }
    }

    #endregion
}
