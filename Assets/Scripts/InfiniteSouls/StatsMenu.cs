using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StatsMenu : MonoBehaviour
{
    private InfiniteSoulsManager InfiniteSoulsManager;

    [SerializeField]
    private TextMeshProUGUI CombatDurationText, AirTimeText, DamageRecievedText, HealthHealedText, AccuracyText, GravityChangesText, DoubleJumpsText, DashesText, RoomCRText, NextRoomCRText, CurrentRoomNumber;

    [SerializeField] 
    private Texture2D CustomCursor;

    [SerializeField]
    private GameObject HUD, StatsAudio;

    private bool IsShowing = false;

    private void Start()
    {
        InfiniteSoulsManager = FindObjectOfType<InfiniteSoulsManager>();
    }

    //Método que modifica el texto de StatsMenu para mostrar las estadísticas que se le proporcionan
    public void ShowStats(float CombatDurationValue, float AirTimeValue, float AccuracyValue, int DamageRecievedValue, int HealthHealedValue, int GravityChangesValue, int DoubleJumpsValue, int DashesValue, int RoomNumber, InfiniteSoulsManager.RoomCR RoomCRValue)
    {
        IsShowing = true;
        HUD.SetActive(false);
        if (CombatDurationValue <= 60)
        {
            CombatDurationText.text = (Mathf.Round(CombatDurationValue)).ToString() + " segundos";
        }
        else
        {
            int Minutes = Mathf.FloorToInt(CombatDurationValue / 60);
            float Seconds = Mathf.Round(CombatDurationValue % 60); 
            CombatDurationText.text = Minutes + ":" + (Seconds < 10 ? "0" + Seconds : Seconds) + " minutos";
        }
        if (AirTimeValue <= 60)
        {
            AirTimeText.text = (Mathf.Round(AirTimeValue)).ToString() + " segundos (" + Mathf.Round(AirTimeValue / CombatDurationValue * 100) + "%)";
        }
        else
        {
            int Minutes = Mathf.FloorToInt(AirTimeValue / 60);
            float Seconds = Mathf.Round(AirTimeValue % 60);
            AirTimeText.text = Minutes + ":" + (Seconds < 10 ? "0" + Seconds : Seconds) + " minutos (" + Mathf.Round(AirTimeValue / CombatDurationValue * 100) + "%)";
        }
        AccuracyText.text = AccuracyValue.ToString() + "%";
        DamageRecievedText.text = DamageRecievedValue.ToString() + " PG";
        HealthHealedText.text = HealthHealedValue.ToString() + " PG";
        GravityChangesText.text = GravityChangesValue.ToString();
        DoubleJumpsText.text = DoubleJumpsValue.ToString();
        DashesText.text = DashesValue.ToString();
        CurrentRoomNumber.text = "Sala " + RoomNumber.ToString() + " completada en dificultad:";
        switch (RoomCRValue)
        {
            case InfiniteSoulsManager.RoomCR.Easy:
                RoomCRText.text = "Facil";
                break;
            case InfiniteSoulsManager.RoomCR.Medium:
                RoomCRText.text = "Media";
                break;
            case InfiniteSoulsManager.RoomCR.Hard:
                RoomCRText.text = "Dificil";
                break;
            case InfiniteSoulsManager.RoomCR.GodsScar:
                RoomCRText.text = "Cicatriz de los dioses";
                break;
            default:
                break;
        }
    }

    public void SetNextRoomCR(InfiniteSoulsManager.RoomCR NextRoomCRValue)
    {
        switch (NextRoomCRValue)
        {
            case InfiniteSoulsManager.RoomCR.Easy:
                NextRoomCRText.text = "Facil";
                break;
            case InfiniteSoulsManager.RoomCR.Medium:
                NextRoomCRText.text = "Media";
                break;
            case InfiniteSoulsManager.RoomCR.Hard:
                NextRoomCRText.text = "Dificil";
                break;
            case InfiniteSoulsManager.RoomCR.GodsScar:
                NextRoomCRText.text = "Cicatriz de los dioses";
                break;
            default:
                break;
        }

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Cursor.SetCursor(CustomCursor, Vector2.zero, CursorMode.Auto);
    }

    public void ContinueToNextRoom()
    {
        InfiniteSoulsManager.GenerateNextRoom(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        HUD.SetActive(true);
        this.gameObject.SetActive(false);
        IsShowing = false;
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void HoverButton()
    {
        StatsAudio.GetComponents<AudioSource>()[0].Play();
    }

    public void ClickButton()
    {
        StatsAudio.GetComponents<AudioSource>()[1].Play();
    }

    public bool AreStatsShowing()
    {
        return IsShowing;
    }
}
