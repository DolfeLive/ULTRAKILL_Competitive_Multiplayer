using System;
using UnityEngine;
using ULTRAKILL;
using UnityEngine.UI;
using BepInEx;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using HarmonyLib;
using Clogger = UltraIDK.Logger;
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

namespace UltraIDK
{
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
            /*
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
            patterns.Add(arena);

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
            patterns.Add(arena2);
            */
            
            string GeneratePattern(int width, int height, int minValue, int maxValue)
            {
                System.Random rand = new System.Random();
                StringBuilder sb = new StringBuilder();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int number = rand.Next(minValue, maxValue + 1);
                        sb.Append($"({number})");
                    }
                    if (y != height - 1)
                        sb.Append("\n");
                }

                return sb.ToString();
            }
            Task addPatterns = new Task(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    string pattern = GeneratePattern(32, 32, 0, 5);
                    ArenaPattern arena = ScriptableObject.CreateInstance<ArenaPattern>();
                    arena.name = $"randomArena{i}";
                    arena.heights = pattern;
                    patterns.Add(arena);
                }
                
            });
            addPatterns.Start();
            
            // string pattern = GeneratePattern(32, 32, 0, 5);            
            // ArenaPattern randomArena = ScriptableObject.CreateInstance<ArenaPattern>();
            // randomArena.name = "randomArena";
            // randomArena.heights = pattern;
            // patterns.Add(randomArena);

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
        void NewMovementRespawn()
        {
            Debug.Log("=== RESPAWN SEQUENCE STARTED ===");

            Debug.Log("Setting CameraController.activated");
            if (CameraController.Instance != null)
            {
                CameraController.Instance.activated = true;
                Debug.Log("CameraController.activated set to true");
            }
            else
                Debug.LogError("CameraController.Instance is null");

            Debug.Log("Enabling NewMovement component");
            if (this.nm != null)
            {
                this.nm.enabled = true;
                Debug.Log("NewMovement.enabled set to true");
            }
            else
                Debug.LogError("this.nm reference is null");

            Debug.Log("Setting NewMovement.activated");
            if (nm != null)
            {
                nm.activated = true;
                Debug.Log("nm.activated set to true");
            }
            else
                Debug.LogError("nm reference is null");

            Debug.Log("Setting camera occlusion culling");
            if (CameraController.Instance != null && CameraController.Instance.cam != null)
            {
                CameraController.Instance.cam.useOcclusionCulling = true;
                Debug.Log("Camera occlusion culling enabled");
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

            Debug.Log("Attempting to stop slide");
            try
            {
                nm.StopSlide();
                Debug.Log("Slide stopped successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping slide: {ex.Message}");
            }

            Debug.Log("Setting player health stats");
            nm.hp = 150;
            nm.boostCharge = 299f;
            nm.antiHp = 0f;
            nm.antiHpCooldown = 0f;
            Debug.Log($"Health set to {nm.hp}, boost charge to {nm.boostCharge}");

            Debug.Log("Setting rigidbody constraints");
            if (nm.rb != null)
            {
                nm.rb.constraints = nm.defaultRBConstraints;
                Debug.Log("Rigidbody constraints reset");
            }
            else
                Debug.LogWarning("Player rigidbody is null");

            Debug.Log("Handling death sequence");
            try
            {
                Debug.Log("Attempting to access deathSequence");
                var deathSequence = nm.deathSequence;
                Debug.Log($"deathSequence reference obtained: {(deathSequence != null ? "Valid" : "Null")}");

                if (deathSequence != null && deathSequence.gameObject != null)
                {
                    Debug.Log("Disabling deathSequence GameObject");
                    deathSequence.gameObject.SetActive(false);
                    Debug.Log("deathSequence GameObject disabled");
                }
                else
                {
                    Debug.LogWarning($"deathSequence issues: {(deathSequence == null ? "deathSequence is null" : "deathSequence.gameObject is null")}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in handling deathSequence: {ex.Message}");
                Debug.LogError($"Exception type: {ex.GetType().Name}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
            //DeathSequence

            //Transform blackscreen = transform.Find("Canvas/BackScreen");
            //if (blackscreen != null) 
            //    blackscreen.gameObject.SetActive(false);
            //Shader.SetGlobalFloat("_Sharpness", 0);
            //Shader.SetGlobalFloat("_Deathness", 0);

            Debug.Log("Enabling character controller");
            if (nm.cc != null)
            {
                nm.cc.enabled = true;
                Debug.Log("Character controller enabled");
            }
            else
                Debug.LogWarning("Character controller is null");

            Debug.Log("Resetting power meter");
            if (PowerUpMeter.Instance != null)
            {
                PowerUpMeter.Instance.juice = 0f;
                Debug.Log("Power meter juice reset to 0");
            }
            else
                Debug.LogWarning("PowerUpMeter.Instance is null");

            Debug.Log("Setting up weapons");
            GunControl gunc = nm != null ? nm.GetComponentInChildren<GunControl>() : null;
            Debug.Log($"GunControl component {(gunc != null ? "found" : "not found")}");
            if (gunc != null)
            {
                gunc.YesWeapon();
                Debug.Log("Weapons enabled");
            }
            else
                Debug.LogWarning("GunControl component not found");

            Debug.Log("Setting UI and player state");
            if (nm.screenHud != null)
            {
                nm.screenHud.SetActive(false);
                Debug.Log("Screen HUD disabled");
            }
            else
                Debug.LogWarning("screenHud is null");

            nm.dead = false;
            Debug.Log("Player marked as not dead");

            Debug.Log("Setting time controller");
            if (TimeController.Instance != null)
            {
                TimeController.Instance.controlPitch = true;
                Debug.Log("Time controller pitch control enabled");
            }
            else
                Debug.LogWarning("TimeController.Instance is null");

            Debug.Log("Handling hook arm");
            if (HookArm.Instance != null)
            {
                HookArm.Instance.Cancel();
                Debug.Log("Hook arm canceled");
            }
            else
                Debug.LogWarning("HookArm.Instance is null");

            Debug.Log("Setting up punch mechanics");
            if (nm.punch != null)
            {
                nm.punch.activated = true;
                Debug.Log("Punch activated");
                nm.punch.YesFist();
                Debug.Log("Fist enabled");
            } 
            else
                Debug.LogWarning("Player punch component is null");

            nm.slowMode = false;
            Debug.Log("Slow mode disabled");

            Debug.Log("Setting up weapon charges");
            if (WeaponCharges.Instance != null)
            {
                WeaponCharges.Instance.MaxCharges();
                Debug.Log("Weapon charges maximized");

                if (WeaponCharges.Instance.rocketLauncher != null)
                {
                    WeaponCharges.Instance.rocketLauncher.UnfreezeRockets();
                    Debug.Log("Rockets unfrozen");
                }
                else
                    Debug.LogWarning("RocketLauncher is null");
            }
            else
                Debug.LogWarning("WeaponCharges.Instance is null");

            Debug.Log("Stopping camera shake");
            if (nm.cc != null)
            {
                nm.cc.StopShake();
                Debug.Log("Camera shake stopped");
            }
            else
                Debug.LogWarning("Cannot stop camera shake - character controller is null");

            Debug.Log("Activating player");
            try
            {
                nm.ActivatePlayer();
                Debug.Log("Player activated successfully");
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
                //DoCGStuff();
            }
        }

        IEnumerator DoCGStuff()
        {
            yield return new WaitForSeconds(0.5f);

            EndlessGrid cybergrid = FindFirstObjectByType<EndlessGrid>();
            if (cybergrid == null)
            {
                Debug.LogError("Cybergrind is null");
                yield break;
            }


            GameObject arenaGO = cybergrid.gameObject;
            PrefabDatabase prefabs = cybergrid.prefabs;
            GameObject gridCube = cybergrid.gridCube;
            NavMeshSurface navMeshSurface = cybergrid.nms;
            EndlessCube[][] Cubes = cybergrid.cubes;

            for (int i = 0; i < Cubes.Length; i++)
            {
                for (int j = 0; j < Cubes[i].Length; j++)
                {
                    Destroy(Cubes[i][j]);
                }
            }

            GameObject combinedGridStaticObject = cybergrid.combinedGridStaticObject;
            float offset = 5f;
            float glowMult = 1f;
            
            DestroyImmediate(cybergrid);
            if (cybergrid == null)
            {
                Debug.Log("Deletion sucsessful");
            }
            
            CustomCybergrind cg = arenaGO.AddComponent<CustomCybergrind>();
            cg.gridCube = gridCube; 
            cg.prefabs = prefabs;
            cg.nms = navMeshSurface;
            cg.offset = 5;
            cg.glowMultiplier = 1f;
            cg.combinedGridStaticObject = combinedGridStaticObject;
            
            cg.Init();
            
            if (cg == null)
            {
                Debug.LogError("cg creation failed!");
                yield break;
            }
            /*
            [Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.CreateSubmeshes (System.Collections.Generic.List`1[T] materials) (at <818eee5ee85c4958aefb5380d16abe11>:0)
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.SetupStaticGridMesh () (at <818eee5ee85c4958aefb5380d16abe11>:0)
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.TrySetupStaticGridMesh () (at <818eee5ee85c4958aefb5380d16abe11>:0)
ULTRAKILL_Competitive_Multiplayer.CustomCybergrind.Init () (at <818eee5ee85c4958aefb5380d16abe11>:0)
UltraIDK.CompMultiplayerMain+<DoCGStuff>d__27.MoveNext () (at <818eee5ee85c4958aefb5380d16abe11>:0)
UnityEngine.SetupCoroutine.InvokeMoveNext (System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) (at <dfbdd4656e0844829a5285bde9c1a365>:0)
*/
            
            
            print($"cubes: {cg.cubes}");
            
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
            if (cg.gridCube == null)
            {
                print("Gridcbue is null");
            }
            //InvokePrivate<CustomCybergrind>(cg, "SetGlowColor", new object[] { true });
            if (cg.CurrentPatternPool != null && cg.CurrentPatternPool.Length > 0)
            {
                if (cg.currentPatternNum >= 0 && cg.currentPatternNum < cg.CurrentPatternPool.Length)
                {
                    cg.LoadPattern(patterns[patternIndex]);
                    //InvokePrivate<CustomCybergrind>(cg, "LoadPattern", new object[] { cg.CurrentPatternPool[cg.currentPatternNum] });
                }
                else
                {
                    Debug.LogError("currentPatternNum is out of bounds");
                }
            }
            else
            {
                Debug.LogError("CurrentPatternPool is either null or empty");
            }
        }

        void OpenMultiMenu()
        {
            Clogger.Log("Player clicked open multiplayer button");
            GameObject.Find("Main Menu (1)").SetActive(false);
            MMObject.SetActive(true);
        }

        public void LoadMultiplayerScene()
        {
            //SceneHelper.LoadScene("uk_construct");
            //return;

            if (loadScene == false) return;
            string[] scenes = CompMultiplayerMain.sceneBundle.GetAllScenePaths();
            if (scenes.Length > 0)
            {
                string name = Path.GetFileNameWithoutExtension(scenes.FirstOrDefault());

                if (!string.IsNullOrEmpty(name))
                {
                    //Clogger.Log($"Found scene: {sceneName}");
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

    class MyTargetClass
    {
        private static void MyPrivateStaticMethod(string message)
        {
            UnityEngine.Debug.Log("Static Method Called: " + message);
        }
    }
}