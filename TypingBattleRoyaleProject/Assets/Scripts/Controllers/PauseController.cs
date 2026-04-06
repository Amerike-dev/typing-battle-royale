using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _menuContent;
    [SerializeField] private GameplayManager _gameplayManager;

    [Header("Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    private bool isPaused = false;

    private void Awake()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(ResumeGame);

        if (_restartButton != null)
            _restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneLoader.Reload();
            });

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneLoader.LoadScene("MainMenu");
            });
    }

    private void Start()
    {
        if (_menuContent != null)
            _menuContent.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (_gameplayManager != null && _gameplayManager.stateMachine != null)

            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsGameOverActive())
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        isPaused = true;

        if (_menuContent != null)
            _menuContent.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (_menuContent != null)
            _menuContent.SetActive(false);

        Time.timeScale = 1f;
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

        Time.timeScale = 1f;
    }
}
