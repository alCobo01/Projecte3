using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for handling dialogue UI and logic.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public System.Action OnDialogueEnd;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private string dialogueRootName = "Dialogue";

    [Header("Input")]
    [Tooltip("The action used to advance the dialogue when the player is frozen.")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Settings")]
    [Tooltip("Minimum time between advancing lines to prevent spamming.")]
    [SerializeField] private float progressionCooldown = 0.3f;

    private NPCDialogueSequence _currentSequence;
    private int _blockIndex;
    private int _lineIndex;
    private float _lastProgressionTime;
    private bool _isTyping;
    private bool _isDialogueActive;
    private bool _shouldFreezePlayer;
    private Coroutine _typingCoroutine;

    public bool IsDialogueActive => _isDialogueActive;
    public bool ShouldFreezePlayer => _shouldFreezePlayer && _isDialogueActive;

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }

        RebindSceneReferences();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.started += OnInteractPerformed;
            interactAction.action.Enable();
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.started -= OnInteractPerformed;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneReferences();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void RebindSceneReferences()
    {
        if (dialoguePanel != null && dialogueText != null && speakerPortraitImage != null) return;

        var root = GameObject.Find(dialogueRootName);
        if (root == null)
        {
            root = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == dialogueRootName && go.scene.IsValid());
        }
        if (root == null) return;

        dialoguePanel = root;
        if (dialogueText == null) dialogueText = root.GetComponentInChildren<TMP_Text>(true);
        if (speakerPortraitImage == null)
        {
            var portrait = root.transform.Find("Portrait");
            if (portrait == null) portrait = root.transform.Find("portrait");
            if (portrait != null) speakerPortraitImage = portrait.GetComponent<Image>();
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (_isDialogueActive)
        {
            NextLine();
        }
    }

    private void Update()
    {
        if (!_isDialogueActive) return;

        // Fallback 1: Click izquierdo para pasar dialogo
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextLine();
        }

        // Fallback 2: Tecla E directa (por si el InputActionReference no está bien configurado)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    public void StartDialogue(NPCDialogueSequence sequence, bool freezePlayer = false)
    {
        if (sequence == null || sequence.dialogues.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] Sequence is null or empty.");
            return;
        }

        if (_isDialogueActive) EndDialogue();

        _currentSequence = sequence;
        _shouldFreezePlayer = freezePlayer;
        _blockIndex = 0;
        _lineIndex = 0;
        _lastProgressionTime = Time.time;
        _isDialogueActive = true;

        dialoguePanel.SetActive(true);
        _typingCoroutine = StartCoroutine(TypeLine());
    }

    public void NextLine()
    {
        if (!_isDialogueActive) return;

        // Si el texto se está escribiendo, permitimos completarlo al instante
        if (_isTyping)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            
            var block = _currentSequence.dialogues[_blockIndex];
            dialogueText.SetText(block.lines[_lineIndex]);
            _isTyping = false;
            _lastProgressionTime = Time.time; 
            return;
        }

        // Cooldown solo para pasar a la siguiente frase UNA VEZ TERMINADA la actual
        if (Time.time < _lastProgressionTime + progressionCooldown) return;

        var currentBlock = _currentSequence.dialogues[_blockIndex];
        _lineIndex++;

        if (_lineIndex < currentBlock.lines.Length)
        {
            _typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            // Pasamos al siguiente bloque si existe
            _blockIndex++;
            if (_blockIndex < _currentSequence.dialogues.Length)
            {
                _lineIndex = 0;
                _typingCoroutine = StartCoroutine(TypeLine());
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private IEnumerator TypeLine()
    {
        _isTyping = true;
        var block = _currentSequence.dialogues[_blockIndex];
        string currentLine = block.lines[_lineIndex];
        float speed = block.typingSpeed;

        dialogueText.text = "";
        
        if (speakerPortraitImage != null)
            speakerPortraitImage.sprite = block.speakerPortrait;

        foreach (var letter in currentLine)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(speed);
        }

        _isTyping = false;
        _lastProgressionTime = Time.time;
    }

    public void EndDialogue()
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _isDialogueActive = false;
        _isTyping = false;

        if (dialogueText != null) dialogueText.SetText("");
        if (speakerPortraitImage != null) speakerPortraitImage.sprite = null;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        OnDialogueEnd?.Invoke();
    }
}
