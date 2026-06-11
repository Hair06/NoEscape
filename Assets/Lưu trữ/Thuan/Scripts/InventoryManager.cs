using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Danh sách item đang có trong túi
    public List<string> items = new List<string>();

    void Awake()
    {
        Instance = this;
    }

    // Thêm item vào túi
    public void AddItem(string itemName)
    {
        items.Add(itemName);
        Debug.Log("Đã thêm: " + itemName);
    }

    // Xóa item khỏi túi
    public void RemoveItem(string itemName)
    {
        items.Remove(itemName);
        Debug.Log("Đã xóa: " + itemName);
    }
}
