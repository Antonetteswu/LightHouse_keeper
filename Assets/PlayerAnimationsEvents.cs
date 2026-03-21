using UnityEngine;

public class Entity_AnimationsEvents : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        // This finds the Entity script on the parent object
        entity = GetComponentInParent<Entity>();
    }

    // These functions MUST be public so the Animator can see them
    public void DamageTargets() => entity.DamageTargets();

    public void DisableMovementAndJump() => entity.EnableMovementAndJump(false);

    public void EnableMovementAndJump() => entity.EnableMovementAndJump(true);
}