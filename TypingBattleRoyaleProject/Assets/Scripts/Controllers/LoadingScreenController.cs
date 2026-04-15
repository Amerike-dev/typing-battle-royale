using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Texto y Slider")]
    public TextMeshProUGUI estadoTxt;
    public Slider sliderLoading;

    private int paso = 1;
    private float objetivo = 0f;
    private float velocidad = 0.5f;
    [Header("Variables de prueba")]
    public bool checkServer;
    public bool checkPlayers;
    public bool checkMap;
    public bool checkLoot;
    void Start()
    {
        sliderLoading.value = 0;
    }

    void Update()
    {
        sliderLoading.value = Mathf.MoveTowards(
            sliderLoading.value,
            objetivo,
            velocidad * Time.deltaTime
        );

        if (sliderLoading.value >= objetivo)
        {
            ProcesarPaso();
        }
    }

    void ProcesarPaso()
    {
        switch (paso)
        {
            case 1:
                ChecarServidor();
                break;

            case 2:
                ChecarJugadores();
                break;

            case 3:
                ChecarMapa();
                break;

            case 4:
                ChecarLoot();
                break;

            case 5:
                estadoTxt.text = "Listo";
                SceneManager.LoadScene("GameplayScene");
                break;
        }
    }

    public void ChecarServidor()
    {
        if (!checkServer)
        {
            estadoTxt.text = "Cargando servidor";
            return;
        }

        estadoTxt.text = "Servidor Cargado";
        objetivo = 0.25f;
        paso++;
    }

    public void ChecarJugadores()
    {
        if (!checkPlayers)
        {
            estadoTxt.text = "Cargando jugadores";
            return;
        }

        estadoTxt.text = "Jugadores cargados";
        objetivo = 0.5f;
        paso++;
    }

    public void ChecarMapa()
    {
        if (!checkMap)
        {
            estadoTxt.text = "Cargando mapa";
            return;
        }
        estadoTxt.text = "Mapa cargado";
        objetivo = 0.75f;
        paso++;
    }

    public void ChecarLoot()
    {
        if (!checkLoot)
        {
            estadoTxt.text = "Cargando loot";
            return;
        }
        estadoTxt.text = "Loot cargado";
        objetivo = 1f;
        paso++;
    }
}