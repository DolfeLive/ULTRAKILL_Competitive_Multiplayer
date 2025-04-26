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

namespace UltraIDK
{
    [BepInPlugin("DolfeMods.Ultrakill.ULTRAKILL_Competitive_Multiplayer", "ULTRAKILL Competitive Multiplayer", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public static Class1 instance;

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
            sceneBundle = AssetBundle.LoadFromFile(Path.Combine(path, "ukcmworld.ukcmworld"));

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

            //print("running");
            //Time.timeScale = 1f;

        }
        
        void OnSceneLoaded(Scene scene, LoadSceneMode lsm)
        {
            Clogger.Log($"Scene loaded: {scene.name}");

            if (scene.name == "b3e7f2f8052488a45b35549efb98d902") // Main Menu
            {
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
            string[] scenePaths = Class1.sceneBundle.GetAllScenePaths();
            if (scenePaths.Length > 0)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePaths.FirstOrDefault());

                if (!string.IsNullOrEmpty(sceneName))
                {
                    //Clogger.Log($"Found scene: {sceneName}");
                    Clogger.Log("Loading multiplayer scene");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
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
}