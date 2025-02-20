using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class InfiniteSoulsManager : MonoBehaviour
{
    //Los parámetros que se pueden modificar desde el inspector de Unity están en este primer apartado

    [Header("Referencias a objetos")]
    [SerializeField]
    private RoomGenerator RoomGenerator;

    [SerializeField]
    private GameObject Player;

    [SerializeField]
    private GameObject BlockPrefab;

    [SerializeField]
    private GameObject Altar, Portal, Music, InfiniteSoulsStatsMenu, RespawnCompletedRoomsText;

    [Header("Pesos de la heurística")]
    [SerializeField]
    private float CombatDurationWeight;
    [SerializeField]
    private float AirTimeWeight, DamageAndHealWeight, AccuracyWeight, GravityChangesWeight, DoubleJumpsWeight, DashesWeight;

    [Header("Valores para normalización procedural para Easy")]
    [SerializeField]
    private float MinTimePerEnemyEasy;
    [SerializeField]
    private float MaxTimePerEnemyEasy, MinPercentageOfMaxHealEasy, MaxPercentageOfMaxHealEasy;

    [Header("Valores para normalización procedural para Medium")]
    [SerializeField]
    private float MinTimePerEnemyMedium;
    [SerializeField]
    private float MaxTimePerEnemyMedium, MinPercentageOfMaxHealMedium, MaxPercentageOfMaxHealMedium;

    [Header("Valores para normalización procedural para Hard")]
    [SerializeField]
    private float MinTimePerEnemyHard;
    [SerializeField]
    private float MaxTimePerEnemyHard, MinPercentageOfMaxHealHard, MaxPercentageOfMaxHealHard;

    [Header("Valores para normalización procedural para GodsScar")]
    [SerializeField]
    private float MinTimePerEnemyGodsScar;
    [SerializeField]
    private float MaxTimePerEnemyGodsScar, MinPercentageOfMaxHealGodsScar, MaxPercentageOfMaxHealGodsScar;

    [Header("Rangos de normalización genérica para Easy")]
    [SerializeField]
    private float EasyMinAirTimePercentage;
    [SerializeField]
    private float EasyMaxAirTimePercentage, EasyMinAccuracy, EasyMaxAccuracy, 
                  EasyMinGravityChanges, EasyMaxGravityChanges, EasyMinDoubleJumps, EasyMaxDoubleJumps, EasyMinDashes, EasyMaxDashes;

    [Header("Rangos de normalización genérica para Medium")]
    [SerializeField]
    private float MediumMinAirTimePercentage;
    [SerializeField]
    private float MediumMaxAirTimePercentage, MediumMinAccuracy, MediumMaxAccuracy,
                  MediumMinGravityChanges, MediumMaxGravityChanges, MediumMinDoubleJumps, MediumMaxDoubleJumps, MediumMinDashes, MediumMaxDashes;

    [Header("Rangos de normalización genérica para Hard")]
    [SerializeField]
    private float HardMinAirTimePercentage;
    [SerializeField]
    private float HardMaxAirTimePercentage, HardMinAccuracy, HardMaxAccuracy,
                  HardMinGravityChanges, HardMaxGravityChanges, HardMinDoubleJumps, HardMaxDoubleJumps, HardMinDashes, HardMaxDashes;

    [Header("Rangos de normalización genérica para Gods' Scar")]
    [SerializeField]
    private float GodsScarMinAirTimePercentage;
    [SerializeField]
    private float GodsScarMaxAirTimePercentage, GodsScarMinAccuracy, GodsScarMaxAccuracy,
                  GodsScarMinGravityChanges, GodsScarMaxGravityChanges, GodsScarMinDoubleJumps, GodsScarMaxDoubleJumps, GodsScarMinDashes, GodsScarMaxDashes;


    [Header("Rangos de generación para Easy")]
    [SerializeField]
    private int MinRoomHeightEasy;
    [SerializeField]
    private int MaxRoomHeightEasy, MinRoomWidthEasy, MaxRoomWidthEasy, MinRoomLengthEasy, MaxRoomLengthEasy, MinMinPartitionSizeEasy, MaxMinPartitionSizeEasy, MinMinPlatformSizeEasy, MaxMinPlatformSizeEasy,
                MinMaxDivisionsEasy, MaxMaxDivisonsEasy, MinLayersEasy, MinMinEnemiesEasy, MaxMinEnemiesEasy;
    [SerializeField]
    private float MinApplyBoundariesEasy, MaxApplyBoundariesEasy, MinEnemyDensityEasy, MaxEnemyDensityEasy, MinT800ProbabilityEasy, MaxT800ProbabilityEasy;

    [Header("Rangos de generación para Medium")]
    [SerializeField]
    private int MinRoomHeightMedium;
    [SerializeField]
    private int MaxRoomHeightMedium, MinRoomWidthMedium, MaxRoomWidthMedium, MinRoomLengthMedium, MaxRoomLengthMedium, MinMinPartitionSizeMedium, MaxMinPartitionSizeMedium, MinMinPlatformSizeMedium, MaxMinPlatformSizeMedium,
                MinMaxDivisionsMedium, MaxMaxDivisonsMedium, MinLayersMedium, MinMinEnemiesMedium, MaxMinEnemiesMedium;
    [SerializeField]
    private float MinApplyBoundariesMedium, MaxApplyBoundariesMedium, MinEnemyDensityMedium, MaxEnemyDensityMedium, MinT800ProbabilityMedium, MaxT800ProbabilityMedium;

    [Header("Rangos de generación para Hard")]
    [SerializeField]
    private int MinRoomHeightHard;
    [SerializeField]
    private int MaxRoomHeightHard, MinRoomWidthHard, MaxRoomWidthHard, MinRoomLengthHard, MaxRoomLengthHard, MinMinPartitionSizeHard, MaxMinPartitionSizeHard, MinMinPlatformSizeHard, MaxMinPlatformSizeHard,
                MinMaxDivisionsHard, MaxMaxDivisonsHard, MinLayersHard, MinMinEnemiesHard, MaxMinEnemiesHard;
    [SerializeField]
    private float MinApplyBoundariesHard, MaxApplyBoundariesHard, MinEnemyDensityHard, MaxEnemyDensityHard, MinT800ProbabilityHard, MaxT800ProbabilityHard;

    [Header("Rangos de generación para Gods' Scar")]
    [SerializeField]
    private int MinRoomHeightGodsScar;
    [SerializeField]
    private int MaxRoomHeightGodsScar, MinRoomWidthGodsScar, MaxRoomWidthGodsScar, MinRoomLengthGodsScar, MaxRoomLengthGodsScar, MinMinPartitionSizeGodsScar, MaxMinPartitionSizeGodsScar, MinMinPlatformSizeGodsScar, MaxMinPlatformSizeGodsScar,
                MinMaxDivisionsGodsScar, MaxMaxDivisonsGodsScar, MinLayersGodsScar, MinMinEnemiesGodsScar, MaxMinEnemiesGodsScar;
    [SerializeField]
    private float MinApplyBoundariesGodsScar, MaxApplyBoundariesGodsScar, MinEnemyDensityGodsScar, MaxEnemyDensityGodsScar, MinT800ProbabilityGodsScar, MaxT800ProbabilityGodsScar;

    //Todos los siguientes parámetros no son accesibles desde el inspector de Unity


    //Enum para la representar las diferentes dificultades
    public enum RoomCR
    {
        Easy,
        Medium,
        Hard,
        GodsScar
    }

    private RoomCR CurrentRoomCR = RoomCR.Hard; //La dificultad de la sala actual (Easy por default para empezar desde cero cada vez que se inicia)

    private int CurrentRoomNumber = 0; //El número de la sala actual

    //Rangos de normalización para la evaluación del jugador
    private float MinCombatDuration, MaxCombatDuration; //Rangos de tiempo
    private int MinDamageAndHeal, MaxDamageAndHeal; //Rangos de curación

    //Datos de comportamiento del jugador
    private float CombatDuration = 0, AirTime = 0; //Datos de tiempo
    private int DamageTaken = 0, HealthHealed = 0; //Datos de curación
    private int ShotsFired = 0, ShotsHit = 0; //Datos de precisión
    private int GravityChanges = 0, DoubleJumps = 0, Dashes = 0; //Datos de acciones de movimiento

    //Variables extra para funcionamiento
    private int EnemyAmount = 0; //Cantidad de enemigos para llevar conteo y saber cuando ha acabado el combate
    private bool InCombat = false; //Booleano para saber cuando se está en combate
    private int BlockSize; //Tamaño del bloque de construcción

    void Start()
    {
        BlockSize = (int)BlockPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
        GenerateNextRoom(false);
    }

    private void Update()
    {
        if (InCombat)
        {
            CombatDuration += Time.deltaTime;
            if (!Player.GetComponent<player_movement>().IsPlayerGrounded())
            {
                AirTime += Time.deltaTime;
            }
        }
    }

    public void GenerateNextRoom(bool DeletePrevious)
    {
        //Eliminar la sala anterior
        if (DeletePrevious) RoomGenerator.DeletePreviousRoom();

        int RoomHeight = 0;
        int RoomWidth = 0;
        int RoomLength = 0;
        int MinPartitionSize = 0;
        int MinPlatformSize = 0;
        int MaxAllowedDivisions = 0;
        float ApplyBoundaries = 0;
        int LayerAmount = 0;
        int BetweenLayersSpaceInUnits = 0;
        int MinimunEnemies = 0;
        float PlatformEnemyDensity = 0, t800SpawnPropability = 0;

        //Generar los parámetros de generación en cada dificultad
        switch (CurrentRoomCR)
        {
            case RoomCR.Easy:
                //Generar los tamaños de la sala en Easy
                RoomHeight = UnityEngine.Random.Range(MinRoomHeightEasy, MaxRoomHeightEasy + 1);
                RoomWidth = UnityEngine.Random.Range(MinRoomWidthEasy, MaxRoomWidthEasy + 1);
                RoomLength = UnityEngine.Random.Range(MinRoomLengthEasy, MaxRoomLengthEasy + 1);

                //Generar los parámetros de las plataformas en Easy
                MinPartitionSize = UnityEngine.Random.Range(MinMinPartitionSizeEasy, MaxMinPartitionSizeEasy + 1);
                MinPlatformSize = UnityEngine.Random.Range(MinMinPlatformSizeEasy, MaxMinPlatformSizeEasy + 1);
                MaxAllowedDivisions = UnityEngine.Random.Range(MinMaxDivisionsEasy, MaxMaxDivisonsEasy + 1);
                ApplyBoundaries = UnityEngine.Random.Range(MinApplyBoundariesEasy, MaxApplyBoundariesEasy);

                //Generar los parámetros de las capas en Easy
                LayerAmount = UnityEngine.Random.Range(Mathf.Min(MinLayersEasy, RoomHeight - 1), RoomHeight);
                BetweenLayersSpaceInUnits = Mathf.Max(1, Mathf.FloorToInt((RoomHeight - 2) / Mathf.Max(LayerAmount, 2)));

                //Generar los parámetros para los enemigos en Easy
                MinimunEnemies = UnityEngine.Random.Range(MinMinEnemiesEasy, MaxMinEnemiesEasy + 1);
                PlatformEnemyDensity = UnityEngine.Random.Range(MinEnemyDensityEasy, MaxEnemyDensityEasy);
                t800SpawnPropability = UnityEngine.Random.Range(MinT800ProbabilityEasy, MaxT800ProbabilityEasy);

                break;

            case RoomCR.Medium:
                //Generar los tamaños de la sala en Medium
                RoomHeight = UnityEngine.Random.Range(MinRoomHeightMedium, MaxRoomHeightMedium + 1);
                RoomWidth = UnityEngine.Random.Range(MinRoomWidthMedium, MaxRoomWidthMedium + 1);
                RoomLength = UnityEngine.Random.Range(MinRoomLengthMedium, MaxRoomLengthMedium + 1);

                //Generar los parámetros de las plataformas en Medium
                MinPartitionSize = UnityEngine.Random.Range(MinMinPartitionSizeMedium, MaxMinPartitionSizeMedium + 1);
                MinPlatformSize = UnityEngine.Random.Range(MinMinPlatformSizeMedium, MaxMinPlatformSizeMedium + 1);
                MaxAllowedDivisions = UnityEngine.Random.Range(MinMaxDivisionsMedium, MaxMaxDivisonsMedium + 1);
                ApplyBoundaries = UnityEngine.Random.Range(MinApplyBoundariesMedium, MaxApplyBoundariesMedium);

                //Generar los parámetros de las capas en Medium
                LayerAmount = UnityEngine.Random.Range(Mathf.Min(MinLayersMedium, RoomHeight - 1), RoomHeight);
                BetweenLayersSpaceInUnits = Mathf.Max(1, Mathf.FloorToInt((RoomHeight - 2) / Mathf.Max(LayerAmount, 2)));

                //Generar los parámetros para los enemigos en Medium
                MinimunEnemies = UnityEngine.Random.Range(MinMinEnemiesMedium, MaxMinEnemiesMedium + 1);
                PlatformEnemyDensity = UnityEngine.Random.Range(MinEnemyDensityMedium, MaxEnemyDensityMedium);
                t800SpawnPropability = UnityEngine.Random.Range(MinT800ProbabilityMedium, MaxT800ProbabilityMedium);

                break;

            case RoomCR.Hard:
                //Generar los tamaños de la sala en Hard
                RoomHeight = UnityEngine.Random.Range(MinRoomHeightHard, MaxRoomHeightHard + 1);
                RoomWidth = UnityEngine.Random.Range(MinRoomWidthHard, MaxRoomWidthHard + 1);
                RoomLength = UnityEngine.Random.Range(MinRoomLengthHard, MaxRoomLengthHard + 1);

                //Generar los parámetros de las plataformas en Hard
                MinPartitionSize = UnityEngine.Random.Range(MinMinPartitionSizeHard, MaxMinPartitionSizeHard + 1);
                MinPlatformSize = UnityEngine.Random.Range(MinMinPlatformSizeHard, MaxMinPlatformSizeHard + 1);
                MaxAllowedDivisions = UnityEngine.Random.Range(MinMaxDivisionsHard, MaxMaxDivisonsHard + 1);
                ApplyBoundaries = UnityEngine.Random.Range(MinApplyBoundariesHard, MaxApplyBoundariesHard);

                //Generar los parámetros de las capas en Hard
                LayerAmount = UnityEngine.Random.Range(Mathf.Min(MinLayersHard, RoomHeight - 1), RoomHeight);
                BetweenLayersSpaceInUnits = Mathf.Max(1, Mathf.FloorToInt((RoomHeight - 2) / Mathf.Max(LayerAmount, 2)));

                //Generar los parámetros para los enemigos en Hard
                MinimunEnemies = UnityEngine.Random.Range(MinMinEnemiesHard, MaxMinEnemiesHard + 1);
                PlatformEnemyDensity = UnityEngine.Random.Range(MinEnemyDensityHard, MaxEnemyDensityHard);
                t800SpawnPropability = UnityEngine.Random.Range(MinT800ProbabilityHard, MaxT800ProbabilityHard);

                break;

            case RoomCR.GodsScar:
                //Generar los tamaños de la sala en GodsScar
                RoomHeight = UnityEngine.Random.Range(MinRoomHeightGodsScar, MaxRoomHeightGodsScar + 1);
                RoomWidth = UnityEngine.Random.Range(MinRoomWidthGodsScar, MaxRoomWidthGodsScar + 1);
                RoomLength = UnityEngine.Random.Range(MinRoomLengthGodsScar, MaxRoomLengthGodsScar + 1);

                //Generar los parámetros de las plataformas en GodsScar
                MinPartitionSize = UnityEngine.Random.Range(MinMinPartitionSizeGodsScar, MaxMinPartitionSizeGodsScar + 1);
                MinPlatformSize = UnityEngine.Random.Range(MinMinPlatformSizeGodsScar, MaxMinPlatformSizeGodsScar + 1);
                MaxAllowedDivisions = UnityEngine.Random.Range(MinMaxDivisionsGodsScar, MaxMaxDivisonsGodsScar + 1);
                ApplyBoundaries = UnityEngine.Random.Range(MinApplyBoundariesGodsScar, MaxApplyBoundariesGodsScar);

                //Generar los parámetros de las capas en GodsScar
                LayerAmount = UnityEngine.Random.Range(Mathf.Min(MinLayersGodsScar, RoomHeight - 1), RoomHeight);
                BetweenLayersSpaceInUnits = Mathf.Max(1, Mathf.FloorToInt((RoomHeight - 2) / Mathf.Max(LayerAmount, 2)));

                //Generar los parámetros para los enemigos en GodsScar
                MinimunEnemies = UnityEngine.Random.Range(MinMinEnemiesGodsScar, MaxMinEnemiesGodsScar + 1);
                PlatformEnemyDensity = UnityEngine.Random.Range(MinEnemyDensityGodsScar, MaxEnemyDensityGodsScar);
                t800SpawnPropability = UnityEngine.Random.Range(MinT800ProbabilityGodsScar, MaxT800ProbabilityGodsScar);

                break;

            default:
                break;
        }

        //Asignación de los parámetros generados
        RoomGenerator.SetRoomSize(RoomWidth, RoomHeight, RoomLength);
        RoomGenerator.SetPlatfomGenerationParameters(MinPartitionSize, MinPlatformSize, MaxAllowedDivisions, ApplyBoundaries);
        RoomGenerator.SetLayerGenerationParameters(LayerAmount, BetweenLayersSpaceInUnits);
        RoomGenerator.SetEnemyGenerationParameters(MinimunEnemies, PlatformEnemyDensity, t800SpawnPropability);
        //Solicitud de generación de la sala
        RoomGenerator.GenerateRoomAtPosition(Vector3.zero);

        //Asignación de variables necesarias y colocación de objetos al sitio
        EnemyAmount = RoomGenerator.GetEnemyAmout();
        Player.GetComponent<player_movement>().SetOriginalRotation();
        Player.transform.position = new Vector3((float)(RoomWidth * BlockSize) / 2, 1, (float)(RoomLength * BlockSize) / 2);
        Altar.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, Player.transform.position.z + BlockSize / 4);
        Portal.transform.position = new Vector3(Player.transform.position.x, RoomHeight * BlockSize - 2, Player.transform.position.z);
        Altar.SetActive(true);
        Portal.SetActive(false);

        //Generar los rangos de normalización para la sala no dependientes de dificultad
        MaxDamageAndHeal = 2 * EnemyAmount; //La cantidad máxima de vide que puede recuperar un jugador al curarse es 2

        //Generar los rangos de normalización en cada dificultad
        switch (CurrentRoomCR)
        {
            case RoomCR.Easy:
                //Generar los rangos de normalización para la duración del combate en Easy
                MinCombatDuration = MinTimePerEnemyEasy * EnemyAmount;
                MaxCombatDuration = MaxTimePerEnemyEasy * EnemyAmount;
                MinDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MinPercentageOfMaxHealEasy);
                MaxDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MaxPercentageOfMaxHealEasy);
                break;

            case RoomCR.Medium:
                //Generar los rangos de normalización para la duración del combate en Medium
                MinCombatDuration = MinTimePerEnemyMedium * EnemyAmount;
                MaxCombatDuration = MaxTimePerEnemyMedium * EnemyAmount;
                MinDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MinPercentageOfMaxHealMedium);
                MaxDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MaxPercentageOfMaxHealMedium);
                break;

            case RoomCR.Hard:
                //Generar los rangos de normalización para la duración del combate en Hard
                MinCombatDuration = MinTimePerEnemyHard * EnemyAmount;
                MaxCombatDuration = MaxTimePerEnemyHard * EnemyAmount;
                MinDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MinPercentageOfMaxHealHard);
                MaxDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MaxPercentageOfMaxHealHard);
                break;

            case RoomCR.GodsScar:
                //Generar los rangos de normalización para la duración del combate en GodsScar
                MinCombatDuration = MinTimePerEnemyGodsScar * EnemyAmount;
                MaxCombatDuration = MaxTimePerEnemyGodsScar * EnemyAmount;
                MinDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MinPercentageOfMaxHealGodsScar);
                MaxDamageAndHeal = Mathf.FloorToInt(MaxDamageAndHeal * MaxPercentageOfMaxHealGodsScar);
                break;

            default:
                break;
        }

        //Asignamos el número de la sala que se acaba de completar al texto del respawn por si acaso y lo aumentamos para pasar a la siguiente.
        RespawnCompletedRoomsText.GetComponent<TextMeshProUGUI>().text = "Has completado " + CurrentRoomNumber + " salas";
        CurrentRoomNumber++;


        //Debug
        //Debug.Log("For " + EnemyAmount + " enemies:");
        //Debug.Log("Min Combat Duration " + MinCombatDuration);
        //Debug.Log("Max Combat Duration " + MaxCombatDuration);
        //Debug.Log("Min Damage and Heal " + MinDamageAndHeal);
        //Debug.Log("Max Damage and Heal " + MaxDamageAndHeal);
    }

    public void PlayerChangedGravity()
    {
        if (InCombat) GravityChanges++;
    }

    public void PlayerDoubleJumped()
    {
        if (InCombat) DoubleJumps++;
    }

    public void PlayerDashed()
    {
        if (InCombat) Dashes++;
    }

    public void PlayerTookDamage(int DamageAmount)
    {
        if (InCombat) DamageTaken += DamageAmount;
    }

    public void PlayerHealed(int HealAmount)
    {
        if (InCombat) HealthHealed += HealAmount;
    }

    public void PlayerShot()
    {
        if (InCombat) ShotsFired++;
    }

    public void PlayerHitTarget()
    {
        if (InCombat) ShotsHit++;
    }

    private void CalculateNextRoomCR()
    {
        //Inicializar variables para guardar los valores normalizados
        float CombatDurationNormalized = 0;
        float AirTimeNormalized = 0;
        float DamageAndHealNormalized = 0;
        float AccuracyNormalized = 0;
        float GravityChangesNormalized = 0;
        float DoubleJumpsNormalized = 0;
        float DashesNormalized = 0;

        switch (CurrentRoomCR)
        {
            case RoomCR.Easy:
                //Normalizar los datos recabados según lo esperado del jugador en Easy
                CombatDurationNormalized = NormalizeValue((1 / CombatDuration), (1 / MaxCombatDuration), (1 / MinCombatDuration));
                AirTimeNormalized = NormalizeValue(AirTime / CombatDuration, EasyMinAirTimePercentage, EasyMaxAirTimePercentage);
                DamageAndHealNormalized = NormalizeValue(HealthHealed - DamageTaken, MinDamageAndHeal, MaxDamageAndHeal);
                AccuracyNormalized = NormalizeValue((float)ShotsHit / ShotsFired, EasyMinAccuracy, EasyMaxAccuracy);
                GravityChangesNormalized = NormalizeValue(GravityChanges, EasyMinGravityChanges, EasyMaxGravityChanges);
                DoubleJumpsNormalized = NormalizeValue(DoubleJumps, EasyMinDoubleJumps, EasyMaxDoubleJumps);
                DashesNormalized = NormalizeValue(Dashes, EasyMinDashes, EasyMaxDashes);
                break;
            case RoomCR.Medium:
                //Normalizar los datos recabados según lo esperado del jugador en Medium
                CombatDurationNormalized = NormalizeValue((1 / CombatDuration), (1 / MaxCombatDuration), (1 / MinCombatDuration));
                AirTimeNormalized = NormalizeValue(AirTime / CombatDuration, MediumMinAirTimePercentage, MediumMaxAirTimePercentage);
                DamageAndHealNormalized = NormalizeValue(HealthHealed - DamageTaken, MinDamageAndHeal, MaxDamageAndHeal);
                AccuracyNormalized = NormalizeValue((float)ShotsHit / ShotsFired, MediumMinAccuracy, MediumMaxAccuracy);
                GravityChangesNormalized = NormalizeValue(GravityChanges, MediumMinGravityChanges, MediumMaxGravityChanges);
                DoubleJumpsNormalized = NormalizeValue(DoubleJumps, MediumMinDoubleJumps, MediumMaxDoubleJumps);
                DashesNormalized = NormalizeValue(Dashes, MediumMinDashes, MediumMaxDashes);
                break;
            case RoomCR.Hard:
                //Normalizar los datos recabados según lo esperado del jugador en Hard
                CombatDurationNormalized = NormalizeValue((1 / CombatDuration), (1 / MaxCombatDuration), (1 / MinCombatDuration));
                AirTimeNormalized = NormalizeValue(AirTime / CombatDuration, HardMinAirTimePercentage, HardMaxAirTimePercentage);
                DamageAndHealNormalized = NormalizeValue(HealthHealed - DamageTaken, MinDamageAndHeal, MaxDamageAndHeal);
                AccuracyNormalized = NormalizeValue((float)ShotsHit / ShotsFired, HardMinAccuracy, HardMaxAccuracy);
                GravityChangesNormalized = NormalizeValue(GravityChanges, HardMinGravityChanges, HardMaxGravityChanges);
                DoubleJumpsNormalized = NormalizeValue(DoubleJumps, HardMinDoubleJumps, HardMaxDoubleJumps);
                DashesNormalized = NormalizeValue(Dashes, HardMinDashes, HardMaxDashes);
                break;
            case RoomCR.GodsScar:
                //Normalizar los datos recabados según lo esperado del jugador en GodsScar
                CombatDurationNormalized = NormalizeValue((1 / CombatDuration), (1 / MaxCombatDuration), (1 / MinCombatDuration));
                AirTimeNormalized = NormalizeValue(AirTime / CombatDuration, GodsScarMinAirTimePercentage, GodsScarMaxAirTimePercentage);
                DamageAndHealNormalized = NormalizeValue(HealthHealed - DamageTaken, MinDamageAndHeal, MaxDamageAndHeal);
                AccuracyNormalized = NormalizeValue((float)ShotsHit / ShotsFired, GodsScarMinAccuracy, GodsScarMaxAccuracy);
                GravityChangesNormalized = NormalizeValue(GravityChanges, GodsScarMinGravityChanges, GodsScarMaxGravityChanges);
                DoubleJumpsNormalized = NormalizeValue(DoubleJumps, GodsScarMinDoubleJumps, GodsScarMaxDoubleJumps);
                DashesNormalized = NormalizeValue(Dashes, GodsScarMinDashes, GodsScarMaxDashes);
                break;
            default:
                break;
        }

        //Calcular las puntuaciones del jugador en cada dato
        float CombatDurationPerformance = CombatDurationWeight * CombatDurationNormalized; //A menor duración del combate, más puntuación.
        float AirTimePerformance = AirTimeWeight * AirTimeNormalized; //A más tiempo en el aire, mayor puntuación.
        float DamageAndHealPerformance = DamageAndHealWeight * DamageAndHealNormalized;
        float AccuracyPerformance = AccuracyWeight * AccuracyNormalized;
        float GravityChangesPerformance = GravityChangesWeight * GravityChangesNormalized;
        float DoubleJumpsPerformance = DoubleJumpsWeight * DoubleJumpsNormalized;
        float DashesPerformance = DashesWeight * DashesNormalized;

        //Calcular el rendimiento global del jugador
        float PlayerPerformance = CombatDurationPerformance + AirTimePerformance + DamageAndHealPerformance + AccuracyPerformance + GravityChangesPerformance + DoubleJumpsPerformance + DashesPerformance;

        //Aumentar o reducir la dificultad basado en los resultados del jugador
        switch (PlayerPerformance)
        {
            case < 0f:
                ReduceCR();
                break;
            case > 1f:
                IncreaseCR();
                break;
            default:
                break;
        }

        //Debug
        Debug.Log("PLAYER PERFOMANCE");
        Debug.Log("Overall: " + PlayerPerformance.ToString());
        Debug.Log("Combat duration: " + CombatDurationNormalized.ToString());
        Debug.Log("Air time: " + AirTimeNormalized.ToString());
        Debug.Log("Healing: " + DamageAndHealNormalized.ToString());
        Debug.Log("Acuracy: " + AccuracyNormalized.ToString());
        Debug.Log("Gravity changes: " + GravityChangesNormalized.ToString());
        Debug.Log("Double jumps: " + DoubleJumpsNormalized.ToString());
        Debug.Log("Dashes: " + DashesNormalized.ToString());
    }

    private float NormalizeValue(float Value, float Min, float Max)
    {
        return (Value - Min) / (Max - Min);
    }

    private void ReduceCR()
    {
        switch (CurrentRoomCR)
        {
            case RoomCR.Easy:
                break;
            case RoomCR.Medium:
                CurrentRoomCR = RoomCR.Easy;
                break;
            case RoomCR.Hard:
                CurrentRoomCR = RoomCR.Medium;
                break;
            case RoomCR.GodsScar:
                CurrentRoomCR = RoomCR.Hard;
                break;
            default:
                break;
        }
    }

    private void IncreaseCR()
    {
        switch (CurrentRoomCR)
        {
            case RoomCR.Easy:
                CurrentRoomCR = RoomCR.Medium;
                break;
            case RoomCR.Medium:
                CurrentRoomCR = RoomCR.Hard;
                break;
            case RoomCR.Hard:
                CurrentRoomCR = RoomCR.GodsScar;
                break;
            case RoomCR.GodsScar:
                break;
            default:
                break;
        }
    }

    public void BeginCombat()
    {
        CombatDuration = 0; AirTime = 0; //Reseteo de los datos de tiempo
        DamageTaken = 0; HealthHealed = 0; //Reseteo de los datos de vida
        ShotsFired = 0; ShotsHit = 0; //Reseteo de los datos de precisión
        GravityChanges = 0; DoubleJumps = 0; Dashes = 0; //Reseteo de los datos de acciones de movimiento

        Altar.SetActive(false);
        Portal.SetActive(false);
        RoomGenerator.ActivateEnemies();
        InCombat = true;
        Music.GetComponents<AudioSource>()[0].enabled = false;
        Music.GetComponents<AudioSource>()[1].enabled = true;
    }

    public void EnemyDead()
    {
        EnemyAmount--;
        if (EnemyAmount <= 0)
        {
            EndCombat();
        }
    }

    public void EndCombat()
    {
        InCombat = false;
        Portal.SetActive(true);
        Music.GetComponents<AudioSource>()[1].enabled = false;
        Music.GetComponents<AudioSource>()[0].enabled = true;
    }

    public void StatsScreen()
    {
        InfiniteSoulsStatsMenu.SetActive(true);
        float AccuracyPercentageForStats = (float)ShotsHit / ShotsFired;
        InfiniteSoulsStatsMenu.GetComponent<StatsMenu>().ShowStats(CombatDuration, AirTime, Mathf.Round(AccuracyPercentageForStats * 100), DamageTaken, HealthHealed, GravityChanges, DoubleJumps, Dashes, CurrentRoomNumber, CurrentRoomCR);
        CalculateNextRoomCR();
        InfiniteSoulsStatsMenu.GetComponent<StatsMenu>().SetNextRoomCR(CurrentRoomCR);
    }
}
