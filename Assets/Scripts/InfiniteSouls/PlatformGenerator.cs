using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

public class PlatformGenerator : MonoBehaviour
{
    private int Width, Height, BlockSize, MinPartitionSize, MinPlatformSizeInUnits; // Anchura, altura, tama�o del bloque, tama�o m�nimo de partici�n y tama�o m�nimo de plataforma

    private int MaxDivisions, CurrentDivisions; // M�ximas divisiones posibles y las divisiones que se han realizado hasta el momento

    private float ApplyBoundariesProbability = 0.55f; // Probabilidad de aplicar los bordes a la hora de generar plataformas

    [SerializeField]
    private GameObject BlockPrefab; // Prefab del bloque unitario para la construcci�n de plataformas

    [SerializeField]
    private GameObject PillarPrefab; // Prefab del pilar que sujeta las plataformas

    [SerializeField]
    private LayerMask LayerForPillarsToHit; //Capa con la que esperan colisionar los pilares para saber hasta donde instanciarse

    private List<RectInt> Partitions; // Lista de particiones generadas por BSP
    private List<(GameObject, int, int)> Platforms; // Lista de los GameObjects de las plataformas junto con sus dimensiones X e Y
    private List<GameObject> Blocks; // Lista de los GameObjects de los bloques instanciados

    private float PlatformsHeigth; // La altura a la que se han de generar las plataformas

    public void InitiateValuesInUnits(int SpaceWidthInUnits, int SpaceHeightInUnits, int PartitionMinSizeInUnits, int PlatformMinSizeInUnits, int MaxPosibleDivisions, float PlatformBoundariesPropability)
    {
        BlockSize = (int)BlockPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
        MaxDivisions = MaxPosibleDivisions;
        Width = SpaceWidthInUnits * BlockSize;
        Height = SpaceHeightInUnits * BlockSize;
        MinPartitionSize = PartitionMinSizeInUnits * BlockSize;
        MinPlatformSizeInUnits = PlatformMinSizeInUnits;
        ApplyBoundariesProbability = PlatformBoundariesPropability;
    }

    public void GeneratePlatformsAtPosition(Vector3 Position)
    {
        PlatformsHeigth = Position.y;
        Partitions = new List<RectInt>();
        Platforms = new List<(GameObject, int, int)>();
        Blocks = new List<GameObject>();
        CurrentDivisions = 0;
        DivideSpace(new RectInt((int)Position.x, (int)Position.z, Width, Height));
        GenerateBlocks();
    }

    public List<(GameObject, int, int)> GetGeneratedPlatforms()
    {
        return Platforms;
    }

    // M�todo para dividir el espacio usando BSP
    private void DivideSpace(RectInt space)
    {
        // Limitar las divisiones si se alcanzan las condiciones de parada
        if (CurrentDivisions >= MaxDivisions || (space.width <= MinPartitionSize && space.height <= MinPartitionSize))
        {
            Partitions.Add(space);
            return;
        }

        // Decidir aleatoriamente si dividir verticalmente u horizontalmente
        bool divideVertically = Random.value > 0.5f;

        // Si no se puede dividir en la direcci�n seleccionada, intentar la otra direcci�n
        if (divideVertically && space.width > MinPartitionSize * 2)
        {
            // Dividir verticalmente en una posici�n v�lida
            int splitX = Random.Range(MinPartitionSize, space.width - MinPartitionSize);
            splitX = (splitX / BlockSize);
            splitX *= BlockSize; // Alinear a m�ltiplo de BlockSize

            RectInt leftSpace = new RectInt(space.x, space.y, splitX, space.height);
            RectInt rightSpace = new RectInt(space.x + splitX, space.y, space.width - splitX, space.height);

            CurrentDivisions++;
            
            // Llamada recursiva en las nuevas particiones
            DivideSpace(leftSpace);
            DivideSpace(rightSpace);
        }
        else if (!divideVertically && space.height > MinPartitionSize * 2)
        {
            // Dividir horizontalmente en una posici�n v�lida
            int splitY = Random.Range(MinPartitionSize, space.height - MinPartitionSize);
            splitY = (splitY / BlockSize);
            splitY *= BlockSize;// Alinear a m�ltiplo de BlockSize

            RectInt topSpace = new RectInt(space.x, space.y, space.width, splitY);
            RectInt bottomSpace = new RectInt(space.x, space.y + splitY, space.width, space.height - splitY);

            CurrentDivisions++;
            
            // Llamada recursiva en las nuevas particiones
            DivideSpace(topSpace);
            DivideSpace(bottomSpace);
        }
        else
        {
            // Si no se puede dividir en ninguna direcci�n, agregar el espacio a las particiones finales
            Partitions.Add(space);
        }
    }

