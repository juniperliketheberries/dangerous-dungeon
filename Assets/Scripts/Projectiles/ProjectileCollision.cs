using UnityEngine;
using UnityEngine.Events;

public class ProjectileCollision : MonoBehaviour
{
    [System.Serializable]
    public enum Reaction
    {
        Block,
        Event
    }

    public Reaction ReactionMode;

    private IProjectile m_Last;

    public UnityEvent<IProjectile> OnCollision;

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Projectile collided with {gameObject.name}. ReactionMode Mode: {ReactionMode}");

    }

    public void OnTriggerEnter(Collider other)
    {
        if (!enabled) { return; }

        if (!other.TryGetComponent<IProjectile>(out var projectile)) { return; }

        Debug.Log($"Projectile collided with {gameObject.name} to trigger. ReactionMode Mode: {ReactionMode}");

        switch (ReactionMode)
        {
            case Reaction.Block:
                BlockProjectile(projectile);
                break;
            case Reaction.Event:
                OnCollision?.Invoke(projectile);
                break;
        }

        m_Last = projectile;
    }

    /// <summary>
    ///     Disposes of the last projectile to collide with this object
    /// </summary>
    public void BlockProjectile(IProjectile projectile)
    {
        if (projectile == null) return;
        projectile.Decoration = true;
        projectile.Dispose();
    }

    public void SetReactionModeBlock() => ReactionMode = Reaction.Block;

    public void SetReactionModeEvent() => ReactionMode = Reaction.Event;
}
