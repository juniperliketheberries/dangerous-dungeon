using UnityEngine;
using UnityEngine.Events;

public interface IVulnerable
{
    public float InitialPoints { get; set; }

    public float CurrentPoints { get; set; }

    public float LastHitTime { get; }

    public bool IsDamaged { get; }

    public UnityEvent Damaged { get; }

}
