/* Introduction:設定快取伺服器IP
 * Author:江宗寰
 * Update Date:2016/09/13
 * */

using UnityEditor;

[InitializeOnLoad]
public class SetCacheServerIP
{
    private const string CacheServerIP = "192.168.37.36";

    static SetCacheServerIP()
    {
        SetIP();
        EditorApplication.projectWindowChanged += OnProjectWindowChanged;
    }

    public static void OnProjectWindowChanged()
    {
        SetIP();
    }

    private static void SetIP()
    {
        EditorPrefs.SetBool("CacheServerEnabled", true);
        EditorPrefs.SetString("CacheServerIPAddress", CacheServerIP);
        AssetDatabase.Refresh();
    }
}