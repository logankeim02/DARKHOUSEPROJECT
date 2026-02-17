using UnityEngine;

[CreateAssetMenu(menuName = "DarkHouse/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string displayName;
    public Sprite icon;

    public Sprite inspectSprite; // NEW
}
