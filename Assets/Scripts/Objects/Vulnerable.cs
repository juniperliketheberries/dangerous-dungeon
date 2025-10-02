using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


/// <summary>
///     A class of objects which can receive hits and be damaged
/// </summary>
public class Vulnerable : MonoBehaviour, IVulnerable
{
    private bool dirty = false;

    [field:SerializeField]
    public float InitialPoints { get; set; } = 5;

    public float CurrentPoints { get; set; }

    public float LastHitTime { get; protected set; }

    // Events
    [field: FormerlySerializedAs("m_Damaged")]
    [field: SerializeField]
    public UnityEvent Damaged { get; set; }

    // Derived Properties
    public bool IsDamaged => CurrentPoints <= 0;


    protected virtual void Start()
    {
        CurrentPoints = InitialPoints;
    }

    protected virtual void Update()
    {
        if (dirty)
        {
            dirty = false;

            if (IsDamaged) { Damage(); }
        }
    }

    public virtual void Hit(IProjectile projectile)
    {
        HandleCollision(projectile);

        LastHitTime = Time.time;

        // Subtract from hitpoints on collision
        if (!IsDamaged) 
        {
            CurrentPoints--; 
        }

        if (IsDamaged && !dirty)
        {
            dirty = true;
        }

        Debug.Log($"Object {name} now has {CurrentPoints} hitpoints");
    }

    protected virtual void Damage() => Damaged?.Invoke();

    private void HandleCollision(IProjectile projectile)
    {
        projectile.Decoration = true;

        // Eventually projectiles may "pierce" and should 
        // not be disposed on contact
        projectile.Dispose();
    }
}
