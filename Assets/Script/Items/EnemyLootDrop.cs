using UnityEngine;

public sealed class EnemyLootDrop : MonoBehaviour
{
    [SerializeField] private ItemDefinition guaranteedDrop;
    [SerializeField, Min(1)] private int quantity = 1;
    private bool dropped;

    public void Drop()
    {
        if (dropped || guaranteedDrop == null)
            return;
        dropped = true;
        ItemPickup2D.Create(guaranteedDrop, quantity, transform.position + Vector3.up * 0.35f);
    }
}
