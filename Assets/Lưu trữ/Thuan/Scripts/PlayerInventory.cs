using System.Collections.Generic;

public static class PlayerInventory
{
    public static List<string> items = new List<string>();

    public static bool hasFlashlight = false;   // den pin: co/khong, hien o o cuoi

    public static void Add(string itemName)
    {
        items.Add(itemName);
    }

    public static int Count(string itemName)
    {
        int n = 0;
        foreach (string it in items)
            if (it == itemName) n++;
        return n;
    }

    public static void RemoveAll(string itemName)
    {
        items.RemoveAll(x => x == itemName);
    }

    public static void Clear()
    {
        items.Clear();
        hasFlashlight = false;   // reset luon khi clear
    }
}