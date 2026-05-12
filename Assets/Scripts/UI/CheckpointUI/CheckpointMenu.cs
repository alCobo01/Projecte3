using System.Collections.Generic;
using UnityEngine;

public class CheckpointMenu : MonoBehaviour
{
    public static CheckpointMenu Instance { get; private set; }

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject buttonPrefab;

    private void Awake()
    {
        Instance = this;
        menuPanel.SetActive(false);
    }

    public void OpenMenu()
    {
        menuPanel.SetActive(true);
        RefreshList();
        
        // Pausar el juego si es necesario
        if (PauseManager.Instance != null)
        {
            // Podrías llamar a PauseManager.Instance.TogglePause() o similar
        }
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
    }

    private void RefreshList()
    {
        // Limpiar lista anterior
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Obtener checkpoints descubiertos
        List<Checkpoint> discovered = CheckpointManager.Instance.GetDiscoveredCheckpoints();

        foreach (var cp in discovered)
        {
            GameObject go = Instantiate(buttonPrefab, container);
            if (go.TryGetComponent<CheckpointUIElement>(out var element))
            {
                element.Setup(cp);
            }
        }
    }

    private void Update()
    {
        // Abrir menú con una tecla (ejemplo: 'G' de Guardado/Gate)
        // O podrías abrirlo solo al interactuar con un checkpoint
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (menuPanel.activeSelf) CloseMenu();
            else OpenMenu();
        }
    }
}
