using UnityEngine;

[CreateAssetMenu(menuName = "DarkHouse/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string displayName;

    [Header("Inventory UI")]
    public Sprite icon;          // small image in inventory bar

    [Header("Inspect View")]
    public Sprite inspectSprite; // big image shown when item is opened
}
