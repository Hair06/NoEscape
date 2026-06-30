using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemIcon
    {
        public string itemName;   // ten item (khop voi PickupItem)
        public Sprite icon;       // anh icon cua item do
    }

    [Header("Danh sach icon cho tung loai item")]
    public ItemIcon[] iconLibrary;   // khai bao Xang -> anh binh xang, BoPhan1 -> anh...

    [Header("Cac o tren hotbar (keo theo thu tu)")]
    public Image[] slots;            // o 0, o 1, o 2... (cac Image dat trong khung)

    void Start()
    {
        PlayerInventory.Clear();
        RefreshHotbar();
    }

    void Update()
    {
        RefreshHotbar();
    }

    void RefreshHotbar()
    {
        // Duyet tung o
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < PlayerInventory.items.Count)
            {
                // O nay co item -> hien icon tuong ung
                string name = PlayerInventory.items[i];
                Sprite icon = FindIcon(name);

                slots[i].sprite = icon;
                slots[i].enabled = (icon != null);
            }
            else
            {
                // O trong -> an icon
                slots[i].enabled = false;
            }
        }
    }

    Sprite FindIcon(string itemName)
    {
        foreach (var entry in iconLibrary)
            if (entry.itemName == itemName) return entry.icon;
        return null;
    }
}