using System.Collections.Generic;
using UnityEngine;

public class CheckpointMenu : BaseMenu
{
    public static CheckpointMenu Instance { get; private set; }

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject buttonPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public override void Open()
    {
        base.Open();
        if (menuPanel != null) menuPanel.SetActive(true);
        RefreshList();
    }

    public void OpenMenu()
    {
        Open();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        Close();
    }

    private void RefreshList()
    {
        // Limpiar lista anterior
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Obtener checkpoints descubiertos (todos, no solo los de esta escena)
        List<CheckpointManager.DiscoveredCheckpointData> discovered = CheckpointManager.Instance.GetDiscoveredCheckpointsData();

        foreach (var cpData in discovered)
        {
            GameObject go = Instantiate(buttonPrefab, container);
            if (go.TryGetComponent<CheckpointUIElement>(out var element))
            {
                element.Setup(cpData);
            }
        }
    }
}
