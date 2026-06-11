using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;   // Panel chứa inventory
    public Transform slotContainer;    // Nơi chứa các ô slot
    public GameObject slotPrefab;       // Mẫu 1 ô slot

    void Update()
    {
        // Nhấn I để mở/đóng inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isOpen = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isOpen);
        }
    }

    // Vẽ lại toàn bộ slot khi inventory thay đổi
    public void RefreshUI()
    {
        // Xóa slot cũ
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        // Tạo slot mới cho từng item
        foreach (string itemName in InventoryManager.Instance.items)
        {
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.GetComponentInChildren<Text>().text = itemName;
        }
    }
}