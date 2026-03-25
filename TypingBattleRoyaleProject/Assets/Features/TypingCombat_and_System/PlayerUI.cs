using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.NetworkInformation;

public class PlayerUI : MonoBehaviour
{
    public CastInputController CIController;

    public GameObject InG_UI;
    public GameObject InS_UI;
    public bool G_UI = true;

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
        CreatePlayerCanvas();
        InG_UI.SetActive(true);
        InS_UI.SetActive(false);
    }

    void Update()
    {
        CurrentText();
        SpellCompleted();
        UpdateHPSliders();
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
            InG_UI.SetActive(false);
            InS_UI.SetActive(true);
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
            InS_UI.SetActive(false);
            InG_UI.SetActive(true);
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
