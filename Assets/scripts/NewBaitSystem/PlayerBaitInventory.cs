using Fishing.Core.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBaitInventory : MonoBehaviour
{
    public static PlayerBaitInventory Instance; // Ïðîñòîé ñèíãëòîí

    // Ñëîâàðü äëÿ õðàíåíèÿ: íàæèâêà -> êîëè÷åñòâî
    private Dictionary<BaitData, int> baitCounts = new Dictionary<BaitData, int>();

    [Header("Ñòàðòîâûé íàáîð")]
    public BaitData starterBait; // Ñþäà íàçíà÷èøü "Òåñòî"
    public int starterAmount = 15;

    // Ñîáûòèå, ÷òîáû îáíîâëÿòü UI ïðè èçìåíåíèè
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Äîáàâëÿåì ñòàðòîâóþ íàæèâêó
        if (starterBait != null)
        {
            AddBait(starterBait, starterAmount);
        }
    }

    // Äîáàâèòü íàæèâêó (íàïðèìåð, ïîñëå ïîêóïêè)
    public void AddBait(BaitData bait, int amount)
    {
        if (baitCounts.ContainsKey(bait))
            baitCounts[bait] += amount;
        else
            baitCounts[bait] = amount;

        OnInventoryChanged?.Invoke();
    }

    // Ïîòðàòèòü íàæèâêó
    public bool SpendBait(BaitData bait, int amount = 1)
    {
        if (!baitCounts.ContainsKey(bait) || baitCounts[bait] < amount)
            return false;

        baitCounts[bait] -= amount;

        if (baitCounts[bait] <= 0)
            baitCounts.Remove(bait);

        OnInventoryChanged?.Invoke();
        return true;
    }

    // Ïîëó÷èòü êîëè÷åñòâî
    public int GetBaitCount(BaitData bait)
    {
        return baitCounts.ContainsKey(bait) ? baitCounts[bait] : 0;
    }

    // Ïîëó÷èòü ñïèñîê âñåõ íàæèâîê, êîòîðûå åñòü ó èãðîêà (äëÿ UI)
    public List<BaitData> GetOwnedBaits()
    {
        return baitCounts.Keys.ToList();
    }
}