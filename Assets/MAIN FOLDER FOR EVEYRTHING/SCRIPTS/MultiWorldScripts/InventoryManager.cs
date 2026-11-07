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

    // true once the tree has been spawned in the hub, so it reappears on later visits
    public bool TreeSpawned { get; set; }

    // whenever an item is added. Useful for updating UI
    public event Action<ItemType> OnItemAdded;

    // ---------- Coins ----------
    public int Coins { get; private set; }

    // whenever the coin total changes, [passes the new total
    public event Action<int> OnCoinsChanged;

    // IDs of coins already picked up, so they don't respawn when a scene reloads.
    private readonly HashSet<string> collectedCoinIds = new HashSet<string>();

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

    //Adds an item. Returns false if the inventory is full or already holds it
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

    // ---------- Coin methods ----------

    public bool IsCoinCollected(string coinId) => collectedCoinIds.Contains(coinId);

    //Adds coins and remembers this coin's ID so it stays collected
    public void CollectCoin(string coinId, int value)
    {
        if (!collectedCoinIds.Add(coinId)) return; // already collected
        AddCoins(value);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
        Debug.Log($"Coins: {Coins}");
    }

    /// <summary>Spends coins if the player has enough. Returns false otherwise.</summary>
    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }
}