using System.Collections.Generic;

public static class PlayerInventory
{
    // Danh sach ten cac item dang cam (theo thu tu nhat)
    public static List<string> items = new List<string>();

    // Them item vao kho
    public static void Add(string itemName)
    {
        items.Add(itemName);
    }

    // Dem so luong 1 loai item (vd dem so binh xang)
    public static int Count(string itemName)
    {
        int n = 0;
        foreach (string it in items)
            if (it == itemName) n++;
        return n;
    }

    // Xoa toan bo 1 loai item (vd sau khi do het xang)
    public static void RemoveAll(string itemName)
    {
        items.RemoveAll(x => x == itemName);
    }

    // Xoa het kho (khi bat dau game)
    public static void Clear()
    {
        items.Clear();
    }
}