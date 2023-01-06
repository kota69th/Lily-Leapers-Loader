using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using JetBrains.Annotations;
using Levels.Obstacles;
using Levels.FloorFall;
using FG.Common;
using FG.Common.LODs;
using FGClient;
using MPG.Utility;
using Fall_Factory;
using Il2CppSystem.Collections;
using System.Collections;
using static FG.Common.MetricConstants;
using System.IO;
using Il2CppSystem.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using FG.Common.Definition;
using FMODUnity;
using System.Diagnostics;
using UnhollowerBaseLib;
using System.Drawing;
using UnityEngine.Events;
using System.Linq.Expressions;

namespace LilyLeapersLoader
{
    public class Main : MelonMod
    {
        private bool LevelAndFGLoaded = false;
        GameObject SpawnForFF;
        string SpeedrunTimer;
        private float SpeedrunTimerEarly;
        MultiplayerStartingPosition[] MultiPlayerStartPos;
        Vector3 SpawnPos = new Vector3(-30.4235f, 2.6f, -178.5029f);
        GameObject IntroductionCamera = GameObject.Find("IntroCams");
        private bool StartStoppingIntroCam = false;
        Sprite IntroCanvasLogo;
        private bool LetReload = true;

        public override void OnApplicationStart()
        {
            LoggerInstance.Msg("Lily Leapers Loader | RELEASE | 3.0");
            LoggerInstance.Msg("A mod made by kota69th, possible thanks to Fall Factory.");
            MelonCoroutines.Start(MakeUI());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            StartStoppingIntroCam = true;
        }

        private static CameraDirector CameraDirectorLL;


        Texture2D tex;
        private void GetLilyLeapersLogo()
        {
            WebClient GetFile = new WebClient();
            byte[] DataAsByte = GetFile.DownloadData("https://github.com/kota69th/Z180/blob/main/UI_Medal_Icon_LilyLeapers.png?raw=true");
            System.Drawing.Image IMGWithSystem;
            using (var ms = new System.IO.MemoryStream(DataAsByte))
            {
                IMGWithSystem = System.Drawing.Image.FromStream(ms);
            }
            int size = IMGWithSystem.Width * IMGWithSystem.Height;
            tex = new Texture2D(IMGWithSystem.Width, IMGWithSystem.Height);
            if (ImageConversion.LoadImage(tex, DataAsByte))
            {
                IntroCanvasLogo = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.Tight);
            }
        }

        AssetBundle UIBundle;
        GameObject UICanvasObj;
        GameObject UIKeybindObj;
        GameObject UIMainObj;

        public void DownloadUIBundle()
        {
            WebClient Request = new WebClient();
            byte[] BundleAsByte = Request.DownloadData("https://github.com/kota69th/FranticExplorer-Bundles/blob/main/LilyLeapersUI.bundle?raw=true");
            UIBundle = AssetBundle.LoadFromMemory(BundleAsByte);
        }

        bool KeyBindHelpActive = false;
        System.Collections.IEnumerator MakeUI()
        {
            yield return new WaitForSeconds(7f);
            // Download the Assetbundle from Github.
            DownloadUIBundle();
            // Make the Object
            UICanvasObj = UnityEngine.Object.Instantiate(UIBundle.LoadAsset("UI Canvas (LLT)").Cast<GameObject>());
            UICanvasObj.name = "UI Canvas (LLT)";
            UnityEngine.Object.DontDestroyOnLoad(UICanvasObj);
            // Apply each button its own action.
            Tools.GetButton("Load").onClick.Action(delegate 
            {
                if (LetReload)
                {
                    SceneManager.LoadScene("FallGuy_Drumtop");
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    LevelAndFGLoaded = false;
                }
            });
            Tools.GetButton("Play").onClick.Action(delegate 
            { 
                MelonCoroutines.Start(SpawnPlayer());
            });
            Tools.GetButton("Help").onClick.Action(delegate 
            {
                MelonCoroutines.Start(ToggleKeyBind());
            });
            // Get UI and Keybind object because they are toggleable UI objects.
            UIMainObj = GameObject.Find("Main UI");
            UIKeybindObj = GameObject.Find("KeybindHelp");
            UIKeybindObj.SetActive(false);
        }

        bool DoKeyBindToggle = true;
        System.Collections.IEnumerator ToggleKeyBind()
        {
            if (DoKeyBindToggle)
            {
                DoKeyBindToggle = false;
                // Load it
                KeyBindHelpActive = !KeyBindHelpActive;
                UIKeybindObj.SetActive(KeyBindHelpActive);
                // Wait 7.5 seconds
                yield return new WaitForSeconds(7.5f);
                // Unload it
                KeyBindHelpActive = !KeyBindHelpActive;
                UIKeybindObj.SetActive(KeyBindHelpActive);
                DoKeyBindToggle = true;
            }
        }

