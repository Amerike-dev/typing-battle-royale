using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AttractModeController : MonoBehaviour
{
    public static AttractModeController Instance { get; private set; }

    public enum AttractState 
    { 
        WaitingInLobby, 
        FadingToDemo, 
        LoadingDemo, 
        PlayingDemo, 
        FadingToLobby, 
        UnloadingDemo 
    }

    [Header("General configuration")]
    [SerializeField] private float timeToTriggerDemo = 30f;
    [SerializeField] private string demoSceneName = "AttractMode";
    
    [Header("UI and Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private GameObject lobbyCanvas;

    [Header("State")]
    public AttractState currentState = AttractState.WaitingInLobby;
    private float idleTimer = 0f;
    private AsyncOperation asyncOperation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (fadeImage != null)
        {
            SetFadeAlpha(0f);
            fadeImage.raycastTarget = false;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case AttractState.WaitingInLobby:
                idleTimer += Time.unscaledDeltaTime;
                if (idleTimer >= timeToTriggerDemo)
                {
                    Debug.Log("Iniciando Demo Mode...");
                    currentState = AttractState.FadingToDemo;
                    fadeImage.raycastTarget = true;
                }
                break;

            case AttractState.FadingToDemo:
                if (ProcessFade(1f))
                {
                    asyncOperation = SceneManager.LoadSceneAsync(demoSceneName, LoadSceneMode.Additive);
                    currentState = AttractState.LoadingDemo;
                }
                break;

            case AttractState.LoadingDemo:
                if (asyncOperation != null && asyncOperation.isDone)
                {
                    if (lobbyCanvas != null) lobbyCanvas.SetActive(false);
                    currentState = AttractState.PlayingDemo;
                }
                break;

            case AttractState.PlayingDemo:
                if (ProcessFade(0f))
                {
                    fadeImage.raycastTarget = false;
                    
                    if (AnyInputDetected())
                    {
                        Debug.Log("Abortando Demo...");
                        currentState = AttractState.FadingToLobby;
                        fadeImage.raycastTarget = true;
                    }
                }
                break;

            case AttractState.FadingToLobby:
                if (ProcessFade(1f))
                {
                    asyncOperation = SceneManager.UnloadSceneAsync(demoSceneName);
                    currentState = AttractState.UnloadingDemo;
                }
                break;

            case AttractState.UnloadingDemo:
                if (asyncOperation != null && asyncOperation.isDone)
                {
                    if (lobbyCanvas != null) lobbyCanvas.SetActive(true);
                    
                    idleTimer = 0f;
                    currentState = AttractState.WaitingInLobby;
                }
                break;
        }
        
        if (currentState == AttractState.WaitingInLobby)
        {
            if (ProcessFade(0f)) fadeImage.raycastTarget = false;
        }
    }

    public void ResetIdleTimer()
    {
        if (currentState == AttractState.WaitingInLobby) idleTimer = 0f;
    }
    
    private bool ProcessFade(float targetAlpha)
    {
        if (fadeImage == null) return true;

        Color c = fadeImage.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        fadeImage.color = c;

        return Mathf.Abs(c.a - targetAlpha) < 0.01f;
    }

    private void SetFadeAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    private bool AnyInputDetected()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame) ||
               (Mouse.current != null && (Mouse.current.delta.ReadValue().sqrMagnitude > 0 || Mouse.current.leftButton.wasPressedThisFrame));
    }
}