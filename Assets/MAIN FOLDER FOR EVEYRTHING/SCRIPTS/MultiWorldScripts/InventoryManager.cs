
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public const int MaxItems = 3;
    private readonly HashSet<ItemType> items = new HashSet<ItemType>();

    public bool TreeSpawned { get; set; }
    
    public bool BeanstalkCutscenePlayed { get; set; }


    public event Action<ItemType> OnItemAdded;

    // ---------- Coins ----------
    public int Coins { get; private set; }


    public event Action<int> OnCoinsChanged;


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
    
    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }
}