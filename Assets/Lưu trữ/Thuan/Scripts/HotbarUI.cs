using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemIcon
    {
        public string itemName;
        public Sprite icon;
    }

    [Header("Danh sach icon cho tung loai item")]
    public ItemIcon[] iconLibrary;

    [Header("Cac o tren hotbar (keo theo thu tu)")]
    public Image[] slots;

    [Header("O co dinh cho den pin (o cuoi)")]
    public Image flashlightSlot;   // keo o cuoi cung vao day

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
        // Cac o thuong: dien theo thu tu nhat
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < PlayerInventory.items.Count)
            {
                string name = PlayerInventory.items[i];
                Sprite icon = FindIcon(name);
                slots[i].sprite = icon;
                slots[i].enabled = (icon != null);
            }
            else
            {
                slots[i].enabled = false;
            }
        }

        // O den pin: hien khi da nhat, an khi chua
        if (flashlightSlot != null)
        {
            if (PlayerInventory.hasFlashlight)
            {
                Sprite icon = FindIcon("DenPin");
                flashlightSlot.sprite = icon;
                flashlightSlot.enabled = (icon != null);
            }
            else
            {
                flashlightSlot.enabled = false;
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