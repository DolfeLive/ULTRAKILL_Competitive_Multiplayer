using System;
using UnityEngine;
using ULTRAKILL;
using UnityEngine.UI;
using BepInEx;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using HarmonyLib;
using Clogger = ULTRAKILL_Competitive_Multiplayer.Logger;
using UnityEngine.Events;
using Steamworks;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using ULTRAKILL_Competitive_Multiplayer;
using Unity.AI.Navigation;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ULTRAKILL_Competitive_Multiplayer;

[BepInPlugin("DolfeMods.Ultrakill.ULTRAKILL_Competitive_Multiplayer", "ULTRAKILL Competitive Multiplayer", "1.0.0")]
public class CompMultiplayerMain : BaseUnityPlugin
{
    public static CompMultiplayerMain instance;
    public bool inMultiplayerScene = false;

    public static Toggle CrackedToggle;
    
    bool loadScene = true; // For testing

    public static string modName = "UKCM";
    public static uint appId => cracked ? 480u : 1229490u;
    public static bool cracked = false;

    public static AssetBundle AssetsBundle;
    public static AssetBundle sceneBundle;

    GameObject MultiplayerMenu;
    public static GameObject MultiplayerModel;
    GameObject MMObject;
    public static GameObject LobbyPrefab;
    public static GameObject LobbyParent;

    private LobbyList? lobbyList;

    void Awake()
    {
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;
        
        Semtings.Init();

        Harmony har = new Harmony("UKCM");
        har.PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;

        string bundlePath = Paths.PluginPath;
        LoadAssets(bundlePath);
        //InvokePrivate<MyTargetClass>(null, "MyPrivateStaticMethod", new object[] { "Hello, world!" });
        AddPatterns();
    }

    void AddPatterns()
    {
        
        ArenaPattern arena = ScriptableObject.CreateInstance<ArenaPattern>();
        arena.name = "Arena1";
        arena.heights =
@"(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)
(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)
(2)(2)(5)(4)(3)(0)(0)(0)(0)(0)(0)(3)(4)(5)(2)(2)
(2)(2)(4)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(4)(2)(2)
(1)(1)(3)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(3)(1)(1)
(1)(1)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(1)(1)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(1)(1)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(1)(1)
(1)(1)(3)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(3)(1)(1)
(2)(2)(4)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(4)(2)(2)
(2)(2)(5)(4)(3)(0)(0)(0)(0)(0)(0)(3)(4)(5)(2)(2)
(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)
(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)";

        ArenaPattern arena2 = ScriptableObject.CreateInstance<ArenaPattern>();
        arena2.name = "Arena2";
        arena2.heights =
@"(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)
(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)
(2)(2)(5)(4)(3)(0)(0)(0)(0)(0)(0)(3)(4)(5)(2)(2)
(2)(2)(4)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(4)(2)(2)
(1)(1)(3)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(3)(1)(1)
(1)(1)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(1)(1)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(5)(5)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(5)(5)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(1)(1)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(1)(1)
(1)(1)(3)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(3)(1)(1)
(2)(2)(4)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(4)(2)(2)
(2)(2)(5)(4)(3)(0)(0)(0)(0)(0)(0)(3)(4)(5)(2)(2)
(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)
(2)(2)(2)(2)(1)(1)(0)(0)(0)(0)(1)(1)(2)(2)(2)(2)";
        
        ArenaPattern stairsTest = ScriptableObject.CreateInstance<ArenaPattern>();
        stairsTest.name = "Stairs test";
        stairsTest.heights =
@"(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(1)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(2)(4)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)
(3)(2)(1)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)(0)";
        stairsTest.prefabs =
@"0000000000000000
0000000000000000
0000000000000000
0000000000000000
0000000000000000
0000000000000000
0000000000000000
0000000000000000
0000000000000000
0000000000000000
00JJ000000000000
000JJ00000000000
s000JJ0000000000
s000000000000000
sJ00000000000000
Jsss000000000000";

