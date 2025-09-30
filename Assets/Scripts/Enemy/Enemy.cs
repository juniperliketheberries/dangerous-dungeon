using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public interface IEnemy : IVulnerable, IPoolable
{
    public CharacterController Controller { get; }

    public Action OnDeath { get; set; }

    public float Speed { get ; set; }

    public void Move();
}


public class Enemy : MonoBehaviour, IEnemy
{
    private const float DeathAnimWaitTime = 1.3f;
    private readonly int AnimMoveSpeedKey = Animator.StringToHash("MovementSpeed");
    private readonly int AnimDeadParam = Animator.StringToHash("Dead");

    
    [SerializeField] private CharacterController m_Controller;
    
    private GameObject m_Target;
    public CharacterController Controller => m_Controller;

    public Action OnDeath { get; set; }

    [Header("Movement Settings")]
    [SerializeField] private Animator m_Animator;
    [SerializeField, Range(0.1f, 10f)] private float m_MovementSpeed = 1f;
    

    public float Speed { get => m_MovementSpeed; set => m_MovementSpeed = value; }

    [Header("Gameplay Settings")]
    [SerializeField] private float m_InitialHitPoints = 1f;
    private float m_HitPoints;

    public float InitialHitPoints { get => m_InitialHitPoints; set => m_InitialHitPoints = value; }
    public float HitPoints { get => m_HitPoints; set => m_HitPoints = value; }

    public float LastHitTime { get; set; }

    public bool IsDamaged => HitPoints <= 0;


    // TODO 
    public UnityEvent Damaged => throw new NotImplementedException();

    public UnityEvent Restored => throw new NotImplementedException();

    protected virtual void OnValidate()
    {
        if (!m_Controller) { m_Controller = GetComponent<CharacterController>(); }
    }


    protected virtual void Start()
    {
        // Inject this externally through EnemyManager
        m_Target = GameObject.FindGameObjectWithTag("Player");

        if (!m_Target)
        {
            Debug.LogWarning("Entity has no player to follow");
            Destroy(gameObject);
        }
    }

    /// <summary>
    ///     Uses the <see cref="CharacterController"/> to move the entity and respect collisions
    /// </summary>
    protected virtual void Update()
    {
        Move();
    }

    public virtual void Move()
    {
        if (!m_Target || !m_Controller.enabled) { return; }

        transform.LookAt(m_Target.transform.position);

        Vector3 directionVector = m_MovementSpeed * Time.deltaTime * Vector3.Normalize(m_Target.transform.position - transform.position);
        m_Controller.Move(directionVector);

        // Animate movement
        m_Animator.SetFloat(AnimMoveSpeedKey, directionVector.magnitude);
    }

    public virtual void Hit(IProjectile projectile)
    {
        (this as IVulnerable).HandleCollision(projectile);

        LastHitTime = Time.time;

        Debug.Log($"Enemy {name} now has {HitPoints} hitpoints");

        if (IsDamaged) { Die(); }
    }

    public virtual void Die()
    {
        m_Controller.enabled = false;
        StartCoroutine(DieCoroutine());
    }

    protected virtual IEnumerator DieCoroutine()
    {
        // Could play a death animation here
        //yield return new WaitForSeconds(1);
        m_Animator.SetBool(AnimDeadParam, true);

        yield return new WaitForSeconds(DeathAnimWaitTime);

        OnDeath?.Invoke();
    }

    public void New()
    {
        m_Animator.SetBool(AnimDeadParam, false);
        m_HitPoints = m_InitialHitPoints;
    }

    // Currently not used, but can be called from the Object Pool
    public void Free() { }
}
