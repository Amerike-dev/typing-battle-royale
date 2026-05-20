using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

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

    [Header("UIanimation")]
    [SerializeField] RectTransform _panelUI;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Vector2 _hidePos;
    [SerializeField] Vector2 _showPos;
    [SerializeField] float _time = 0.2f;
    Coroutine _pauseCoroutine;

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
        var state = GameplayManager.Instance.stateMachine.currentState;

        if(state is GameOverState || state is WaitingState) return;

        if (_gameplayManager != null && _gameplayManager.stateMachine != null)

        TogglePause();
        Debug.Log("Juego en pausa");
        isPaused = true;
        
        Debug.Log(isPaused);

        if (_menuContent != null && isPaused)
        {
            UIMove(_hidePos);
            UIAnimator.FadeOut(_canvasGroup, _time);
            //_menuContent.SetActive(isPaused);
            Debug.Log("Me activo Menu");
        }

        Debug.Log("Juego en pausa");
        AudioListener.pause = true;
    }
    public void ResumeGame()
    {
        isPaused = false;
        
        if (_menuContent != null && !isPaused)
        {
            UIAnimator.FadeIn(_canvasGroup, _time);
            UIMove(_showPos);

            //_menuContent.SetActive(isPaused);
        }
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
        SceneManager.LoadScene("LobbyScene");
    }

    public void UIMove(Vector2 target)
    {
        if (_pauseCoroutine != null)
        {
            StopCoroutine(_pauseCoroutine);
        }
        _pauseCoroutine = StartCoroutine(UIAnimator.PanelUIMove(_panelUI, target, _time));
    }
    public IEnumerator ChangeMode()
    {
        yield return new WaitForSeconds(_time);
        _menuContent.SetActive(true);
    }
}
