using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

namespace ULTRAKILL_Competitive_Multiplayer;

/*
[Info   : Unity Log] CustomCybergrind started
[Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.CreateSubmeshes (System.Collections.Generic.List`1[T] materials) (at <f2be90e38e574a7d8a5d49aeacff2935>:0)
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.SetupStaticGridMesh () (at <f2be90e38e574a7d8a5d49aeacff2935>:0)
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.TrySetupStaticGridMesh () (at <f2be90e38e574a7d8a5d49aeacff2935>:0)
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.Init () (at <f2be90e38e574a7d8a5d49aeacff2935>:0)
UltraIDK.CompMultiplayerMain+<DoCGStuff>d__27.MoveNext () (at <f2be90e38e574a7d8a5d49aeacff2935>:0)
UnityEngine.SetupCoroutine.InvokeMoveNext (System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) (at <dfbdd4656e0844829a5285bde9c1a365>:0)
 * 
 */


[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CustomCybergrind : MonoSingleton<CustomCybergrind>
{
    public CustomCybergrind(GameObject gridcube)
    {
        this.gridCube = gridcube;
    }
    #region unimportant_properties
    public bool customPatternMode = false;
    public ArenaPattern[] customPatterns;
    public ArenaPattern[] patterns;
    public PrefabDatabase prefabs;
    public GameObject gridCube;
    public float offset = 5f;
    public EndlessCube[][] cubes;
    private int incompleteBlocks;
    private ArenaPattern currentPattern;
    public NavMeshSurface nms;
    public int currentPatternNum = -1;
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private int incompletePrefabs;

    private GoreZone gz;

    public bool crowdReactions;

    private CrowdReactions crorea;
    private Material[] mats;
    private Color targetColor;
    public float glowMultiplier = 1f;

    public GameObject combinedGridStaticObject;

    private MeshRenderer combinedGridStaticMeshRenderer;

    private MeshFilter combinedGridStaticMeshFilter;

    private Mesh combinedGridStaticMesh;

    private static readonly int WorldOffset = Shader.PropertyToID("_WorldOffset");

    private static readonly int GradientSpeed = Shader.PropertyToID("_GradientSpeed");

    private static readonly int GradientFalloff = Shader.PropertyToID("_GradientFalloff");

    private static readonly int GradientScale = Shader.PropertyToID("_GradientScale");

    private static readonly int PcGamerMode = Shader.PropertyToID("_PCGamerMode");

    public ArenaPattern[] CurrentPatternPool
    {
        get
        {
            if (!this.customPatternMode)
            {
                return this.patterns;
            }
            return this.customPatterns;
        }
    }
    #endregion
    public const int ArenaSize = 3;
    int gridWidth => ArenaSize;
    int gridHeight => ArenaSize;
    float cubeOffset = 2f;
    public void Init()
    {
        print("CustomCybergrind started");
        
        if (gridCube == null)
        {
            Debug.LogError("gridCube prefab is not assigned!");
            return;
        }
        
        this.nms = base.GetComponent<NavMeshSurface>();
        this.gz = GoreZone.ResolveGoreZone(base.transform);
        
        this.cubes = new EndlessCube[gridHeight][];
        for (int i = 0; i < gridHeight; i++)
        {
            this.cubes[i] = new EndlessCube[gridWidth];
            for (int j = 0; j < gridWidth; j++)
            {
                try 
                {
                    GameObject gameObject = Instantiate(this.gridCube, transform, true);
                    if (gameObject != null)
                    {
                        gameObject.SetActive(true);
                        gameObject.transform.localPosition = new Vector3(i * this.offset, 0f, j * this.offset);
                        EndlessCube cube = gameObject.GetComponent<EndlessCube>();
                        if (cube != null)
                        {
                            this.cubes[i][j] = cube;
                            this.cubes[i][j].positionOnGrid = new Vector2Int(i, j);
                        }
                        else
                        {
                            Debug.LogError($"EndlessCube component not found on instantiated gridCube at [{i},{j}]");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to instantiate gridCube at [{i},{j}]: {e.Message}");
                }
            }
        }
        this.crorea = MonoSingleton<CrowdReactions>.Instance;
        if (this.crorea != null)
        {
            this.crowdReactions = true;
        }
        //PresenceController.UpdateCyberGrindWave(0);
        this.mats = base.GetComponentInChildren<MeshRenderer>().sharedMaterials;
        foreach (Material material in this.mats)
        {
            material.SetColor(UKShaderProperties.EmissiveColor, Color.blue);
            material.SetFloat(UKShaderProperties.EmissiveIntensity, 0.2f * this.glowMultiplier);
            material.SetFloat("_PCGamerMode", 0f);
            material.SetFloat("_GradientScale", 2f);
            material.SetFloat("_GradientFalloff", 5f);
            material.SetFloat("_GradientSpeed", 10f);
            material.SetVector("_WorldOffset", new Vector4(0f, 0f, 62.5f, 0f));
            this.targetColor = Color.blue;
        }
        this.TrySetupStaticGridMesh();
    }
    
    public void TrySetupStaticGridMesh()
    {
        if (this.incompleteBlocks != 0 || this.incompletePrefabs != 0)
        {
            return;
        }
        this.SetupStaticGridMesh();
    }

    public void SetupStaticGridMesh()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        InitializeStaticMeshObjects();
        
        combinedGridStaticObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        combinedGridStaticObject.transform.localScale = Vector3.one;
        
        List<Material> materials = GetMaterialsFromCubes();
        
        List<Mesh> submeshes = CreateSubmeshes(materials);
        
        CombineSubmeshes(submeshes, materials);
        
        for (int i = 0; i < submeshes.Count; i++)
        {
            UnityEngine.Object.Destroy(submeshes[i]);
        }
        submeshes.Clear();
        
        stopwatch.Stop();
        Debug.Log($"Combined arena mesh in {stopwatch.ElapsedMilliseconds} ms");
        
        UpdatePhysics();
    }

    private void InitializeStaticMeshObjects()
    {
        if (combinedGridStaticObject == null)
        {
            combinedGridStaticObject = new GameObject("Combined Static Mesh");
            combinedGridStaticObject.transform.parent = transform;
            combinedGridStaticObject.layer = LayerMask.NameToLayer("Outdoors");
            combinedGridStaticMeshRenderer = combinedGridStaticObject.AddComponent<MeshRenderer>();
            combinedGridStaticMeshFilter = combinedGridStaticObject.AddComponent<MeshFilter>();
        }
        
        if (combinedGridStaticMesh == null)
        {
            combinedGridStaticMesh = new Mesh();
            // Fix for vertex count exceeding 65535 limit
            combinedGridStaticMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        else
        {
            combinedGridStaticMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedGridStaticMesh.Clear();
        }
    }

    private List<Material> GetMaterialsFromCubes()
    {
        List<Material> materials = new List<Material>();
        
        EndlessCube firstCube = null;
        for (int i = 0; i < gridHeight && firstCube == null; i++)
        {
            for (int j = 0; j < gridWidth && firstCube == null; j++)
            {
                if (cubes[i][j] != null)
                {
                    firstCube = cubes[i][j];
                }
            }
        }
        
        if (firstCube != null)
        {
            foreach (Material material in firstCube.MeshRenderer.sharedMaterials)
            {
                if (!materials.Contains(material))
                {
                    materials.Add(material);
                }
            }
        }
        
        return materials;
    }

    private List<Mesh> CreateSubmeshes(List<Material> materials)
    {
        if (materials == null || materials.Count == 0)
        {
            Debug.LogError("Materials list is null or empty.");
            return new List<Mesh>();
        }
        
        List<Mesh> submeshes = new List<Mesh>();
        bool materialsAdded = false;

        for (int materialIndex = 0; materialIndex < materials.Count; materialIndex++)
        {
            Mesh submesh = new Mesh();
            submesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            List<CombineInstance> combineInstances = new List<CombineInstance>();

            if (cubes == null)
            {
                Debug.LogError("Cubes array is null.");
                return submeshes;
            }

            try
            {
                for (int i = 0; i < gridHeight; i++)
                {
                    for (int j = 0; j < gridWidth; j++)
                    {
                        
                        if (cubes[i] == null)
                        {
                            Debug.LogWarning($"Row {i} in cubes array is null.");
                            continue;
                        }

                        EndlessCube cube = cubes[i][j];
                        if (cube == null)
                        {
                            Debug.LogWarning($"Cube at position [{i}][{j}] is null");
                            continue;
                        }

                        if (cube.MeshFilter != null && cube.MeshRenderer != null && cube.MeshFilter.sharedMesh != null)
                        {
                            combineInstances.Add(new CombineInstance
                            {
                                transform = cube.MeshRenderer.localToWorldMatrix,
                                mesh = cube.MeshFilter.sharedMesh,
                                subMeshIndex = materialIndex
                            });
                            cube.MeshRenderer.enabled = false; 
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception in cube processing: {e.Message}\n{e.StackTrace}");
            }

            if (materialIndex == 1)
            {
                if (spawnedPrefabs == null)
                {
                    Debug.LogWarning("spawnedPrefabs is null, skipping stairs processing");
                }
                else
                {
                    Debug.Log($"spawnedPrefabs count: {spawnedPrefabs.Count}");

                    foreach (GameObject prefab in spawnedPrefabs)
                    {
                        if (prefab == null)
                        {
                            Debug.LogWarning("Skipping null prefab");
                            continue;
                        }

                        bool hasComponent = prefab.TryGetComponent<EndlessStairs>(out EndlessStairs stairs);

                        if (hasComponent)
                        {

                            if (!materialsAdded)
                            {
                                try
                                {
                                    AddStairsMaterials(stairs, materials);
                                    materialsAdded = true;
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"Exception in AddStairsMaterials: {e.Message}\n{e.StackTrace}");
                                }
                            }

                            if (stairs.ActivateFirst && stairs.PrimaryMeshFilter != null && stairs.PrimaryMeshRenderer != null)
                            {
                                combineInstances.Add(new CombineInstance
                                {
                                    transform = stairs.PrimaryMeshRenderer.localToWorldMatrix,
                                    mesh = stairs.PrimaryMeshFilter.sharedMesh
                                });
                                stairs.PrimaryMeshRenderer.enabled = false;
                            }

                            if (stairs.ActivateSecond && stairs.SecondaryMeshFilter != null && stairs.SecondaryMeshRenderer != null)
                            {
                                combineInstances.Add(new CombineInstance
                                {
                                    transform = stairs.SecondaryMeshRenderer.localToWorldMatrix,
                                    mesh = stairs.SecondaryMeshFilter.sharedMesh
                                });
                                stairs.SecondaryMeshRenderer.enabled = false;
                            }
                        }
                    }
                }
            }

            if (combineInstances.Count > 0)
            {
                submesh.CombineMeshes(combineInstances.ToArray(), true, true);
                submeshes.Add(submesh);
            }
            else
            {  
                Debug.LogWarning($"No combine instances created for material index {materialIndex}.");
            }
        }


        return submeshes;
    }

    private void AddStairsMaterials(EndlessStairs stairs, List<Material> materials)
    {
        if (stairs.ActivateFirst)
        {
            foreach (Material material in stairs.PrimaryMeshRenderer.sharedMaterials)
            {
                if (!materials.Contains(material))
                {
                    materials.Add(material);
                }
            }
        }
        
        if (stairs.ActivateSecond)
        {
            foreach (Material material in stairs.SecondaryMeshRenderer.sharedMaterials)
            {
                if (!materials.Contains(material))
                {
                    materials.Add(material);
                }
            }
        }
    }

    private void CombineSubmeshes(List<Mesh> submeshes, List<Material> materials)
    {
        if (combinedGridStaticMesh == null)
        {
            Debug.LogError("combinedGridStaticMesh is null!");
            return;
        }

        if (combinedGridStaticObject == null)
        {
            TrySetupStaticGridMesh();
        } 

        if (combinedGridStaticMeshRenderer == null || combinedGridStaticObject == null || combinedGridStaticMeshFilter == null)
        {
            Debug.LogError($"One or more required components are null! combinedGridStaticMeshRenderer:{combinedGridStaticMeshRenderer == null} | combinedGridStaticObject: {combinedGridStaticObject == null} | combinedGridStaticMeshFilter: {combinedGridStaticMeshFilter == null}" );
            return;
        }

        CombineInstance[] combineInstances = new CombineInstance[submeshes.Count];

        for (int i = 0; i < submeshes.Count; i++)
        {
            combineInstances[i] = new CombineInstance
            {
                mesh = submeshes[i]
            };
        }

        combinedGridStaticMesh.CombineMeshes(combineInstances, false, false);
        combinedGridStaticMesh.Optimize();
        combinedGridStaticMesh.RecalculateBounds();
        combinedGridStaticMesh.RecalculateNormals();
        combinedGridStaticMesh.UploadMeshData(false);

        combinedGridStaticMeshRenderer.sharedMaterials = materials.ToArray();
        combinedGridStaticObject.SetActive(true);
        combinedGridStaticMeshFilter.sharedMesh = combinedGridStaticMesh;
    }

    private void UpdatePhysics()
    {
        if (combinedGridStaticObject.TryGetComponent<PhysicsSceneStateEnforcer>(out PhysicsSceneStateEnforcer enforcer))
        {
            enforcer.ForceUpdate();
        }
        else
        {
            Transform transform = combinedGridStaticObject.transform;
            MonoSingleton<SceneHelper>.Instance.AddMeshToPhysicsScene(
                combinedGridStaticMesh,
                combinedGridStaticMeshRenderer.sharedMaterials,
                transform.position,
                transform.rotation,
                transform.lossyScale,
                combinedGridStaticObject.layer,
                combinedGridStaticObject
            );
        }
    }
    
    public void LoadPattern(ArenaPattern pattern)
    {
        string patternName = GetFormattedPatternName(pattern);

        string[] rows = pattern.heights.Split('\n', StringSplitOptions.None);
        ValidatePatternDimensions(pattern.name, rows);

        ProcessPatternRows(pattern, rows);

        currentPattern = pattern;
        MakeGridDynamic();
    }
    private string GetFormattedPatternName(ArenaPattern pattern)
    {
        if (!customPatternMode)
            return pattern.name;

        string[] array = pattern.name.Split('\\', StringSplitOptions.None);
        string text = array[array.Length - 1];
        text = text.Substring(0, text.Length - 4);
        text = text.Replace("CG_", "").Replace("Cg_", "").Replace("cg_", "");
        text = text.Replace('_', ' ');
        text = SplitCamelCase(text);
        text = text.Replace("  ", " ");
        return text.ToUpper();
    }
    
    private void ValidatePatternDimensions(string patternName, string[] rows)
    {
        if (rows.Length != gridHeight)
        {
            throw new ArgumentException(
                $"[Heights] Pattern \"{patternName}\" has {rows.Length} rows instead of {gridHeight}");
        }
    }   
    
    private void ProcessPatternRows(ArenaPattern pattern, string[] rows)
    {
        if (pattern == null)
        {
            Debug.LogError("ProcessPatternRows: pattern is null");
            return;
        }
        
        if (rows == null)
        {
            Debug.LogError("ProcessPatternRows: rows array is null");
            return;
        }
        
        if (cubes == null)
        {
            Debug.LogError("ProcessPatternRows: cubes array is null");
            return;
        }
        
        incompleteBlocks = 0;
        
        for (int i = 0; i < rows.Length && i < gridHeight; i++)
        {
            if (rows[i] == null)
            {
                Debug.LogWarning($"ProcessPatternRows: row {i} is null, skipping");
                continue;
            }
            
            try
            {
                int[] heights = ParseRowHeights(pattern.name, rows[i], i);
                
                for (int j = 0; j < heights.Length && j < gridWidth; j++)
                {
                    if (cubes[i] != null && cubes[i][j] != null)
                    {
                        cubes[i][j].SetTarget(heights[j] * cubeOffset / 2f);
                        cubes[i][j].blockedByPrefab = false;
                        incompleteBlocks++;
                    }
                    else
                    {
                        Debug.LogWarning($"ProcessPatternRows: cube at [{i},{j}] is null");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"ProcessPatternRows: Error processing row {i}: {e.Message}");
            }
        }
    }


    private int[] ParseRowHeights(string patternName, string row, int rowIndex)
    {
        int[] heights = new int[gridWidth];

        if (row.Length == gridWidth)
        {
            for (int k = 0; k < row.Length; k++)
            {
                heights[k] = int.Parse(row[k].ToString());
            }
        }
        else if (row.Length < gridWidth)
        {
            throw new ArgumentException(
                $"[Heights] Pattern \"{patternName}\" has {row.Length} elements in row {rowIndex} instead of {gridWidth}");
        }
        else
        {
            int columnIndex = 0;
            bool inParentheses = false;
            string extendedNumber = "";
            
            for (int j = 0; j < row.Length; j++)
            {
                char currentChar = row[j];
                
                if (inParentheses)
                {
                    if (currentChar == ')')
                    {
                        heights[columnIndex] = int.Parse(extendedNumber);
                        columnIndex++;
                        inParentheses = false;
                        extendedNumber = "";
                    }
                    else
                    {
                        extendedNumber += currentChar;
                    }
                }
                else if (currentChar == '(')
                {
                    inParentheses = true;
                }
                else if (int.TryParse(currentChar.ToString(), out int height) || currentChar == '-')
                {
                    if (columnIndex >= gridWidth)
                    {
                        throw new ArgumentException(
                            $"Unable to parse pattern: {patternName} at row {rowIndex} and column {j}");
                    }
                    heights[columnIndex] = height;
                    columnIndex++;
                }
            }
            
            if (columnIndex != gridWidth)
            {
                throw new ArgumentException(
                    $"[Heights] Pattern \"{patternName}\" has {row.Length} characters but parsed {columnIndex} " +
                    $"elements in row {rowIndex} instead of {gridWidth}");
            }
        }

        return heights;
    }

    public void MakeGridDynamic()
    {
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                EndlessCube cube = cubes[i][j];
                if (cube != null)
                {
                    cube.MeshRenderer.enabled = true;
                }
            }
        }
        
        foreach (GameObject prefab in spawnedPrefabs)
        {
            if (prefab.TryGetComponent<EndlessStairs>(out EndlessStairs stairs))
            {
                if (stairs.ActivateFirst)
                {
                    stairs.PrimaryMeshRenderer.enabled = true;
                }
                if (stairs.ActivateSecond)
                {
                    stairs.SecondaryMeshRenderer.enabled = true;
                }
            }
        }
        
        if (combinedGridStaticObject != null)
        {
            combinedGridStaticObject.SetActive(false);
        }
    }
    
    private string SplitCamelCase(string str)
    {
        return Regex.Replace(Regex.Replace(str, "(\\P{Ll})(\\P{Ll}\\p{Ll})", "$1 $2"), "(\\p{Ll})(\\P{Ll})", "$1 $2");
    }
}