        string GeneratePattern(int width, int height, int minValue, int maxValue)
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int number = UnityEngine.Random.Range(minValue, maxValue);
                    sb.Append($"({number})");
                }
                if (y != height - 1)
                    sb.Append("\n");
            }

            return sb.ToString();
        }
        string pattern = GeneratePattern(CustomCybergrind.ArenaSize, CustomCybergrind.ArenaSize, 0, 6);
        ArenaPattern arena3 = ScriptableObject.CreateInstance<ArenaPattern>();
        arena3.name = $"randomArena1";
        arena3.heights = pattern;
        //arena3.prefabs = pattern;

        string pattern2 = GeneratePattern(CustomCybergrind.ArenaSize, CustomCybergrind.ArenaSize, 0, 6);
        ArenaPattern arena4 = ScriptableObject.CreateInstance<ArenaPattern>();
        arena4.name = $"randomArena2";
        arena4.heights = pattern2;
        //arena4.prefabs = pattern2;

        patterns.Add(arena);
        patterns.Add(arena2);
        patterns.Add(stairsTest);
        patterns.Add(arena3);
        patterns.Add(arena4);
    }

    void crackedToggleChanged(bool newVal)
    {
        Clogger.Log($"Cracked value changed to: {newVal}");
        MultiplayerUtil.LobbyManager.ReInnitSteamClient(newVal);
        lobbyList?.FetchLobbies();
    }


    void LoadAssets(string path)
    {
        AssetsBundle = AssetBundle.LoadFromFile(Path.Combine(path, "ukcm.ukcm"));
        sceneBundle = AssetBundle.LoadFromFile(Path.Combine(path, "level.meatgrinder"));

        if (AssetsBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }

        LobbyPrefab = AssetsBundle.LoadAsset<GameObject>("Lobby");
        MultiplayerMenu = AssetsBundle.LoadAsset<GameObject>("Multiplayer Menu");
        MultiplayerModel = AssetsBundle.LoadAsset<GameObject>("MultiplayerModelV2");
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            NewMovement.Instance.GetHurt(1000, false);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            print($"Respawned, deaths: {deaths}");

            NewMovementRespawn();
            
        }
        else if (Input.GetKeyDown(KeyCode.O))
        { 
            print($"Pat indx: {patternIndex}, patterns cnt: {patterns.Count}, patterns: {patterns.Select(_ => _.name)}");
            patternIndex++;
            if (patternIndex > patterns.Count - 1)
            {
                patternIndex = 0;
            }
            CustomCybergrind cybergrind = FindFirstObjectByType<CustomCybergrind>();
            if (cybergrind == null)
            {
                Debug.LogError("cybergrind is null");
                return;
            }
            print($"Loading pattern: {patterns[patternIndex].name}, {patternIndex}");
            cybergrind.LoadPattern(patterns[patternIndex]);
        }
    }
    public List<ArenaPattern> patterns = new();
    int patternIndex = 0;

    NewMovement nm;

    int deaths = 0;
    public void NewMovementRespawn()
    {
        Debug.Log("=== RESPAWN SEQUENCE STARTED ===");

        if (CameraController.Instance != null)
        {
            CameraController.Instance.activated = true;
        }
        else
            Debug.LogError("CameraController.Instance is null");

        if (this.nm != null)
        {
            this.nm.enabled = true;
        }
        else
            Debug.LogError("this.nm reference is null");

        if (nm != null)
        {
            nm.activated = true;
        }
        else
            Debug.LogError("nm reference is null");

        if (CameraController.Instance != null && CameraController.Instance.cam != null)
        {
            CameraController.Instance.cam.useOcclusionCulling = true;
        }
        else
            Debug.LogError("Camera reference issue: " +
                          (CameraController.Instance == null ? "CameraController.Instance is null" : "CameraController.Instance.cam is null"));

        if (nm == null)
        {
            Debug.LogError("NewMovement reference is null, cannot respawn player");
            return;
        }

        deaths++;
        Debug.Log($"Death counter increased to {deaths}");

        try
        {
            nm.StopSlide();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error stopping slide: {ex.Message}");
        }

        nm.hp = 150;
        nm.boostCharge = 299f;
        nm.antiHp = 0f;
        nm.antiHpCooldown = 0f;

        if (nm.rb != null)
        {
            nm.rb.constraints = nm.defaultRBConstraints;
        }
        else
            Debug.LogWarning("Player rigidbody is null");

        Transform blackscreen = transform.Find("/Canvas/BackScreen");
        if (blackscreen != null)
            blackscreen.gameObject.SetActive(false);

        DeathSequence deathSeq = FindFirstObjectByType<DeathSequence>();
        if (deathSeq != null)
        {
            deathSeq.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("DeathSequence is null");
        }

        if (nm.cc != null)
        {
            nm.cc.enabled = true;
        }
        else
            Debug.LogWarning("Character controller is null");

        if (PowerUpMeter.Instance != null)
        {
            PowerUpMeter.Instance.juice = 0f;
        }
        else
            Debug.LogWarning("PowerUpMeter.Instance is null");

        GunControl gunc = nm != null ? nm.GetComponentInChildren<GunControl>() : null;
        if (gunc != null)
        {
            gunc.YesWeapon();
        }
        else
            Debug.LogWarning("GunControl component not found");

        if (nm.screenHud != null)
        {
            nm.screenHud.SetActive(true);
        }
        else
            Debug.LogWarning("screenHud is null");

        nm.dead = false;

        if (TimeController.Instance != null)
        {
            TimeController.Instance.controlPitch = true;
        }
        else
            Debug.LogWarning("TimeController.Instance is null");

        if (HookArm.Instance != null)
        {
            HookArm.Instance.Cancel();
        }
        else
            Debug.LogWarning("HookArm.Instance is null");

        if (nm.punch != null)
        {
            nm.punch.activated = true;
            nm.punch.YesFist();
        } 
        else
            Debug.LogWarning("Player punch component is null");

        nm.slowMode = false;
        
        if (WeaponCharges.Instance != null)
        {
            WeaponCharges.Instance.MaxCharges();

            if (WeaponCharges.Instance.rocketLauncher != null)
            {
                WeaponCharges.Instance.rocketLauncher.UnfreezeRockets();
            }
            else
                Debug.LogWarning("RocketLauncher is null");
        }
        else
            Debug.LogWarning("WeaponCharges.Instance is null");

        if (nm.cc != null)
        {
            nm.cc.StopShake();
        }
        else
            Debug.LogWarning("Cannot stop camera shake - character controller is null");

        try
        {
            nm.ActivatePlayer();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in ActivatePlayer: {ex.Message}");
            Debug.LogError($"Exception type: {ex.GetType().Name}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }

        Debug.Log("=== RESPAWN SEQUENCE COMPLETED ===");
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode lsm)
    {
        Clogger.Log($"Scene loaded: {scene.name}");

        if (scene.name == "b3e7f2f8052488a45b35549efb98d902") // Main Menu
        {
           

            try
            {
                MultiplayerUtil.LobbyManager.Disconnect();
            }
            catch { }

            GameObject MainMenu = GameObject.Find("Main Menu (1)");
            if (MainMenu == null) { Debug.LogError("MainMenu not found!"); return; }

            GameObject Continue = MainMenu.transform.Find("LeftSide/Continue").gameObject;
            GameObject ArenaButton = Instantiate(Continue, Continue.transform.parent);
            ArenaButton.name = "Arena";

            MMObject = Instantiate(MultiplayerMenu, MainMenu.transform.parent);
            CrackedToggle = MMObject.transform.Find("Cracked/CrackedToggle").GetComponent<Toggle>();

            MMObject.SetActive(false);
            MMObject.transform.Find("Create Lobby").gameObject.AddComponent<LobbyCreate>();
            MMObject.transform.Find("Join By Code").gameObject.AddComponent<LobbyJoin>();
            GameObject LobyList = MMObject.transform.Find("Lobby List").gameObject;
            lobbyList = LobyList.AddComponent<LobbyList>();
            LobbyParent = LobyList.transform.Find("Scroll Rect (1)/Contents").gameObject;


            Destroy(ArenaButton.GetComponent<HudOpenEffect>());

            RectTransform rt = ArenaButton.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(1048.212f, -578.5742f, 0f);
            rt.sizeDelta = new Vector2(167.32f, 52.39999f);
            rt.localScale = Vector3.one;

            ArenaButton.AddComponent<HudOpenEffect>();

            ArenaButton.GetComponentInChildren<TMP_Text>().text = "Arena";

            Button playButton = ArenaButton.GetComponent<Button>();
            playButton.onClick = new Button.ButtonClickedEvent();
            playButton.onClick.AddListener(OpenMultiMenu);

            EscMenu____ escMenu = MMObject.AddComponent<EscMenu____>();
            escMenu.Menu = MainMenu;

            CrackedToggle.isOn = SteamClient.AppId.Value == 480u ? true : false;

            UnityAction<bool> crackedChanged = new UnityAction<bool>(crackedToggleChanged);
            CrackedToggle.onValueChanged.AddListener(crackedChanged);
            ArenaButton.SetActive(true);
        }
        if (scene.name == "MEATGRINDER")
        {
            print($"Newmovment exists: {NewMovement.Instance != null}");
            NewMovement.Instance.hp = 150;
            nm = NewMovement.Instance;
            inMultiplayerScene = true;
            print("now in multiplayer scene!");
            StartCoroutine(DoCGStuff());
        }
        if (scene.name == "Bootstrap")
        {
            print("Bootstrap loaded");

            [DllImport("user32.dll")]
            static extern IntPtr GetActiveWindow();

            [DllImport("user32.dll")]
            static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            const int SW_MAXIMIZE = 3;
            
            IntPtr windowHandle = GetActiveWindow();
            ShowWindow(windowHandle, SW_MAXIMIZE);


        }
    }

    IEnumerator DoCGStuff()
    {
        yield return new WaitForSeconds(0.4f);

        Transform spawnPos = transform.Find("/SPAWNPOSITION");
        if (spawnPos == null)
        {
            Debug.Log("Spawnpos was null");
        } 
        else
            nm.transform.position = spawnPos.transform.position;

        
        EndlessGrid cybergrid = FindFirstObjectByType<EndlessGrid>();
        if (cybergrid == null)
        {
            Debug.LogError("DoCGStuff - Cybergrind is null");
            yield break;
        }

        GameObject arenaGO = cybergrid.gameObject;
        PrefabDatabase prefabs = cybergrid.prefabs;

        GameObject jumpPad = prefabs.jumpPad;
        Debug.Log("[DEBUG] jumpPad instantiated: " + (jumpPad != null));
        jumpPad.name = "JUMPPADTEMPLATE";
        
        EndlessPrefabAnimator originalAnimator = jumpPad.GetComponent<EndlessPrefabAnimator>();
        Debug.Log("[DEBUG] originalAnimator: " + (originalAnimator != null));

        bool reverse = originalAnimator.reverse;
        bool reverseOnly = originalAnimator.reverseOnly;

        DestroyImmediate(originalAnimator);
        CustomEndlessPrefabAnimator CEPA = jumpPad.AddComponent<CustomEndlessPrefabAnimator>();
        CEPA.reverse = reverse;
        CEPA.reverseOnly = reverseOnly;

        
        prefabs.jumpPad = jumpPad;

        GameObject stairs = prefabs.stairs;
        Debug.Log("[DEBUG] stairs instantiated: " + (stairs != null));
        stairs.name = "STAIRSTEMPLATE";

        EndlessPrefabAnimator stairsAnimator = stairs.GetComponent<EndlessPrefabAnimator>();
        Debug.Log("[DEBUG] stairsAnimator: " + (stairsAnimator != null));

        bool stairsReverse = stairsAnimator.reverse;
        bool stairsReverseOnly = stairsAnimator.reverseOnly;
        
        
        DestroyImmediate(stairsAnimator);
        CustomEndlessPrefabAnimator stairsCEPA = stairs.AddComponent<CustomEndlessPrefabAnimator>();
        stairsCEPA.reverse = stairsReverse;
        stairsCEPA.reverseOnly = stairsReverseOnly;


        EndlessStairs oldStairs = stairs.GetComponent<EndlessStairs>();
        Debug.Log("[DEBUG] oldStairs: " + (oldStairs != null));

        MeshRenderer primaryRenderer = oldStairs.primaryMeshRenderer;
        MeshRenderer secondaryRenderer = oldStairs.secondaryMeshRenderer;
        MeshFilter primaryFilter = oldStairs.primaryMeshFilter;
        MeshFilter secondaryFilter = oldStairs.secondaryMeshFilter;
        Transform primaryStairs = oldStairs.primaryStairs;
        Transform SecondaryStairs = oldStairs.secondaryStairs;

        DestroyImmediate(oldStairs);

        CustomEndlessStairs newStairs = stairs.AddComponent<CustomEndlessStairs>();
        newStairs.primaryMeshRenderer = primaryRenderer;
        newStairs.secondaryMeshRenderer = secondaryRenderer;
        newStairs.primaryMeshFilter = primaryFilter;
        newStairs.secondaryMeshFilter = secondaryFilter;
        newStairs.primaryStairs = primaryStairs;
        newStairs.secondaryStairs = SecondaryStairs;

        prefabs.stairs = stairs;

        
        GameObject gridCube = cybergrid.gridCube;
        NavMeshSurface navMeshSurface = cybergrid.nms;
        GameObject combinedGridStaticObject = cybergrid.combinedGridStaticObject;
        EndlessCube[][] Cubes = cybergrid.cubes;
        for (int i = 0; i < Cubes.Length; i++)
        {
            for (int j = 0; j < Cubes[i].Length; j++)
            {
                DestroyImmediate(Cubes[i][j].gameObject);
            }
        }
        DestroyImmediate(combinedGridStaticObject);

        EndlessCube cube = gridCube.GetComponent<EndlessCube>();
        Vector2Int positionOnGrid = cube.positionOnGrid;
        bool blockedByPrefab = cube.blockedByPrefab;
        Vector3 targetPos = cube.targetPos;
        bool active = cube.active;
        float speed = cube.speed;
        cube.enabled = false;


        CustomEndlessCube gCube = gridCube.AddComponent<CustomEndlessCube>();
        gCube.positionOnGrid = positionOnGrid;
        gCube.blockedByPrefab = blockedByPrefab;
        gCube.targetPos = targetPos;
        gCube.active = active;
        gCube.speed = speed;
        
        DestroyImmediate(cybergrid);
        if (cybergrid == null)
        {
            Debug.Log("cybergrid Deletion sucsessful");
        }
        
        CustomCybergrind cg = arenaGO.AddComponent<CustomCybergrind>();
        cg.gridCube = gridCube; 
        cg.prefabs = prefabs;
        cg.nms = navMeshSurface;
        cg.offset = 5;
        cg.glowMultiplier = 1f;


        if (patterns[patternIndex] != null)
        {
            cg.customPatterns = new ArenaPattern[1];
            cg.customPatterns[0] = patterns[patternIndex];
            cg.currentPatternNum = 0;
            cg.customPatternMode = true;
        }
        else
        {
            Debug.LogError("Arena pattern is null");
        }

        cg.spawnedPrefabs = new List<GameObject>();

        cg.Init();
        
        yield return null;
        
        if (cg.CurrentPatternPool != null && cg.CurrentPatternPool.Length > 0)
        {
            try
            {
                cg.LoadPattern(patterns[patternIndex]);
                Debug.Log("Pattern loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading pattern: {e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError("CurrentPatternPool is either null or empty");
        }

        Debug.Log("DoCGStuff completed");
    }

    void OpenMultiMenu()
    {
        Clogger.Log("Player clicked open multiplayer button");
        GameObject.Find("Main Menu (1)").SetActive(false);
        MMObject.SetActive(true);
    }

    public void LoadMultiplayerScene()
    {
        if (loadScene == false) return;
        string[] scenes = CompMultiplayerMain.sceneBundle.GetAllScenePaths();
        if (scenes.Length > 0)
        {
            string name = Path.GetFileNameWithoutExtension(scenes.FirstOrDefault());

            if (!string.IsNullOrEmpty(name))
            {
                Clogger.Log("Loading multiplayer scene");
                
                MonoSingleton<SceneHelper>.Instance.loadingBlocker.SetActive(true);
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
                MonoSingleton<SceneHelper>.Instance.loadingBlocker.SetActive(false);
            }
            else
            {
                Clogger.LogWarning("Scene 'PVPWorld' not found.");
            }
        }
        else
        {
            Clogger.LogWarning("No scenes found in the scene bundle.");
        }
    }


    public static void InvokePrivate<T>(T target, string methodName, object[] paramz)
    {

        Type[] parameterTypes = paramz != null ? paramz.Select(p => p.GetType()).ToArray() : new Type[0];

        MethodInfo method = AccessTools.Method(typeof(T), methodName, parameterTypes);

        if (method != null)
        {
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, paramz);
                }
                else
                {
                    method.Invoke(target, paramz);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error invoking method: {ex.Message}");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Method not found!");
        }
    }
}

public class EscMenu____ : MonoBehaviour
{
    public GameObject Menu;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Menu.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}