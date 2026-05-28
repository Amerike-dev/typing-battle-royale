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
    [SerializeField] public GameObject _menuContent;
    [SerializeField] private GameplayManager _gameplayManager;
    public string sceneMenu;

    [Header("Panels")]
    [SerializeField] private GameObject _pauseMainPanel;
    [SerializeField] private GameObject _optionPanel;

    [Header("Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _backOptionButton;

    [SerializeField] private InputActionReference _aPause;
    [SerializeField] private GameObject _buttonHost;
    

    [SerializeField]private bool isPaused = false;

    [Header("UIanimation")]
    [SerializeField] RectTransform _panelUI;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Vector2 _hidePos;
    [SerializeField] Vector2 _showPos;
    [SerializeField] float _time = 0.2f;
    Coroutine _moveCoroutine;
    Coroutine _closeCoroutine;

    private void Awake()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(ResumeGame);

        if (_optionButton != null)
            _optionButton.onClick.AddListener(ShowOptionsPanel);

        if (_backOptionButton != null)
            _backOptionButton.onClick.AddListener(ShowPauseMenuPanel);

        _menuContent.SetActive(false);

        if (_pauseMainPanel != null)
            _pauseMainPanel.SetActive(true);

        if (_optionPanel != null)
            _optionPanel.SetActive(false);
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
    public void OnPausa()
    {
        if (MonolithLevelSelectUI.Instance != null && MonolithLevelSelectUI.Instance.myCanvas.enabled)
        {
            Debug.Log("[PauseController] El monolito está abierto, bloqueando pausa.");
            return;
        }
        
        var state = GameplayManager.Instance.stateMachine.currentState;

        if(state is GameOverState || state is WaitingState || state is BattleState) return;

        if(state is GameOverState) ResumeGame();

        if (isPaused) ResumeGame();
        else PauseGame();
    }
    public void PauseGame()
    {
        isPaused = true;
        _menuContent.SetActive(true);

        if (_pauseMainPanel != null)
            _pauseMainPanel.SetActive(true);

        if (_optionPanel != null)
            _optionPanel.SetActive(false);

        if (_menuContent != null && isPaused)
        {
            UIMove(_showPos);
            UIAnimator.FadeIn(_canvasGroup, _time);
            Debug.Log("Me activo Menu");
        }
        AudioListener.pause = true;
    }
    public void ResumeGame()
    {
        isPaused = false;

        if (_menuContent != null)
        {
            UIMove(_hidePos);
            UIAnimator.FadeOut(_canvasGroup, _time);
        }
        AudioListener.pause= false;

        if(_closeCoroutine != null)
        {
            StopCoroutine(_closeCoroutine);
            _closeCoroutine = null;
        }
        _closeCoroutine = StartCoroutine(ChangeMode());
    }

    public void ShowOptionsPanel()
    {
        if (_pauseMainPanel != null)
            _pauseMainPanel.SetActive(false);

        if (_optionPanel != null)
            _optionPanel.SetActive(true);
    }

    public void ShowPauseMenuPanel()
    {
        if (_optionPanel != null)
            _optionPanel.SetActive(false);

        if (_pauseMainPanel != null)
            _pauseMainPanel.SetActive(true);

    }

    private void OnDestroy()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(ResumeGame);

        if (_optionButton != null)
            _optionButton.onClick.RemoveListener(ShowOptionsPanel);

        if (_backOptionButton != null)
            _backOptionButton.onClick.RemoveListener(ShowPauseMenuPanel);
    }
    
    public void UIMove(Vector2 target)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        _moveCoroutine = StartCoroutine(UIAnimator.PanelUIMove(_panelUI, target, _time));
    }
    public IEnumerator ChangeMode()
    {
        yield return new WaitForSeconds(_time);

        if (_menuContent != null)
            _menuContent.SetActive(false);

        _menuContent.SetActive(false);
    }
}