            System.Collections.IEnumerator SpawnPlayer()
            {
            // We make Scene Reloading impossible for stability reasons.
            LetReload = false;
            // Create Spawn block for Fall Factory and Checkpoint System.
            SpawnForFF = GameObject.CreatePrimitive(PrimitiveType.Cube);
            SpawnForFF.GetComponent<Renderer>().material.color = new UnityEngine.Color(0f, 5f, 0f, 1f);
            UnityEngine.Object.Destroy((UnityEngine.Object)SpawnForFF.GetComponent<BoxCollider>());
            SpawnForFF.name = "Spawn";
            // Download the Lily Leapers logo from Github.
            GetLilyLeapersLogo();
            // Set Spawn Position.
            SpawnForFF.transform.position = new Vector3(-26.64f, 2.3948f, -178.41f);
            SpawnForFF.transform.rotation = Quaternion.Euler(0, 0, 0);
            CameraDirectorLL = UnityEngine.Object.FindObjectOfType<CameraDirector>();
            // Make the IntroCamera.
            foreach (Prefab_UI_Intro_Overlay UIIntro in UnityEngine.Object.FindObjectsOfType<Prefab_UI_Intro_Overlay>())
            {
                UIIntro.LevelNameText = "LILY LEAPERS";
                UIIntro.LevelIconSprite = IntroCanvasLogo;
                UIIntro.LevelDescriptionText = "Use the drums to bounce to the finish line!\nUsing Lily Leapers Loader!";
                GameObject.Find("title-tab").GetComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0.007f, 0.713f, 0.32f, 1f);
            }
            GameObject.Find("IntroCams").GetComponent<IntroCameras>().Play(1);
            GameObject.Find("IntroCanvas").SetActive(true);
            // Wait IntroCamera duration and then sapwn the Player.
            yield return new WaitForSeconds(CameraDirectorLL.IntroCamerasDuration);
            Fall_Factory.FFMainClass.SpawnFallGuy();
            // Start In-Game Instances and let the scene reloading possible again.
            LevelAndFGLoaded = true;
            LetReload = true;
        }

        private void Checkpoint()
        {
            SpawnForFF.transform.position = GameObject.Find("FallGuy").transform.position;
            SpawnForFF.GetComponent<Renderer>().material.color = new UnityEngine.Color(0f, 5f, 5f, 1f);
            return;
        }
        public override void OnUpdate()
        {
            // On Scene was loaded.
            if (StartStoppingIntroCam)
            {
                try
                {
                    GameObject IntroCams = GameObject.Find("CM_Intro_01");
                    IntroCams.active = false;
                    GameObject.Find("Main Camera Brain").transform.position = new Vector3(-179.874f, 175.222f, -182.275f);
                    GameObject.Find("Main Camera Brain").transform.rotation = Quaternion.Euler(33.3f, 64.2f, 0f);
                }
                catch { }
                StartStoppingIntroCam = false;
            }


            // Every In-Game based stuff.
            if (LevelAndFGLoaded)
            {
                try
                {
                    FFMainClass.fagus(true);
                } catch { }

                if (GameObject.Find("FallGuy").transform.position.y < -50)
                {
                    GameObject.Find("FallGuy").transform.position = SpawnForFF.transform.position;
                }

                if (Input.GetKeyDown(KeyCode.C))
                {
                    Checkpoint();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (LevelAndFGLoaded)
                    {
                        SpawnForFF.GetComponent<Renderer>().material.color = new UnityEngine.Color(0f, 5f, 0f, 1f);
                        SpawnForFF.transform.position = new Vector3(-26.64f, 2.3948f, -178.41f);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    return;
                }
                if (Cursor.visible)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    return;
                }
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                UIMainObj.SetActive(!UIMainObj.activeSelf);
            }
        }
    }

    internal static class Tools
    {
        // Make UI easier to do.
        public static void Action(this UnityEvent UEvent, Action WhatToDo)
        {
            UEvent.AddListener(WhatToDo);
        }

        public static Button GetButton(string Inst)
        {
            Button Button = (Button)null;
            if(Inst == "Load")
            {
                Button = GameObject.Find("LoadLL").GetComponent<Button>();
            }
            if (Inst == "Play")
            {
                Button = GameObject.Find("PlayLL").GetComponent<Button>();
            }
            if (Inst == "Help")
            {
                Button = GameObject.Find("KeyBinds").GetComponent<Button>();
            }

            return Button;
        }
    }


}