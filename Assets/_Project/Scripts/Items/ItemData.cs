using UnityEngine;

[CreateAssetMenu(menuName = "DarkHouse/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string displayName;

    [Header("Inventory")]
    public Sprite icon;

    [Header("Inspect")]
    public Sprite inspectSprite;

    [Header("Inspect SFX")]
    public AudioClip inspectOpenSfx;
    public AudioClip inspectCloseSfx;
}
