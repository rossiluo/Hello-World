using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Artifact : MonoBehaviour
{
    enum EState : byte
    {
        None,
        Initializing,
        RMSInitializing,
        LuaInitializing,
        HotfixRunning,
    }

    static EState state = EState.None;
    static bool initialized = false;

    public string luaEntryName = "LuaArtifact";

    public GameObject terrainObj;
    public GameObject terrainObj_005;
    public GameObject floorObj;
    public GameObject allObj;
    public GameObject city1;
    public GameObject city2;
    public GameObject msg;
    public GameObject cgui;
    public GameObject UIMessage;
    public GameObject war;

    void OnGUI()
    {
        //    if (GUI.Button(new Rect(0, 800, 100, 100), "Trees"))
        //    {
        //        if (!msg)
        //            msg = GameObject.Find("SCENE_005/Trees");
        //        msg.SetActive(!msg.activeSelf);

        //        if (!floorObj)
        //            floorObj = GameObject.Find("SCENE_005/mountain");
        //        floorObj.SetActive(!floorObj.activeSelf);

        //        if (!allObj)
        //            allObj = GameObject.Find("SCENE_005/Obj");
        //        allObj.SetActive(!allObj.activeSelf);

        //        if (!city1)
        //            city1 = GameObject.Find("SCENE_005/City1");
        //        city1.SetActive(!city1.activeSelf);

        //        if (!city2)
        //            city2 = GameObject.Find("SCENE_005/City2");
        //        city2.SetActive(!city2.activeSelf);
        //    }

        //    if (GUI.Button(new Rect(0, 900, 100, 100), "UI"))
        //    {
        //        if (!cgui)
        //            cgui = GameObject.Find("CGUI/Camera");
        //        cgui.SetActive(!cgui.activeSelf);
        //    }

        //if (GUI.Button(new Rect(0, 1000, 100, 100), "war"))
        //{
        //    //if (!terrainObj_005)
        //    //    terrainObj_005 = GameObject.Find("SCENE_005/SceneObj_005_006_terrain");
        //    //terrainObj_005.SetActive(!terrainObj_005.activeSelf);

        //    if (!war)
        //        war = GameObject.Find("10171/2/WA_R");
        //    war.SetActive(!war.activeSelf);
        //}

        //if (GUI.Button(new Rect(0, 1100, 100, 100), "UIMessage"))
        //{
        //    if (!UIMessage)
        //        UIMessage = GameObject.Find("CGUI/UIMessage");
        //    UIMessage.SetActive(!UIMessage.activeSelf);
        //}
    }

    void Awake()
    {
        Lua.luaMainScriptName = luaEntryName;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = ProjectSetting.FrameRate;
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
    }

    void Start()
    {
        Artifact.Restart();
    }

    void Update()
    {
        switch (state)
        {
            case EState.None:
				state = EState.Initializing;
                Debug.LogWarning(state);

                Initialize();
                break;
            case EState.Initializing:
                if (initialized)
                {
					state = EState.RMSInitializing;
                    Debug.LogWarning(state);

                    CGResource.Initialize();
                }
                break;
            case EState.RMSInitializing:
                if (CGResource.initialized)
                {
					state = EState.LuaInitializing;
                    Debug.LogWarning(state);

                    Lua.Initialize();
                }
                break;
            case EState.LuaInitializing:
                if (Lua.initialized)
                {
					state = EState.HotfixRunning;
					Debug.LogWarning(state);

                    Lua.DoFunction(Lua.luaMainScriptName, "Initialize");
                }
                break;
            case EState.HotfixRunning:
                Lua.DoFunction(Lua.luaMainScriptName, "Update");
                break;
        }
    }

    void Initialize()
    {
        initialized = true;
    }

    public static void Restart()
    {
        state = EState.None;
        initialized = false;
    }

    public static void MemoryCollect()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        Lua.MemoryCollect();
    }

    public static void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        RMSWebLoad.Reset();
        CompressManager.Dispose();
    }
}