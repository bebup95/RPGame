using UnityEngine;

public class Entity_AnimationEvents : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    public void DamageTargets() => entity.DamageTargets();

    private void DisableMovementAndJump() => entity.EnableMovement(false);  
    
    private void EnableMovementAndJump()
    {
        entity.EndAttackSwing();
        entity.EnableMovement(true);
    }

}
