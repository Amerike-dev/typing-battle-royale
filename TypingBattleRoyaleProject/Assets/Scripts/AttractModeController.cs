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

    [Header("Configuración General")]
    [SerializeField] private float timeToTriggerDemo = 30f;
    [SerializeField] private string demoSceneName = "DemoScene";
    
    [Header("UI y Transiciones (Fade)")]
    [SerializeField] private Image fadeImage; // Imagen negra que cubre toda la pantalla
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private GameObject lobbyCanvas; // El contenedor de toda tu UI del lobby

    [Header("Estado Actual (Solo lectura)")]
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
        // Ojo: Ya NO es DontDestroyOnLoad. Este script vive y muere en el Lobby.
        
        // Asegurarnos de que el fade empiece transparente
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
                    fadeImage.raycastTarget = true; // Bloquea clics durante el fade
                }
                break;

            case AttractState.FadingToDemo:
                if (ProcessFade(1f)) // Fundido a negro
                {
                    // Iniciar carga aditiva (encima del lobby)
                    asyncOperation = SceneManager.LoadSceneAsync(demoSceneName, LoadSceneMode.Additive);
                    currentState = AttractState.LoadingDemo;
                }
                break;

            case AttractState.LoadingDemo:
                if (asyncOperation != null && asyncOperation.isDone)
                {
                    // Ocultamos la UI del Lobby para que no se empalme con el demo
                    if (lobbyCanvas != null) lobbyCanvas.SetActive(false);
                    currentState = AttractState.PlayingDemo;
                }
                break;

            case AttractState.PlayingDemo:
                if (ProcessFade(0f)) // Fundido a transparente (mostrar la demo)
                {
                    fadeImage.raycastTarget = false; // Permitimos que la UI del demo funcione si hubiera

                    // Escuchar CUALQUIER tecla para abortar
                    if (AnyInputDetected())
                    {
                        Debug.Log("Abortando Demo...");
                        currentState = AttractState.FadingToLobby;
                        fadeImage.raycastTarget = true;
                    }
                }
                break;

            case AttractState.FadingToLobby:
                if (ProcessFade(1f)) // Fundido a negro otra vez
                {
                    // Iniciar descarga de la demo
                    asyncOperation = SceneManager.UnloadSceneAsync(demoSceneName);
                    currentState = AttractState.UnloadingDemo;
                }
                break;

            case AttractState.UnloadingDemo:
                if (asyncOperation != null && asyncOperation.isDone)
                {
                    // Prendemos la UI del Lobby de nuevo
                    if (lobbyCanvas != null) lobbyCanvas.SetActive(true);
                    
                    idleTimer = 0f; // Reiniciamos reloj
                    currentState = AttractState.WaitingInLobby;
                }
                break;
        }

        // Si regresamos al Lobby, fundimos a transparente
        if (currentState == AttractState.WaitingInLobby)
        {
            if (ProcessFade(0f)) fadeImage.raycastTarget = false;
        }
    }

    public void ResetIdleTimer()
    {
        if (currentState == AttractState.WaitingInLobby)
        {
            idleTimer = 0f;
        }
    }

    // Retorna true cuando el fade llega a su objetivo (0 o 1)
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