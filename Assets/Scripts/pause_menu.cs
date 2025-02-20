using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class pause_menu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject HUD;

    [SerializeField] GameObject buttonsControls;
    [SerializeField] GameObject buttonsPause;
    [SerializeField] GameObject buttonsOptions;
    [SerializeField] GameObject buttonsRespawn;
    [SerializeField] GameObject respawnBackground;

    [SerializeField] TextMeshProUGUI volumeValue;
    [SerializeField] TextMeshProUGUI sensValueX;
    [SerializeField] TextMeshProUGUI sensValueY;

    [SerializeField] public Texture2D cursor;
    [SerializeField] public AudioMixer mainMixer;
    [SerializeField] public GameObject menuAudio;
    [SerializeField] public GameObject gameMusic;

    [Header("Sliders")]
    [SerializeField] public Slider sliderVolume;
    [SerializeField] public Slider sliderX;
    [SerializeField] public Slider sliderY;

    [SerializeField] public GameObject player;

    [SerializeField]
    private StatsMenu StatsMenu;

    bool wasInCombat;

    bool isPaused;
    bool playerDead;
    // Start is called before the first frame update
    void Start()
    {
        sliderVolume.onValueChanged.AddListener(
            (newVolume) =>
            {
                volumeValue.text = (((newVolume+80)/8)*10).ToString("0");
            }
        );
        sliderX.onValueChanged.AddListener(
            (newSensX) =>
            {
                sensValueX.text = newSensX.ToString("0");
            }
        );
        sliderY.onValueChanged.AddListener(
            (newSensY) =>
            {
                sensValueY.text = newSensY.ToString("0");
            }
        );
        isPaused = false;
        playerDead = false;
        wasInCombat = false;
        pauseMenu.SetActive(isPaused);
        HUD.SetActive(!isPaused);
        //StatsMenu = FindObjectOfType<StatsMenu>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDead && !StatsMenu.AreStatsShowing())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                Time.timeScale = isPaused ? 0 : 1;
                pauseMenu.SetActive(isPaused);
                HUD.SetActive(!isPaused);
                if (isPaused)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
                    menuAudio.GetComponents<AudioSource>()[1].Play();
                }
                else
                {
                    CloseOptions();
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            if (isPaused && Input.GetMouseButtonDown(0))
            {
                buttonsControls.SetActive(false);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R) && isPaused)
            {
                RespawnPlayer();
            }
        }
    }

    public bool GetIsPaused() { return isPaused; }

    public void ExitGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void OpenOptions()
    {
        buttonsPause.SetActive(false);
        buttonsOptions.SetActive(true);
    }

    public void CloseOptions()
    {
        buttonsPause.SetActive(true);
        buttonsOptions.SetActive(false);
    }

    public void OpenControls()
    {
        buttonsControls.SetActive(true);
    }

    public void HoverButton()
    {
        menuAudio.GetComponents<AudioSource>()[0].Play();
    }

    public void ClickButton()
    {
        menuAudio.GetComponents<AudioSource>()[1].Play();
    }



    //OPTIONS METHODS
    public void SetVolume (float volume)
    {
        mainMixer.SetFloat("MainVolume",volume);
    }

    //RESPAWN METHODS
    public void RespawnPlayer()
    {
        gameMusic.GetComponents<AudioSource>()[1].enabled = wasInCombat;
        playerDead = false;
        isPaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        HUD.SetActive(true);
        buttonsPause.SetActive(true);
        respawnBackground.SetActive(false);
        buttonsRespawn.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.GetComponent<player_combat>().Respawn();
    }

    public void RespawnMenu()
    {
        wasInCombat = gameMusic.GetComponents<AudioSource>()[1].enabled;
        gameMusic.GetComponents<AudioSource>()[1].enabled = false;
        //Invoke(nameof(DeathSound), 0.1f);
        playerDead = true;
        isPaused = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        HUD.SetActive(false);
        buttonsPause.SetActive(false);
        buttonsControls.SetActive(false);
        respawnBackground.SetActive(true);
        buttonsRespawn.SetActive(true);
        menuAudio.GetComponents<AudioSource>()[2].Play();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }

    public void ReloadInfiniteSoulsScene()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("InfiniteSouls");
    }

    //private void DeathSound()
    //{

    //}

}
