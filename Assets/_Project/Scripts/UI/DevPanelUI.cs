using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DevPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button addAllItemsButton;
    [SerializeField] private Button removeAllItemsButton;
    [SerializeField] private Button resetPlayerPrefsButton;

    [Header("All ItemData Assets")]
    [SerializeField] private List<ItemData> allItems = new();

    [Header("Teleport")]
[SerializeField] private string room4SceneName = "Room4House1";

    [Header("Optional")]
    [SerializeField] private PickupToastUI toastUI;

    private bool isOpen;

    private void Awake()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (addAllItemsButton != null)
            addAllItemsButton.onClick.AddListener(AddAllItems);

        if (removeAllItemsButton != null)
            removeAllItemsButton.onClick.AddListener(RemoveAllItems);

        if (resetPlayerPrefsButton != null)
            resetPlayerPrefsButton.onClick.AddListener(ResetPlayerPrefsData);

        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>(FindObjectsInactive.Include);

        SetPanelOpen(false);
    }

    public void TogglePanel()
    {
        SetPanelOpen(!isOpen);
    }

    public void SetPanelOpen(bool open)
    {
        isOpen = open;

        if (panelRoot != null)
            panelRoot.SetActive(isOpen);
    }

    public void AddAllItems()
    {
        if (InventoryManager.Instance == null)
            return;

        foreach (var item in allItems)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemId))
                continue;

            if (!InventoryManager.Instance.Has(item.itemId))
                InventoryManager.Instance.Add(item);
        }

        ShowToast("Dev: Added all items");
    }

    public void RemoveAllItems()
    {
        if (InventoryManager.Instance == null)
            return;

        List<string> idsToRemove = new List<string>(InventoryManager.Instance.ItemIds);

        foreach (string id in idsToRemove)
            InventoryManager.Instance.Remove(id);

        if (InventoryInteractionManager.Instance != null)
            InventoryInteractionManager.Instance.ClearSelectedItem();

        ShowToast("Dev: Removed all items");
    }

    public void ResetPlayerPrefsData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (InventoryInteractionManager.Instance != null)
            InventoryInteractionManager.Instance.ClearSelectedItem();

        ShowToast("Dev: Reset PlayerPrefs");
    }

    private void ShowToast(string message)
    {
        if (toastUI == null)
            toastUI = FindFirstObjectByType<PickupToastUI>(FindObjectsInactive.Include);

        if (toastUI != null)
            toastUI.Show(message);
        else
            Debug.Log(message);
    }

    public void TeleportToRoom4House1()
{
    InventoryInteractionManager.Instance?.ClearSelectedItem();

    if (!string.IsNullOrEmpty(room4SceneName))
    {
        SceneManager.LoadScene(room4SceneName);
    }
}
}