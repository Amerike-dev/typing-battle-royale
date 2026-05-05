using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _menuContent;
    [SerializeField] private GameplayManager _gameplayManager;
    public string sceneMenu;

    [Header("Buttons")]
    [SerializeField] private Button _resumeButton;

    [SerializeField] private InputActionReference _aPause;
    [SerializeField] private GameObject _buttonHost;
    

    [SerializeField]private bool isPaused = false;

    private void Awake()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(ResumeGame);


    }

    private void Start()
    {
        isPaused = false;
        if (_menuContent != null)
            _menuContent.SetActive(isPaused);

    }

    private void Update()
    {


            if (_gameplayManager != null && _gameplayManager.stateMachine != null)

            TogglePause();
        
    }

    public void TogglePause()
    {
        if (IsGameOverActive())
           return;

        if (isPaused)
            ResumeGame();
            
    }
    public void OnPausa()
    {
        if (_gameplayManager != null && _gameplayManager.stateMachine != null)

        TogglePause();
        Debug.Log("Juego en pausa");
        isPaused = true;
        Debug.Log(isPaused);

        if (_menuContent != null && isPaused)
            _menuContent.SetActive(isPaused);


        Debug.Log("Juego en pausa");
        AudioListener.pause = true;


    }

   

    public void ResumeGame()
    {
        isPaused = false;
        

        if (_menuContent != null && !isPaused)
            _menuContent.SetActive(isPaused);

    }

    private bool IsGameOverActive()
    {
        if (_gameplayManager == null || _gameplayManager.stateMachine == null)
            return false;

        return _gameplayManager.stateMachine.currentState is GameOverState;
    }

    private void OnDestroy()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(ResumeGame);

    }

    private void OnEnable()
    {
        _aPause.action.started += ctx => OnPausa();
        _aPause.action.Enable();

        _buttonHost.SetActive(NetworkManager.Singleton.IsServer);
    }
    private void OnDisable()
    {
        _aPause.action.started -= ctx => OnPausa();
        _aPause.action.Disable();
    }

    public void SceneMenu()
    {
        SceneManager.LoadScene(sceneMenu);
    }

    
}
