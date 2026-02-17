using UnityEngine;

[CreateAssetMenu(menuName = "DarkHouse/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;      // unique key like "rusty_key"
    public string displayName; // "Rusty Key"
    public Sprite icon;        // optional for later
}
