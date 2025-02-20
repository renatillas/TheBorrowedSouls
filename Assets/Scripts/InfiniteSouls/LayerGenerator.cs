using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class LayerGenerator : MonoBehaviour
{

    [SerializeField]
    PlatformGenerator PlatformGenerator;

    [SerializeField]
    private GameObject BlockPrefab;

    private int BlockSize;

    private List<List<(GameObject, int, int)>> Layers;

    public void GenerateLayersAtPostition(Vector3 Position, int LayerAmount, int SeparationBetweenLayersInUnits, int SpaceWidthInUnits, int SpaceHeightInUnits, int PartitionMinSizeInUnits, int PlatformMinSizeInUnits, int MaxPosibleDivisions, float BoundariesProbability)
    {
        BlockSize = (int)BlockPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
        Layers = new List<List<(GameObject, int, int)>>();

        PlatformGenerator.InitiateValuesInUnits(SpaceWidthInUnits, SpaceHeightInUnits, PartitionMinSizeInUnits, PlatformMinSizeInUnits, MaxPosibleDivisions, BoundariesProbability);

        // Coloca el generador de plataformas en la posición inicial especificada
        PlatformGenerator.gameObject.transform.position = Position;

        // Genera la cantidad especificada de capas de plataformas
        for (int CurrentLayer = 1; CurrentLayer <= LayerAmount; CurrentLayer++)
        {
            // Calcula la posición de la nueva capa en base a la separación y el tamaño de bloque
            Vector3 LayerPosition = new Vector3(Position.x, Position.y + CurrentLayer * SeparationBetweenLayersInUnits * BlockSize, Position.z);

            // Genera plataformas en la posición calculada para la capa actual
            PlatformGenerator.GeneratePlatformsAtPosition(LayerPosition);

            // Agrega las plataformas generadas a la lista de capas
            Layers.Add(PlatformGenerator.GetGeneratedPlatforms());
        }

        // Reconstruye la malla de navegación tras generar las plataformas
        PlatformGenerator.gameObject.GetComponent<NavMeshSurface>().RemoveData();
        PlatformGenerator.gameObject.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public List<List<(GameObject, int, int)>> GetGeneratedLayers()
    {
        return Layers;
    }

    public void DeleteLayers()
    {
        foreach (List<(GameObject, int, int)> Layer in Layers)
        {
            foreach ((GameObject, int, int) Platform in Layer)
            {
                DestroyImmediate(Platform.Item1);
            }
        }
    }
}
