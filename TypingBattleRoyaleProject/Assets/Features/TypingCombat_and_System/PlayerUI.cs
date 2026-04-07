using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public CastInputController CIController;

    public GameObject GameUI;
    public GameObject SpellUI;
    public bool G_UI = true;

    [Header("Monolith Interaction")]
    [SerializeField] private GameObject _monolithPromptPanel;
    [SerializeField] private TextMeshProUGUI _monolithPromptText;
    public PlayerInteractorView playerInteractorView;

    private MonolithView _lastMonolith;
    private string _cachedText;
    private const string PROMPT_BASE = "Presiona E para interactuar - Nivel ";

    [Header("Sliders de vida Jugador y Enemigo")]
    [SerializeField] private Slider LocalHPSlider;
    [SerializeField] private Slider EnemyHPSlider;

    [Header("Referencia al Slider")]
    [SerializeField] private GameObject sliderPrefab;

    [Header("PlayerStats")]
    private PlayerStats localStats;
    private PlayerStats enemyStats;

    [Header("Texto para SpellUI")]
    public TextMeshProUGUI SpellText;
    public TMP_InputField InputSpellText;

    void Start()
    {
        if (_monolithPromptPanel != null) 
        _monolithPromptPanel.SetActive(false);

        CreatePlayerCanvas();
        GameUI.SetActive(true);
        SpellUI.SetActive(false);
    }

    void Update()
    {
        CurrentText();
        UpdateHPSliders();
        HandleMonolithPrompt();
    }

    public void SetPlayerStats(PlayerStats localStats, PlayerStats enemyStats)
    {
        this.localStats = localStats;
        this.enemyStats = enemyStats;
    }

    private void UpdateHPSliders()
    {
        if (localStats != null && LocalHPSlider != null && localStats.MaxHP > 0)
        {
            LocalHPSlider.value = localStats.CurrentHP / localStats.MaxHP;
        }

        if (enemyStats != null && EnemyHPSlider != null && enemyStats.MaxHP > 0)
        {
            EnemyHPSlider.value = enemyStats.CurrentHP / enemyStats.MaxHP;
        }
    }
    public void CurrentText()
    {
        SpellText.text = CIController.spellText;
    }
    
    public void SpellTypingON()
    {
        if (G_UI == true)
        {
            GameUI.SetActive(false);
            SpellUI.SetActive(true);
            HidePrompt();
            CIController.incorrectInput = 0;
            CombatLogic._stringIndex = 0;
            InputSpellText.Select();
            InputSpellText.ActivateInputField();
            G_UI = false;
        }
    }
    public void SpellTypingOFF()
    {
        if (G_UI == false)
        {
            CIController.lastInInput = CIController.incorrectInput;
            CIController.incorrectInput = 0;
            InputSpellText.text=string.Empty;
            InputSpellText.DeactivateInputField();
            SpellUI.SetActive(false);
            GameUI.SetActive(true);
            G_UI=true;
        }
    }
    public void SpellCompleted()
    {
        if (CombatLogic.spellComplete)
        {
            SpellTypingOFF();
        }
    }

    private void HandleMonolithPrompt()
    {
        if (!G_UI || playerInteractorView == null)
        {
            HidePrompt();
            return;
        }
        
        var monolith = playerInteractorView.NearestMonolith;

        if (monolith == null)
        {
            HidePrompt();
            return;
        }

        if (!_monolithPromptPanel.activeSelf)
        _monolithPromptPanel.SetActive(true);

        if (monolith != _lastMonolith)
        {
            _lastMonolith = monolith;

            _cachedText = PROMPT_BASE + monolith.Level;
            _monolithPromptText.text = _cachedText;
        }
    }

    private void HidePrompt()
    {
        if (_monolithPromptPanel != null && _monolithPromptPanel.activeSelf)
        _monolithPromptPanel.SetActive(false);

        _lastMonolith = null;
    }

    private void CreatePlayerCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject sliderInstance = Instantiate(sliderPrefab, canvasGO.transform);
        Slider slider = sliderInstance.GetComponent<Slider>();
        LocalHPSlider = slider;
    }
}
