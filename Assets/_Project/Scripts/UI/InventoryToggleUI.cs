using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryToggleUI : MonoBehaviour
{
    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    [Header("Wiring")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private TMP_Text buttonText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color flashColor = new Color(0.85f, 0.1f, 0.1f, 1f);
    [SerializeField] private float flashTime = 0.25f;

    private Coroutine flashRoutine;
    private Coroutine subscribeRoutine;
    private bool subscribed;

    public void Open()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
    }

    public void Close()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleInventory);

        if (buttonText != null)
            buttonText.color = normalColor;
    }

    private void OnEnable()
    {
        if (subscribeRoutine != null) StopCoroutine(subscribeRoutine);
        subscribeRoutine = StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        Unsubscribe();
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (InventoryManager.Instance == null)
            yield return null;

        if (!subscribed)
        {
            InventoryManager.Instance.OnItemAdded += HandleItemAdded;
            subscribed = true;
        }

        subscribeRoutine = null;
    }

    private void Unsubscribe()
    {
        if (subscribed && InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;

        subscribed = false;
    }

    private void ToggleInventory()
    {
        UISfxPlayer.PlayInventoryToggle();

        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    private void HandleItemAdded(ItemData item)
    {
        FlashButton();
    }

    private void FlashButton()
    {
        if (buttonText == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        buttonText.color = flashColor;
        yield return new WaitForSecondsRealtime(flashTime);
        buttonText.color = normalColor;
    }
}
