using UnityEngine;

public class Entity_AnimationsEvents : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    // ADD THIS FUNCTION BELOW
    public void PlayAttackSound()
    {
        entity.PlayAttackSound();
    }

    public void DamageTargets() => entity.DamageTargets();
    public void DisableMovementAndJump() => entity.EnableMovementAndJump(false);
    public void EnableMovementAndJump() => entity.EnableMovementAndJump(true);
}