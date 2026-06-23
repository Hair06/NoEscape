public static class FuelInventory
{
    public static int cansHeld = 0;

    public static void AddCan()
    {
        cansHeld++;
    }

    public static bool HasCans(int amount)
    {
        return cansHeld >= amount;
    }

    public static bool TryConsumeCans(int amount)
    {
        if (!HasCans(amount))
        {
            return false;
        }

        cansHeld -= amount;
        return true;
    }
}
