using MelonLoader;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;


[assembly: MelonInfo(typeof(Beetleyes.Core), "Beetleyes", "1.0.0", "pablops", null)]
[assembly: MelonGame("Bugfight Studio", "Beetleball")]

namespace Beetleyes
{
    public class Core : MelonMod
    {
        public string assetFolder;
        public override void OnInitializeMelon()
        {
            string assemblyFolder = Path.GetDirectoryName(MelonAssembly.Assembly.Location);
            assetFolder = Path.Combine(assemblyFolder, "assets");
            LoggerInstance.Msg("BeetleEyes Initialized.");
        }

        Camera gameCamera;
        bool markersCreated = false;
        public override void OnUpdate()
        {
            if (gameCamera == null || !gameCamera.gameObject.activeInHierarchy)
                gameCamera = getCamera();

            if (hudRoot == null || !hudRoot.gameObject.activeInHierarchy)
            {
                hudRoot = null;
                FindHudRoot();
            }

            if (gameCamera == null && hudRoot == null && !onMainMenu)
            {
                // MelonLogger.Warning("Game camera or HUD root not found. Speedometer will not be displayed.");
                gameCamera = getCamera();
                return;
            }
            if (!onMainMenu)
            {
                var beetles = GameState.ActiveBeetles;

                if (!markersCreated || markers.Count != beetles.Count)
                {
                    DestroyAllMarkers();
                    DrawEyesOnBeetles();
                }

                UpdateBeetleMarkers();
            }
            else
            {
                if (markersCreated)
                {
                    DestroyAllMarkers();
                }
            }
            HandleInput();
        }

        private void HandleInput()
        {

            if (Input.GetKeyDown(KeyCode.F5))
            {
                
            }

        }

        Texture2D markerTexture;
        public RectTransform rt;
        public List<RawImage> markers = new();
        public void DrawEyesOnBeetles()
        {
            if (markerTexture == null)
            {
                markerTexture = LoadTexture("point.png");
            }

            markers.Clear();

            foreach (var beetle in GameState.ActiveBeetles)
            {


                GameObject go = new GameObject("Marker");
                go.transform.SetParent(hudRoot, false);

                RawImage img = go.AddComponent<RawImage>();
                img.texture = markerTexture;

                RectTransform rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(100, 100);

                markers.Add(img);
            }
            // MelonLogger.Msg($"Created {markers.Count} markers");
            markersCreated = true;
        }

        public Camera getCamera()
        {
            var cam = Camera.main;
            foreach (var brain in UnityEngine.Object.FindObjectsOfType<Unity.Cinemachine.CinemachineBrain>(true))
            {
                // MelonLogger.Msg($"Brain on: {brain.gameObject.name}");

                cam = brain.GetComponent<Camera>();
                // MelonLogger.Msg(cam != null
                //     ? $"Camera component found: {cam.name}"
                //     : "No Camera component");
            }
            return cam;
        }

        public void UpdateBeetleMarkers()
        {
            if (onMainMenu) return;
            // if (lastAllBeetles == null || lastAllBeetles.Count == 0) getAllBeetles();
            var beetles = GameState.ActiveBeetles;
            int i = 0;
            foreach (var beetle in GameState.ActiveBeetles)
            {
                if (i >= markers.Count) break;

                if (beetle == null)
                {
                    // MelonLogger.Msg($"Beetle {i} is null");
                    markers[i].enabled = false;
                    i++;
                    continue;
                }

                if (markers[i] == null)
                {
                    // MelonLogger.Msg($"Marker {i} is null");
                    i++;
                    continue;
                }

                if (!IsBeetleVisible(beetle))
                {
                    markers[i].enabled = false;
                    i++;
                    continue;
                }

                Vector3 screen = gameCamera.WorldToScreenPoint(beetle.transform.position);

                markers[i].enabled = true;
                markers[i].rectTransform.anchoredPosition = ScreenToHud(screen);
                float scale = 5f / screen.z;
                markers[i].rectTransform.sizeDelta = Vector2.one * 300f * scale;
                i++;
            }
        }

        public void DestroyAllMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker != null)
                {
                    GameObject.Destroy(marker.gameObject);
                }
            }
            markers.Clear();
            markersCreated = false;
        }
        
        //----------------------------------------------------------------------------------------------------
        //HELPER METHODS--------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------

        public Vector2 ScreenToHud(Vector3 screenPos)
        {
            RectTransform hud = hudRoot.GetComponent<RectTransform>();

            Vector2 localPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hud,
                screenPos,
                null,          // si el canvas es Overlay
                out localPos);

            return localPos;
        }

        public bool onMainMenu = true;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            markersCreated = false;
            //LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
            if (0 <= buildIndex && buildIndex <= 2)
            {
                onMainMenu = true;
            }
            else
            {
                onMainMenu = false;
            }
        }

        
        private Texture2D LoadTexture(string fileName)
        {
            string path = Path.Combine(assetFolder, fileName);

            if (!File.Exists(path))
            {
                MelonLogger.Error($"Texture not found: {path}");
                return null;
            }

            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, data);
            return tex;
        }

        public Transform hudRoot;


        private void FindHudRoot()
        {
            var canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.name == "Canvas" || canvas.name == "Canvas_CustomLobby")
                {
                    Transform playerHud = canvas.transform.Find("PlayerHUD");
                    if (playerHud != null)
                    {
                        hudRoot = playerHud.Find("Root");
                        if (hudRoot != null)
                        {
                            //MelonLogger.Msg("HUD Root found");
                            return;
                        }
                    }
                }
            }
        }

        public bool IsBeetleVisible(BeetleActor beetle)
        {
            Vector3 target = beetle.transform.position;
            Vector3 origin = gameCamera.transform.position;

            Vector3 dir = target - origin;
            float dist = dir.magnitude;

            if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist))
            {
                if (!hit.transform.IsChildOf(beetle.transform))
                {
                    return false;
                }
            }
            return true;
        }
    }
}