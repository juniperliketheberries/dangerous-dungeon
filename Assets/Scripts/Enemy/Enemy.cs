using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public interface IEnemy
{
    public CharacterController Controller { get; }

    public Action OnDeath { get; set; }

    public float Speed { get ; set; }

    public void Move();

    public void Reset();
}


public class Enemy : Vulnerable, IEnemy
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


    protected virtual void OnValidate()
    {
        if (!m_Controller) { m_Controller = GetComponent<CharacterController>(); }
    }


    protected override void Start()
    {
        // Inject this externally through EnemyManager
        m_Target = GameObject.FindGameObjectWithTag("Player");

        if (!m_Target)
        {
            Debug.LogWarning("Entity has no player to follow");
            Destroy(gameObject);
        }

        Damaged.AddListener(Die);
    }

    /// <summary>
    ///     Uses the <see cref="CharacterController"/> to move the entity and respect collisions
    /// </summary>
    protected override void Update()
    {
        base.Update();

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

    public virtual void Die()
    {
        m_Controller.enabled = false;
        StartCoroutine(DieCoroutine());
    }

    protected virtual IEnumerator DieCoroutine()
    {
        m_Animator.SetBool(AnimDeadParam, true);

        yield return new WaitForSeconds(DeathAnimWaitTime);

        OnDeath?.Invoke();
    }

    public void Reset()
    {
        m_Animator.SetBool(AnimDeadParam, false);
        CurrentPoints = InitialPoints;
    }
}