    // Generar bloques dentro de cada partici�n
    private void GenerateBlocks()
    {
        int CurrentPartitionNumber = 0;
        foreach (RectInt Partition in Partitions)
        {
            // Solo generar bloques si la partici�n tiene un tama�o adecuado
            if (Partition.width >= BlockSize && Partition.height >= BlockSize)
            {
                // Convierte el ancho y alto de la partici�n a unidades en base al tama�o del bloque
                int PartitionXUnits = Partition.width / BlockSize;
                int PartitionYUnits = Partition.height / BlockSize;

                // Variables para definir el tama�o y posici�n de la plataforma dentro de la partici�n
                int PlatformXUnits = 0;
                int PlatformYUnits = 0;

                int PlatformXStartUnit = 0;
                int PlatformYStartUnit = 0;

                int PlatformXEndUnit = 0;
                int PlatformYEndUnit = 0;

                // Determina si se aplicar�n l�mites en la plataforma seg�n una probabilidad dada
                if (Random.value <= ApplyBoundariesProbability)
                {
                    // Si el ancho de la partici�n permite l�mites, se generan con restricciones
                    if (PartitionXUnits >= MinPlatformSizeInUnits + 2)
                    {
                        PlatformXUnits = Random.Range(MinPlatformSizeInUnits, PartitionXUnits - 2);
                        PlatformXStartUnit = Random.Range(1, PartitionXUnits - PlatformXUnits - 1);
                        PlatformXEndUnit = PlatformXStartUnit + PlatformXUnits;
                    }
                    else // Si no, se generan sin restricciones de borde
                    {
                        PlatformXUnits = Random.Range(MinPlatformSizeInUnits, PartitionXUnits);
                        PlatformXStartUnit = Random.Range(0, PartitionXUnits - PlatformXUnits);
                        PlatformXEndUnit = PlatformXStartUnit + PlatformXUnits;
                    }

                    // Si la altura de la partici�n permite l�mites, se generan con restricciones
                    if (PartitionYUnits >= MinPlatformSizeInUnits + 2)
                    {
                        PlatformYUnits = Random.Range(MinPlatformSizeInUnits, PartitionYUnits - 2);
                        PlatformYStartUnit = Random.Range(1, PartitionYUnits - PlatformYUnits - 1);
                        PlatformYEndUnit = PlatformYStartUnit + PlatformYUnits;
                    }
                    else // Si no, se generan sin restricciones de borde
                    {
                        PlatformYUnits = Random.Range(MinPlatformSizeInUnits, PartitionYUnits);
                        PlatformYStartUnit = Random.Range(0, PartitionYUnits - PlatformYUnits);
                        PlatformYEndUnit = PlatformYStartUnit + PlatformYUnits;
                    }
                }
                else
                {
                    // Si no se aplican l�mites, se generan plataformas con el tama�o m�ximo permitido
                    PlatformXUnits = Random.Range(MinPlatformSizeInUnits, PartitionXUnits);
                    PlatformYUnits = Random.Range(MinPlatformSizeInUnits, PartitionYUnits);

                    PlatformXStartUnit = Random.Range(0, PartitionXUnits - PlatformXUnits);
                    PlatformYStartUnit = Random.Range(0, PartitionYUnits - PlatformYUnits);

                    PlatformXEndUnit = PlatformXStartUnit + PlatformXUnits;
                    PlatformYEndUnit = PlatformYStartUnit + PlatformYUnits;
                }

                // Crea un nuevo objeto que representar� la plataforma
                GameObject NewPlatform = new GameObject();
                NewPlatform.transform.parent = transform; // Lo asigna como hijo del objeto actual
                NewPlatform.transform.localPosition = new Vector3(0, PlatformsHeigth, 0); // Lo posiciona en la altura correspondiente
                NewPlatform.name = "Platform " + CurrentPartitionNumber.ToString(); // Le asigna un nombre �nico

                // A�ade la nueva plataforma a la lista de plataformas generadas
                Platforms.Add((NewPlatform, PlatformXUnits, PlatformYUnits));

                // Incrementa el contador de particiones para nombrar correctamente las siguientes plataformas
                CurrentPartitionNumber++;

                //Variable para almacenar los bloques de las esquinas de la plataforma
                GameObject[] CornerBlocks = new GameObject[4];

                //Bucle por cada posici�n unitaria de la plataforma para instanciar los prefabs de los bloques y rellenar la plataforma
                for (int x = Partition.x + PlatformXStartUnit * BlockSize; x < Partition.x + PlatformXEndUnit * BlockSize; x += BlockSize)
                {
                    for (int y = Partition.y + PlatformYStartUnit * BlockSize; y < Partition.y + PlatformYEndUnit * BlockSize; y += BlockSize)
                    {
                        //Instanciaci�n del bloque en la posici�n correspondiente
                        Vector3 position = new Vector3(x, PlatformsHeigth, y);
                        GameObject BlockInstance = Instantiate(BlockPrefab, position, Quaternion.identity, NewPlatform.transform);
                        Blocks.Add(BlockInstance);

                        //Asignaci�n a los bloques esquina para que sean padres de los pilares
                        if (x == Partition.x + PlatformXStartUnit * BlockSize && y == Partition.y + PlatformYStartUnit * BlockSize)
                        {
                            CornerBlocks[0] = BlockInstance;
                        }
                        if ((x + BlockSize) == Partition.x + PlatformXEndUnit * BlockSize && y == Partition.y + PlatformYStartUnit * BlockSize)
                        {
                            CornerBlocks[1] = BlockInstance;
                        }
                        if (x == Partition.x + PlatformXStartUnit * BlockSize && (y + BlockSize) == Partition.y + PlatformYEndUnit * BlockSize)
                        {
                            CornerBlocks[2] = BlockInstance;
                        }
                        if ((x + BlockSize) == Partition.x + PlatformXEndUnit * BlockSize && (y + BlockSize) == Partition.y + PlatformYEndUnit * BlockSize)
                        {
                            CornerBlocks[3] = BlockInstance;
                        }

                    }
                }

                // Calcular las variables necesarias para decidir las esquinas de los pilares
                float HalfBlockSize = BlockSize / 2f;
                float HalfPillarSize = PillarPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x / 2f * PillarPrefab.transform.localScale.x; //97.82669f;
                float PillarHeight = PillarPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * PillarPrefab.transform.localScale.y; //220.75055f;

                //Calcular las esquinas de cada pilar
                Vector3[] PillarCorners = new Vector3[]
                {
                    new Vector3(Partition.x + PlatformXStartUnit * BlockSize - HalfBlockSize + HalfPillarSize, PlatformsHeigth - PillarHeight + 0.95f, Partition.y + PlatformYStartUnit * BlockSize - HalfBlockSize + HalfPillarSize),
                    new Vector3(Partition.x + PlatformXEndUnit * BlockSize - HalfBlockSize - HalfPillarSize, PlatformsHeigth - PillarHeight + 0.95f, Partition.y + PlatformYStartUnit * BlockSize - HalfBlockSize + HalfPillarSize),
                    new Vector3(Partition.x + PlatformXStartUnit * BlockSize - HalfBlockSize + HalfPillarSize, PlatformsHeigth - PillarHeight + 0.95f, Partition.y + PlatformYEndUnit * BlockSize - HalfBlockSize - HalfPillarSize),
                    new Vector3(Partition.x + PlatformXEndUnit * BlockSize - HalfBlockSize - HalfPillarSize, PlatformsHeigth - PillarHeight + 0.95f, Partition.y + PlatformYEndUnit * BlockSize - HalfBlockSize - HalfPillarSize)
                };

                // Generar pilares en cada esquina de la plataforma
                int PillarNum = -1;
                foreach (Vector3 PillarCorner in PillarCorners)
                {
                    PillarNum += 1;
                    //Raycast para comprobar hasta que distancia es necesario instanciar pilares
                    if (Physics.Raycast(new Vector3(PillarCorner.x, PillarCorner.y + PillarHeight - 1.5f, PillarCorner.z), Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerForPillarsToHit))
                    {
                        //Numero de pilar a instanciar seg�n la distancia a cubrir hasta el suelo
                        int PillarAmount = Mathf.CeilToInt(hit.distance / PillarHeight);

                        //Bucle para instanciar los pilares
                        for (int i = 0; i < PillarAmount; i++)
                        {
                            GameObject PillarInstance = Instantiate(PillarPrefab, new Vector3(PillarCorner.x, PillarCorner.y - i * PillarHeight, PillarCorner.z), Quaternion.identity, NewPlatform.transform);
                            PillarInstance.transform.SetParent(CornerBlocks[PillarNum].transform); //Se pone como padre al bloque de esquina correspondiente
                        }
                    }
                }
            }
        }

        // Comnprobar que la capa no ha sido completamente rellenada con bloques. Si ocurre, eliminar un bloque aleatorio para dejar paso al jugador
        int SpaceWidthInUnits = Width * BlockSize;
        int SpaceHeightInUnits = Height * BlockSize;
        int BlockAmount = Blocks.Count;
        if (BlockAmount == SpaceWidthInUnits * SpaceHeightInUnits)
        {
            DestroyImmediate(Blocks[UnityEngine.Random.Range(0, BlockAmount)]);
        }
    }
}