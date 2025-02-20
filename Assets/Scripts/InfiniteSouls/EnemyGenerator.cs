using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject t800Prefab, t200Prefab;

    private List<GameObject> Enemies;

    public void GenerateEnemies(List<List<(GameObject, int, int)>> Layers, int MinEnemiesPerPlatform, float PlatformEnemyDensity, float t800Probability)
    {
        // Lista que almacenará los enemigos generados
        Enemies = new List<GameObject>();

        // Variables para llevar el seguimiento de la capa y la plataforma actual
        int CurrentLayer = -1, CurrentPlatform = -1;

        // Itera sobre cada capa de plataformas
        foreach (List<(GameObject, int, int)> Layer in Layers)
        {
            CurrentLayer++; // Aumenta el índice de la capa
            CurrentPlatform = -1;

            // Itera sobre cada plataforma dentro de la capa
            foreach ((GameObject, int, int) Platform in Layer)
            {
                CurrentPlatform++; // Aumenta el índice de la plataforma

                // Renombra el objeto de la plataforma con su capa y número de plataforma
                Platform.Item1.name = "Layer " + CurrentLayer + " Platfrom " + CurrentPlatform;

                // Calcula el número máximo de enemigos permitidos en la plataforma 
                int MaxEnemies = Mathf.CeilToInt((Platform.Item2 + Platform.Item3) / 2);

                // Calcula la cantidad de enemigos a generar según la densidad especificada
                int NumEnemies = Mathf.Max(MinEnemiesPerPlatform, Mathf.FloorToInt(MaxEnemies * PlatformEnemyDensity));

                // Obtiene todos los bloques hijos de la plataforma y los almacena en una lista
                List<Transform> BlocksTransforms = new List<Transform>(Platform.Item1.GetComponentsInChildren<Transform>());
                BlocksTransforms.RemoveAt(0); // Elimina el transform principal de la plataforma

                // Calcula el intervalo entre posiciones donde se generarán los enemigos
                int Interval = Mathf.FloorToInt(BlocksTransforms.Count / NumEnemies);

                // Genera los enemigos en la plataforma
                for (int i = 0; i < NumEnemies; i++)
                {
                    GameObject EnemySpawned;

                    // Decide aleatoriamente si genera un enemigo tipo t800 o t200
                    if (Random.value < t800Probability)
                    {
                        // Instancia un enemigo t800 en la posición correspondiente
                        EnemySpawned = Instantiate(t800Prefab, new Vector3(BlocksTransforms[i * Interval].position.x, BlocksTransforms[i * Interval].position.y + 1, BlocksTransforms[i * Interval].position.z), Quaternion.identity);
                        EnemySpawned.name = "Layer " + CurrentLayer + " Platfrom " + CurrentPlatform + "t800 " + i;
                    }
                    else
                    {
                        // Instancia un enemigo t200 en la posición correspondiente
                        EnemySpawned = Instantiate(t200Prefab, new Vector3(BlocksTransforms[i * Interval].position.x, BlocksTransforms[i * Interval].position.y + 1, BlocksTransforms[i * Interval].position.z), Quaternion.identity);
                        EnemySpawned.name = "Layer " + CurrentLayer + " Platfrom " + CurrentPlatform + "t200 " + i;
                    }

                    // Deshabilita al enemigo recién generado para activarlo posteriormente en el juego
                    EnemyEnabler.DisableEnemy(EnemySpawned);
                    EnemySpawned.SetActive(false);

                    // Agrega el enemigo a la lista de enemigos generados
                    Enemies.Add(EnemySpawned);
                }
            }
        }
    }


    public List<GameObject> GetGeneratedEnemies()
    {
        return Enemies;
    }

    public void DeleteEnemies()
    {
        foreach (GameObject Enemy in Enemies)
        {
            DestroyImmediate(Enemy);
        }
    }
}
