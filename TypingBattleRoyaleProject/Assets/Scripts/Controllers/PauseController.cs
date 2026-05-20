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
        _menuContent.SetActive(false);
    }

    private void Start()
    {
        isPaused = false;
        if (_menuContent != null)
            _menuContent.SetActive(isPaused);
    }

    private void OnEnable()
    {
        _aPause.action.started += OnPauseChange; //ctx => OnPausa();
        _aPause.action.Enable();

        _buttonHost.SetActive(NetworkManager.Singleton.IsServer);
    }
    private void OnDisable()
    {
        _aPause.action.started -= OnPauseChange; //ctx => OnPausa();
        _aPause.action.Disable();
    }
    private void OnPauseChange(InputAction.CallbackContext ctx) => OnPausa();
    public void SceneMenu()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    /*private void Update()
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
            
    }*/
    public void OnPausa()
    {
        var state = GameplayManager.Instance.stateMachine.currentState;

        if(state is GameOverState || state is WaitingState) return;

        if (isPaused) ResumeGame();
        else PauseGame();
        /*if (_gameplayManager != null && _gameplayManager.stateMachine != null)

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
        AudioListener.pause = true;*/
    }
    public void PauseGame()
    {
        isPaused = true;
        _menuContent.SetActive(true);
        if (_menuContent != null && isPaused)
        {
            UIMove(_showPos);
            UIAnimator.FadeOut(_canvasGroup, _time);
            Debug.Log("Me activo Menu");
        }
        AudioListener.pause = true;
    }
    public void ResumeGame()
    {
        isPaused = false;
        
        if (_menuContent != null && !isPaused)
        {
            UIMove(_hidePos);
            UIAnimator.FadeIn(_canvasGroup, _time);
        }
        AudioListener.pause= false;

        if(_menuContent!=null)
            StopCoroutine(_pauseCoroutine);
        _pauseCoroutine=StartCoroutine(ChangeMode());
    }

    /*private bool IsGameOverActive()
    {
        if (_gameplayManager == null || _gameplayManager.stateMachine == null)
            return false;

        return _gameplayManager.stateMachine.currentState is GameOverState;
    }*/

    private void OnDestroy()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(ResumeGame);
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
        _menuContent.SetActive(false);
    }
}
