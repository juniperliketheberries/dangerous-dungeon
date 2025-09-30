using UnityEngine.Events;

public interface IVulnerable
{
    public float InitialHitPoints { get; set; }

    public float HitPoints { get; set; }

    public bool IsDamaged { get; }

    public float LastHitTime { get; }

    public UnityEvent Damaged { get; }

    public UnityEvent Restored { get; }

    public void HandleCollision(IProjectile projectile)
    {
        projectile.Decoration = true;

        // Eventually projectiles may "pierce" and should 
        // not be disposed on contact
        projectile.Dispose();

        // Avoids negative hitpoints
        if (IsDamaged) { return; }

        // Subtract from hitpoints on collision
        HitPoints--;
    }
}
