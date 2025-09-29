using UnityEngine;
using UnityEngine.Events;

public class Vulnerable : MonoBehaviour, IVulnerable
{
    private bool dirty = false;

    private float restoreCache;
    public float RestoreTime = 5;

    public float InitialHitPoints = 5;

    public float HitPoints { get; set; }

    public bool IsDamaged => HitPoints <= 0;

    [SerializeField] private UnityEvent m_Damaged;
    [SerializeField] private UnityEvent m_Restored;

    public UnityEvent Damaged => m_Damaged;

    public UnityEvent Restored => m_Restored;

    public void Start()
    {
        HitPoints = InitialHitPoints;
    }

    public void Update()
    {
        if (dirty)
        {
            dirty = false;

            if (IsDamaged) { Damage(); }
        }

        // No 'Restore' logic if time is set below 0
        if (RestoreTime < 0) { return; }

        if (IsDamaged)
        {
            restoreCache += Time.deltaTime;
        }

        if (restoreCache > RestoreTime)
        {
            dirty = true;
            Restore();
        }
    }

    public virtual void Hit(IProjectile projectile)
    {
        (this as IVulnerable).HandleCollision(projectile);

        Debug.Log($"Object {name} now has {HitPoints} hitpoints");

        if (IsDamaged && !dirty)
        {
            dirty = true;
        }
    }

    protected void Damage()
    {
        Damaged?.Invoke();
        restoreCache = 0;
    }

    protected void Restore()
    {
        Restored?.Invoke();
        HitPoints = InitialHitPoints;
        restoreCache = 0;
    }
}
