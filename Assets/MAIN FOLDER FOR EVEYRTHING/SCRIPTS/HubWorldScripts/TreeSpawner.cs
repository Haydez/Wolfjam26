// TreeSpawner.cs
// Put this on any GameObject in the hub scene.
// Setup:
//   - Create an empty GameObject where the tree should appear and drag it into "Spawn Point".
//   - Drag your tree prefab into "Tree Prefab".
// Start() runs each time the hub loads, so returning with all three items spawns the tree.
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Remove the three items from the inventory when the tree is planted.")]
    [SerializeField] private bool consumeItems = false;

    private void Start()
    {
        if (treePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("TreeSpawner: assign Tree Prefab and Spawn Point in the Inspector.", this);
            return;
        }

        var inv = InventoryManager.Instance;

        if (inv.TreeSpawned || inv.HasAllItems())
        {
            Instantiate(treePrefab, spawnPoint.position, spawnPoint.rotation);

            if (!inv.TreeSpawned && consumeItems)
                inv.Clear();

            inv.TreeSpawned = true;
        }
    }
}
