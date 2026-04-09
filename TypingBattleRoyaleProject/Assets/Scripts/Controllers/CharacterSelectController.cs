using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectController : MonoBehaviour
{
    public Image[] slots; // Los 4 slots
    public Button confirmButton;

    private int selectedIndex = -1;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;

    private void Start()
    {
        confirmButton.interactable = false;

        // Asignar listeners a cada slot
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            slots[i].GetComponent<Button>().onClick.AddListener(() => SelectCharacter(index));
        }
    }

    public void SelectCharacter(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == index) ? selectedColor : normalColor;
        }

        confirmButton.interactable = true;
    }

    public void ConfirmSelection()
    {
        PlayerPrefs.SetInt("SelectedCharacter", selectedIndex);
        PlayerPrefs.Save();

        SceneLoader.LoadScene("GameplayScene");
    }

    public void GoBack()
    {
        SceneLoader.LoadScene("MainMenu");
    }
}
