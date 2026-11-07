// InventoryManager.cs
// Persistent inventory that survives scene loads.
// Created automatically before the first scene loads, so you don't need
// to place it in any scene — it works no matter which scene you press Play in.
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public const int MaxItems = 3;
    private readonly HashSet<ItemType> items = new HashSet<ItemType>();
    
    public bool TreeSpawned { get; set; }
    
    public event Action<ItemType> OnItemAdded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("InventoryManager");
        go.AddComponent<InventoryManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public bool AddItem(ItemType item)
    {
        if (items.Count >= MaxItems || items.Contains(item)) return false;

        items.Add(item);
        OnItemAdded?.Invoke(item);
        Debug.Log($"Picked up {item} ({items.Count}/{MaxItems})");
        return true;
    }

    public bool HasItem(ItemType item) => items.Contains(item);

    public bool HasAllItems() =>
        items.Contains(ItemType.MineralWater) &&
        items.Contains(ItemType.SpecialDirt) &&
        items.Contains(ItemType.JellybeanSeeds);

    public int Count => items.Count;

    public void Clear() => items.Clear();
}
