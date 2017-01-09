using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity.Settings;

public class BuildProjectWizzard : EditorWindow
{
    public static EArea SelectedArea = ProjectSetting.Area;
    public static EVersion SelectedVersion = ProjectSetting.Version;

    //新增的Define大小寫要與程式碼中的相同
    enum DefineSymbol 
    {
        ShowDebug = 0,
        GenLuaWrap,
        ForceUseAsset,
        ThirdParty,
        ThirdParty_Test,
        Alipay,
        //新增的Define要加在Count之前
        Count
    }
    private static int defineSymbolCount = (int)DefineSymbol.Count;
    private static bool[] defineSymbolFlags = new bool[defineSymbolCount];

    private const string ASTAR = "ASTAR_NO_JSON;ASTAR_NO_ZIP; ASTAR_FAST_NO_EXCEPTIONS;ASTAR_OPTIMIZE_POOLING;ASTAR_GRID_NO_CUSTOM_CONNECTIONS;ASTAR_LEVELGRIDNODE_FEW_LAYERS;ASTAR_NO_LOGGING;ASTAR_NO_PENALTY;ASTAR_NoGUI;ASTAR_RECAST_LARGER_TILES";

    private const string ICON_PATH = "Assets/02.Arts/AppIcon/{0}/ICON.png";
    private const string KEY_STORE_NAME = "/06.AndroidKey/cg.keystore";
    private const string KEY_STORE_PASS = "70541704";
    private const string KEY_ALIAS_NAME = "cg";
    private const string KEY_ALIAS_PASS = "70541704";

    [MenuItem("Artifact/Build Project Wizzard")]
    static void Init()
    {
        ParseDefineSymbol();
        EditorWindow.GetWindow(typeof(BuildProjectWizzard), true);
    }

	void OnFocus()
	{
		ParseDefineSymbol();
	}

    static void ParseDefineSymbol()
    {
        string[] currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');

        for (int i = 0; i < currentDefineSymbols.Length; i++)
        {
            for (DefineSymbol symbol = DefineSymbol.ShowDebug; symbol < DefineSymbol.Count; symbol++)
            {
                if (string.Compare(currentDefineSymbols[i], symbol.ToString()) == 0)
                {
                    defineSymbolFlags[(int)symbol] = true;
                    break;
                }
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUILayout.Label("地區", EditorStyles.boldLabel);
        SelectedArea = (EArea)EditorGUILayout.EnumPopup("Select Area:", SelectedArea);

        GUILayout.Label("版本", EditorStyles.boldLabel);
        SelectedVersion = (EVersion)EditorGUILayout.EnumPopup("Select Version:", SelectedVersion);

        GUILayout.Label("Define", EditorStyles.boldLabel);
        for (DefineSymbol symbol = DefineSymbol.ShowDebug; symbol < DefineSymbol.Count; symbol++)
            defineSymbolFlags[(int)symbol] = EditorGUILayout.Toggle(symbol.ToString(), defineSymbolFlags[(int)symbol]);

        EditorGUILayout.Space();

        GUILayout.Label("Step1.按 SwitchDefine 切換 \"Define\"", style);
        if (GUILayout.Button("SwitchDefine", GUILayout.Height(40)))
          SetDefine();

        EditorGUILayout.Space();

        GUILayout.Label("Step2.轉完圈圈，才能按SetPlayerSetting", style);
        if (GUILayout.Button("SetPlayerSetting", GUILayout.Height(40)))
          SetPlayerSetting();
    }

    void SetDefine()
    {
        string defineSymbol = GetDefineSymbol();
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbol);
    }

    void SetPlayerSetting()
    {
        SetLuaDefine();
        SetIcon();
        SetProjectSetting();
        SetAndroidSetting();
        SetFacebook();
    }

    string GetDefineSymbol()
    {
        StringBuilder defineSymbol = new StringBuilder();

        //地區
        defineSymbol.Append(SelectedArea.ToString() + ";");

        //版本
        defineSymbol.Append(SelectedVersion.ToString() + ";");

        //Define
        for (DefineSymbol symbol = DefineSymbol.ShowDebug; symbol < DefineSymbol.Count; symbol++)
        {
            if (defineSymbolFlags[(int)symbol])
                defineSymbol.Append(symbol.ToString() + ";");
        }

        //aStar
        defineSymbol.Append(ASTAR+ ";");

        return defineSymbol.ToString();
    }

    private static void SetProjectSetting()
    {
        PlayerSettings.companyName = ProjectSetting.CompanyName;
        PlayerSettings.productName = ProjectSetting.ProductName;
        PlayerSettings.bundleIdentifier = ProjectSetting.Bundle_ID;
        PlayerSettings.bundleVersion = ProjectSetting.BundleVersion;
    }

    private static void SetAndroidSetting()
    {
        PlayerSettings.Android.keystoreName = Application.dataPath + KEY_STORE_NAME;
        PlayerSettings.Android.keystorePass = KEY_STORE_PASS;
        PlayerSettings.Android.keyaliasName = KEY_ALIAS_NAME;
        PlayerSettings.Android.keyaliasPass = KEY_ALIAS_PASS;
        PlayerSettings.Android.bundleVersionCode = ProjectSetting.BundleVersionCode;
    }

    private static void SetIcon()
    {
        string IconPatch;
#if HK
		IconPatch = "HK";
#else
        IconPatch = "TW";
#endif
        var types = new[] { BuildTargetGroup.iOS, BuildTargetGroup.Android };
        foreach (var type in types)
        {
            int[] sizeForTarget;
            string path;
            Texture2D[] IconTexture = PlayerSettings.GetIconsForTargetGroup(type);

            for (int i = 0; i < IconTexture.Length; i++)
                IconTexture[i] = null;

            IconTexture = null;
            sizeForTarget = PlayerSettings.GetIconSizesForTargetGroup(type);
            IconTexture = new Texture2D[sizeForTarget.Length];
            for (int i = 0; i < sizeForTarget.Length; i++)
            {
                path = string.Format(ICON_PATH, IconPatch);
                IconTexture[i] = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                //Debug.LogError(IconPatch + " : " + path);
            }

            PlayerSettings.SetIconsForTargetGroup(type, IconTexture);
        }
    }

    const string LUAHELPER_PATH = "Assets/03.Scripts/Lua/LuaHelper.cs";

    public static void SetLuaDefine()
    {
        if (!File.Exists(LUAHELPER_PATH))
            return;
        bool startDefine = false;
        try
        {
            List<string> contents = new List<string>();

            using (StreamReader sr = File.OpenText(LUAHELPER_PATH))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.Equals("    static void InitAppDefine()", line))
                    {
                        startDefine = true;
                        contents.Add(line);
                        line = sr.ReadLine();
                        contents.Add(line);
                        string[] currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
                        contents.Add("\t\tdefines = new HashSet<string>();");
                        for(int i = 0; i < currentDefineSymbols.Length; i++)
                        {
                            line = "\t\tdefines.Add(\"" + currentDefineSymbols[i] + "\");";
                            contents.Add(line);
                        }
                    }

                    if (startDefine && string.Equals("    }", line))
                        startDefine = false;

                    if (!startDefine)
                        contents.Add(line);
                }

                sr.Close();
            }

