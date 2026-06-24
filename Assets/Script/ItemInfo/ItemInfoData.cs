using UnityEngine;

public class ItemInfoData : MonoBehaviour

{
    [Header("Thông tin item")]
    public string itemName;

    [TextArea(2, 5)]
    public string description;

    [TextArea(2, 5)]
    public string useDescription;
    
}
