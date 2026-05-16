using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using WotK.Brand;

public class SplashController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup studioCanvas;
    [SerializeField] private CanvasGroup gameCanvas;

    [SerializeField] private TMP_Text taglineText;

    [Header("Timing Configuration")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float skipFadeDuration = 0.2f;
    
    private Sequence splashSequence;
    private bool isSkipping = false;

    void Start()
    {
        if (backgroundImage != null) backgroundImage.color = WotKBrand.Black;
        
        if (taglineText != null) taglineText.color = WotKBrand.White;
        
        studioCanvas.alpha = 0f;
        gameCanvas.alpha = 0f;
        PlaySplashSequence();
    }

    void Update()
    {
        if (!isSkipping && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) SkipSplash();
    }
    
    private void PlaySplashSequence()
    {
        splashSequence = DOTween.Sequence();
        splashSequence.SetUpdate(true);
        splashSequence.Append(studioCanvas.DOFade(1f, fadeInDuration));
        splashSequence.AppendInterval(holdDuration);
        splashSequence.Append(studioCanvas.DOFade(0f, fadeOutDuration));
        splashSequence.Join(gameCanvas.DOFade(1f, fadeInDuration));
        splashSequence.AppendInterval(holdDuration);
        splashSequence.Append(gameCanvas.DOFade(0f, fadeOutDuration));
        splashSequence.OnComplete(LoadLobbyScene);
    }

    private void SkipSplash()
    {
        isSkipping = true;
        splashSequence.Kill();
        Sequence skipSequence = DOTween.Sequence().SetUpdate(true);
        skipSequence.Join(studioCanvas.DOFade(0f, skipFadeDuration));
        skipSequence.Join(gameCanvas.DOFade(0f, skipFadeDuration));
        skipSequence.OnComplete(LoadLobbyScene);
    }

    private void LoadLobbyScene()
    {
        SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    private void OnDestroy()
    { 
        splashSequence?.Kill();
    }
}
