using UnityEngine;

[CreateAssetMenu(menuName = "DarkHouse/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string displayName;

    [Header("Inventory")]
    public Sprite icon; // keep this name to match existing UI code

    [Header("Inspect")]
    public Sprite inspectSprite;

    [Header("Inspect SFX")]
    [Tooltip("Played when the inspect overlay opens for this item.")]
    public AudioClip inspectOpenSfx;

    [Tooltip("Played when the inspect overlay closes for this item.")]
    public AudioClip inspectCloseSfx;
}