            File.WriteAllLines(LUAHELPER_PATH, contents.ToArray());
        }
        catch
        {
        }

        AssetDatabase.Refresh();
    }

    #region FB
    enum FBKind { IOSPLIST = 0, AndroidXML }

    const string FACEBOOK_PLIAT_PATH = "Assets/Editor/Prime31/plistAdditions.plist";
    const string FACEBOOK_URL = "fb{0}";
    const string FACEBOOK_XML = "<string>{0}</string>";

    const string FACEBOOK_ANDROIDXML_PATH = "Assets/Plugins/Android/AndroidManifest.xml";
    const string FACEBOOK_ANDROIDXML = @"    <meta-data android:name=""com.facebook.sdk.ApplicationId"" android:value=""{0}"" />";
    const string FACEBOOK_ANDROIDXML_PROVIDER = @"    <provider android:name=""com.facebook.FacebookContentProvider"" android:authorities=""com.facebook.app.FacebookContentProvider{0}"" android:exported=""true"" />";

    static readonly string[] FB_IDAY = new string[] { "755114554581301", "1617806281847940" };
    static readonly string[] FB_URLIDAY = new string[] { "fb755114554581301", "fb1617806281847940" };
    static readonly string[] FB_NAME = new string[] { "artifact", "artifacthk" };

    public static void SetFacebook()
    {
        #region FBSDK
        FacebookSettings.AppIds.Clear();
        FacebookSettings.AppIds.Add(ProjectSetting.FacebookId);

        FacebookSettings.AppLabels.Clear();
        FacebookSettings.AppLabels.Add(ProjectSetting.FacebookName);
        #endregion

        #region IOSPLIST
        if (!File.Exists(FACEBOOK_PLIAT_PATH))
            return;

        try
        {
            List<string> defines = new List<string>();

            using (StreamReader sr = File.OpenText(FACEBOOK_PLIAT_PATH))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    line = CheckFBID(FBKind.IOSPLIST, line);
                    line = CheckFBURLID(FBKind.IOSPLIST, line);
                    line = CheckFBNAME(FBKind.IOSPLIST, line);

                    defines.Add(line);
                }

                sr.Close();
            }

            File.WriteAllLines(FACEBOOK_PLIAT_PATH, defines.ToArray());
        }
        catch
        {
        }
        #endregion

        #region AndroidXML
        if (!File.Exists(FACEBOOK_ANDROIDXML_PATH))
            return;

        try
        {
            List<string> defines = new List<string>();

            using (StreamReader sr = File.OpenText(FACEBOOK_ANDROIDXML_PATH))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    line = CheckFBID(FBKind.AndroidXML, line);
                    line = CheckFBURLID(FBKind.AndroidXML, line);
                    defines.Add(line);
                }

                sr.Close();
            }

            File.WriteAllLines(FACEBOOK_ANDROIDXML_PATH, defines.ToArray());
        }
        catch
        {
        }
        #endregion
    }

    static string CheckFBID(FBKind kind, string id)
    {
        string info = id;
        string check = string.Empty;

        for (int i = 0; i < FB_IDAY.Length; i++)
        {
            switch (kind)
            {
                case FBKind.IOSPLIST:
                    check = string.Format(FACEBOOK_XML, FB_IDAY[i]);
                    break;
                case FBKind.AndroidXML:
                    check = string.Format(FACEBOOK_ANDROIDXML_PROVIDER, FB_IDAY[i]);
                    break;
            }

            if (info.IndexOf(check) > -1)
                info = info.Replace(FB_IDAY[i], ProjectSetting.FacebookId);
        }

        return info;
    }

    static string CheckFBURLID(FBKind kind, string urlid)
    {
        string info = urlid;
        string check = string.Empty;

        for (int i = 0; i < FB_URLIDAY.Length; i++)
        {
            switch (kind)
            {
                case FBKind.IOSPLIST:
                    check = string.Format(FACEBOOK_XML, FB_URLIDAY[i]);
                    break;
                case FBKind.AndroidXML:
                    check = string.Format(FACEBOOK_ANDROIDXML, FB_URLIDAY[i]);
                    break;
            }

            if (info.IndexOf(check) > -1)
                info = info.Replace(FB_URLIDAY[i], string.Format(FACEBOOK_URL, ProjectSetting.FacebookId));
        }

        return info;
    }

    static string CheckFBNAME(FBKind kind, string urlname)
    {
        string info = urlname;
        string check = string.Empty;

        for (int i = 0; i < FB_NAME.Length; i++)
        {
            switch (kind)
            {
                case FBKind.IOSPLIST:
                    check = string.Format(FACEBOOK_XML, FB_NAME[i]);
                    break;
            }

            if (info.IndexOf(check) > -1)
                info = info.Replace(FB_NAME[i], ProjectSetting.FacebookName);
        }

        return info;
    }
    #endregion

    #region 自動產檔接口
    static string GetDefine(EArea area, EVersion version, bool thirdParty, bool thirdParty_Test)
    {
        string define = "";

        //地區 + Define
        switch (area)
        {
            case EArea.TW:
                define = define + "TW;GenLuaWrap;";
                break;
            case EArea.HK:
                define = define + "HK;GenLuaWrap;";
                break;
        }

        //版本
        switch (version)
        {
            case EVersion.Debug:
                define = define + "Debug;ShowDebug;";
                break;
            case EVersion.QA:
                define = define + "QA;ShowDebug;";
                break;
            case EVersion.Release:
                define = define + "Release;";
                break;
        }

        if (thirdParty)
        {
            define = define + "ThirdParty;";
            switch (area)
            {
              case EArea.HK:
                  define = define + "Alipay;";
                  break;
            }
        }           

        if (thirdParty_Test)
            define = define + "ThirdParty_Test;";

        //Astar
        define = define + ASTAR;

        return define;
    }

    public static void SetDefineAndroid_TW()
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, GetDefine(EArea.TW, EVersion.Debug, false, false));
    }

    public static void BuildAndroid_TW()
    {
        BuildAndroidBinary(EArea.TW, false, true);
    }

    public static void BuildAndroid_TW_ThirdParty()
    {
        BuildAndroidBinary(EArea.TW, true, true);
    }

    public static void SetDefineAndroid_HK()
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, GetDefine(EArea.HK, EVersion.Debug, false, false));
    }

    public static void BuildAndroid_HK()
    {
        BuildAndroidBinary(EArea.HK, false, true);
    }

    public static void SetDefineAndroid_HK_ThirdParty()
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, GetDefine(EArea.HK, EVersion.Debug, true, false));
    }

    public static void BuildAndroid_HK_ThirdParty()
    {
        BuildAndroidBinary(EArea.HK, true, true);
    }

    public static void SetDefineIOS_TW()
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, GetDefine(EArea.TW, EVersion.Debug, false, false));
    }

    public static void BuildIOS_TW_Debug()
    {
        BuildIOSBinary(EArea.TW, EVersion.Debug, false, false, true);
    }

    public static void BuildIOS_TW_QA()
    {
        BuildIOSBinary(EArea.TW, EVersion.QA, false, false, true);
    }

    public static void BuildIOS_TW_Release()
    {
        BuildIOSBinary(EArea.TW, EVersion.Release, false, false, true);
    }

    public static void SetDefineIOS_HK()
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, GetDefine(EArea.HK, EVersion.Debug, false, false));
    }

    public static void BuildIOS_HK_Debug()
    {
        BuildIOSBinary(EArea.HK, EVersion.Debug, false, false, true);
    }

    public static void BuildIOS_HK_QA()
    {
        BuildIOSBinary(EArea.HK, EVersion.QA, false, false, true);
    }

    public static void BuildIOS_HK_Release()
    {
        BuildIOSBinary(EArea.HK, EVersion.Release, false, false, true);
    }

    public static void SetDefineIOS_HK_ThirdParty()
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, GetDefine(EArea.HK, EVersion.Debug, true, false));
    }

    public static void BuildIOS_HK_ThirdParty_Debug()
    {
        BuildIOSBinary(EArea.HK, EVersion.Debug, true, true, true);
    }

    public static void BuildIOS_HK_ThirdParty_QA()
    {
        BuildIOSBinary(EArea.HK, EVersion.QA, true, true, true);
    }

    public static void BuildIOS_HK_ThirdPartyTest_Release()
    {
        BuildIOSBinary(EArea.HK, EVersion.Release, true, true, true);
    }

    public static void BuildIOS_HK_ThirdParty_Release()
    {
        BuildIOSBinary(EArea.HK, EVersion.Release, true, false, true);
    }

    static void BuildAndroidBinary(EArea area, bool thirdParty, bool buildResource)
    {       
        if (thirdParty)
        {
            BuildPlayer(BuildTarget.Android, area, EVersion.Debug, true, true, AndroidTargetDevice.FAT);
            BuildPlayer(BuildTarget.Android, area, EVersion.QA, true, true, AndroidTargetDevice.FAT); 
            BuildPlayer(BuildTarget.Android, area, EVersion.Release, true, true, AndroidTargetDevice.FAT);
            BuildPlayer(BuildTarget.Android, area, EVersion.Release, true, false, AndroidTargetDevice.FAT);                     
        }
        else
        {
            BuildPlayer(BuildTarget.Android, area, EVersion.Debug, false, false, AndroidTargetDevice.FAT);    
            BuildPlayer(BuildTarget.Android, area, EVersion.QA, false, false, AndroidTargetDevice.FAT);   
            BuildPlayer(BuildTarget.Android, area, EVersion.Release, false, false, AndroidTargetDevice.FAT);     
        }       
    }

    static void BuildIOSBinary(EArea area, EVersion version, bool thirdParty, bool thirdPartyTest, bool buildResource)
    {
        BuildPlayer(BuildTarget.iOS, area, version, thirdParty, thirdPartyTest);
    }

    static void BuildPlayer(BuildTarget target, EArea area, EVersion version, bool thirdParty, bool thirdPartyTest, AndroidTargetDevice targetDevice = AndroidTargetDevice.FAT)
    {
        string outputPath = "";
        switch (target)
        {
            case BuildTarget.Android:
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, GetDefine(area, version, thirdParty, thirdPartyTest));
                PlayerSettings.Android.targetDevice = targetDevice;

                SetLuaDefine();
                SetAndroidSetting();

                outputPath = ProjectSetting.ProjectName + "_" + area.ToString() + "_" + version.ToString() + (thirdParty ? "_ThirdParty" : "") + (thirdPartyTest ? "Test" : "") + (target == BuildTarget.Android ? "_" + targetDevice.ToString() : "") + ".apk";
                break;
            case BuildTarget.iOS:
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, GetDefine(area, version, thirdParty, thirdPartyTest));
                SetLuaDefine();

                outputPath = ProjectSetting.ProjectName + "_" + area.ToString() + "_" + version.ToString() + (thirdParty ? "_ThirdParty" : "") + (thirdPartyTest ? "Test" : "") + (target == BuildTarget.Android ? "_" + targetDevice.ToString() : "") + ".app";
                break;
        }

        SetIcon();
        SetProjectSetting();
        SetFacebook();

        BuildPipeline.BuildPlayer(
            new string[2] { RMSGlobal.PROJECT_MAIN_SCENE, RMSGlobal.PROJECT_EMPTY_SCENE },
            outputPath,
            target,
            BuildOptions.None
        );
    }
    #endregion
}