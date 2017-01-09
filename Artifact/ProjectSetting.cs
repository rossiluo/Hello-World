using UnityEngine;
using System.Text;
using System.Collections;

public enum EArea { TW, HK }
public enum EVersion { Debug, QA, Release }
public enum EPlatform { IOS, Android }

public static class ProjectSetting
{
    #region BundleId
#if HK
    public const EArea Area = EArea.HK;
    public const string ProjectName = "ArtifactHK";

    public const string CompanyName = "980x";
    public const string ProductName = "天晶Online";

    public const string BundleVersion = "1.0.4";
    public const int BundleVersionCode = 1;
#else
    public const EArea Area = EArea.TW;
    public const string ProjectName = "Artifact";

    public const string CompanyName = "Chinesegamer";
    public const string ProductName = "神兵玄奇";

    public const string BundleVersion = "1.0.1";
    public const int BundleVersionCode = 1;
#endif
    #endregion

#if Debug
    public const EVersion Version = EVersion.Debug;
#elif QA
    public const EVersion Version = EVersion.QA;
#else
    public const EVersion Version = EVersion.Release;
#endif

    #region BundleId
#if HK
    #if UNITY_ANDROID
    public const string Bundle_ID = "com.x980.lethalweapon";
    #elif UNITY_IOS
      #if ThirdParty
      public const string Bundle_ID = "com.980x.lethalweaponhk";
      #else
      public const string Bundle_ID = "com.980x.lethalweapon";
      #endif
    #endif
#else
    #if UNITY_ANDROID
    public const string Bundle_ID = "com.chinesegamer.gwtw";
    #elif UNITY_IOS
    public const string Bundle_ID = "com.chinesegamer.Artifact";
    #endif
#endif
    #endregion
   
    #region FTP
    public const string Project_FTP_Account = "Artifact";
    public const string Project_FTP_Password = "70541704";
#if Debug
    public const string Project_FTP_Address = "192.168.37.36";
    public const string Project_FTP_Port = "";
#elif QA
    public const string Project_FTP_Address = "192.168.41.88";
    public const string Project_FTP_Port = "";
#else
    #if HK
    public const string Project_FTP_Address = "gwhkftp.chinesegamer.net";
    public const string Project_FTP_Port = "";
    #else
    public const string Project_FTP_Address = "gwtwftp.chinesegamer.net";
    public const string Project_FTP_Port = "";
    #endif
#endif
    #endregion
    
    #region App Store URL
#if HK
    #if UNITY_ANDROID
        #if MyCard
    public const string Project_DownloadUrl = "https://";
        #else
    public const string Project_DownloadUrl = "https://play.google.com/store/apps/details?id=com.x980.lethalweapon";
        #endif
    #elif UNITY_IOS
        #if MyCard
    public const string Project_DownloadUrl = "https://";
        #else
    public const string Project_DownloadUrl = "https://itunes.apple.com/hk/app/id1184237991";
        #endif
    #endif
#else
    #if UNITY_ANDROID
    public const string Project_DownloadUrl = "https://play.google.com/store/apps/details?id=com.Chinesegamer.Artifact";
    #elif UNITY_IOS
    public const string Project_DownloadUrl = "https://itunes.apple.com/app/id954726783";
    #endif
#endif
    #endregion

    #region FB
#if HK
    public const string FacebookId = "1617806281847940";
    public const string FacebookName = "artifacthk";
#else
    public const string FacebookId = "755114554581301";
    public const string FacebookName = "artifact";
#endif
    #endregion
    
    public const string Project_MainScene = "Assets/01.Scenes/Main.unity";
    public const string Project_EmptyScene = "Assets/01.Scenes/Empty.unity";

    public static readonly byte[] Project_EnCrypt_Key = Encoding.UTF8.GetBytes("1234567870541704");
    public static readonly byte[] Project_EnCrypt_IV = Encoding.UTF8.GetBytes("7054170412345678");

    public const int FrameRate = 30;

    public const int UnityLayer_Default = 0;
    public const int UnityLayer_TransparentFX = 1;
    public const int UnityLayer_IgnoreRaycast = 2;
    public const int UnityLayer_Water = 4;
    public const int UnityLayer_UI = 5;
    public const int UnityLayer_Floor = 8;
    public const int UnityLayer_RenderTexture = 9;
    public const int UnityLayer_Reflection = 10;
    public const int UnityLayer_Role = 11;
    public const int UnityLayer_Obstacle = 12;
    public const int UnityLayer_UICamera = 13;
    public const int UnityLayer_DefaultNearHide = 18;
    public const int UnityLayer_DefaultFarHide = 19;
    public const int UnityLayer_FloorNearHide = 20;
    public const int UnityLayer_FloorFarHide = 21;
    public const int UnityLayer_ObstacleNearHide = 22;
    public const int UnityLayer_ObstacleFarHide = 23;
    public const int UnityLayer_ObstacleInvisible = 24;

    public const int Defalut_ScreenWidth = 1024;
}
