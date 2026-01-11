#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// UI component'lerini otomatik bağlar
/// </summary>
public class UIAutoLinker : Editor
{
    [MenuItem("AYBU/Auto Link UI Components")]
    public static void AutoLinkUI()
    {
        LinkMainMenuUI();
        LinkHUDController();
        LinkMapUI();

        Debug.Log("UI component'leri bağlandı!");
    }

    private static void LinkMainMenuUI()
    {
        MainMenuUI mainMenu = FindFirstObjectByType<MainMenuUI>();
        if (mainMenu == null) return;

        SerializedObject so = new SerializedObject(mainMenu);
        Transform t = mainMenu.transform;

        SetObjectField(so, "startButton", FindInChildren<Button>(t, "StartButton"));
        SetObjectField(so, "mapButton", FindInChildren<Button>(t, "MapButton"));
        SetObjectField(so, "resetButton", FindInChildren<Button>(t, "ResetButton"));
        SetObjectField(so, "titleText", FindInChildren<Text>(t, "TitleText"));
        SetObjectField(so, "subtitleText", FindInChildren<Text>(t, "SubtitleText"));
        SetObjectField(so, "scoreText", FindInChildren<Text>(t, "ScoreText"));
        SetObjectField(so, "collectionText", FindInChildren<Text>(t, "CollectionText"));

        LinkCampusData(so, "campusData");
        so.ApplyModifiedProperties();
        Debug.Log("MainMenuUI bağlandı");
    }

    private static void LinkHUDController()
    {
        HUDController hud = FindFirstObjectByType<HUDController>();
        if (hud == null) return;

        SerializedObject so = new SerializedObject(hud);
        Transform t = hud.transform;

        SetObjectField(so, "scoreText", FindInChildren<Text>(t, "HUDScoreText"));
        SetObjectField(so, "locationText", FindInChildren<Text>(t, "HUDLocationText"));
        SetObjectField(so, "collectionText", FindInChildren<Text>(t, "HUDCollectionText"));
        SetObjectField(so, "gpsStatusText", FindInChildren<Text>(t, "GPSStatusText"));
        SetObjectField(so, "notificationText", FindInChildren<Text>(t, "NotificationText"));
        SetObjectField(so, "mapButton", FindInChildren<Button>(t, "HUDMapButton"));
        SetObjectField(so, "menuButton", FindInChildren<Button>(t, "HUDMenuButton"));
        SetObjectField(so, "notificationPanel", FindInChildren(t, "NotificationPanel"));

        LinkCampusData(so, "campusData");
        so.ApplyModifiedProperties();
        Debug.Log("HUDController bağlandı");
    }

    private static void LinkMapUI()
    {
        MapUI mapUI = FindFirstObjectByType<MapUI>();
        if (mapUI == null) return;

        SerializedObject so = new SerializedObject(mapUI);
        Transform t = mapUI.transform;

        SetObjectField(so, "mapImage", FindInChildren<Image>(t, "MapImage"));
        SetObjectField(so, "mapContainer", FindInChildren<RectTransform>(t, "MapImage"));
        SetObjectField(so, "closeButton", FindInChildren<Button>(t, "CloseMapButton"));
        SetObjectField(so, "infoPanel", FindInChildren(t, "LocationInfoPanel"));
        SetObjectField(so, "locationNameText", FindInChildren<Text>(t, "LocationNameText"));
        SetObjectField(so, "locationDescText", FindInChildren<Text>(t, "LocationDescText"));
        SetObjectField(so, "locationStatusText", FindInChildren<Text>(t, "LocationStatusText"));

        // Marker prefab
        string[] guids = AssetDatabase.FindAssets("MapMarker t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            so.FindProperty("locationMarkerPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        LinkCampusData(so, "campusData");
        so.ApplyModifiedProperties();
        Debug.Log("MapUI bağlandı");
    }

    private static void LinkCampusData(SerializedObject so, string propertyName)
    {
        string[] guids = AssetDatabase.FindAssets("t:CampusLocationsData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<CampusLocationsData>(path);
        }
    }

    private static T FindInChildren<T>(Transform parent, string name) where T : Component
    {
        foreach (var comp in parent.GetComponentsInChildren<T>(true))
            if (comp.gameObject.name == name) return comp;
        return null;
    }

    private static GameObject FindInChildren(Transform parent, string name)
    {
        foreach (var child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child.gameObject;
        return null;
    }

    private static void SetObjectField(SerializedObject so, string fieldName, Object value)
    {
        var prop = so.FindProperty(fieldName);
        if (prop != null && value != null)
            prop.objectReferenceValue = value;
    }

    [MenuItem("AYBU/Link AR Components")]
    public static void LinkARComponents()
    {
        LocationVerifier verifier = FindFirstObjectByType<LocationVerifier>();
        if (verifier != null)
        {
            var imageManager = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
            if (imageManager != null)
            {
                SerializedObject so = new SerializedObject(verifier);
                so.FindProperty("imageManager").objectReferenceValue = imageManager;
                so.ApplyModifiedProperties();
                Debug.Log("ARTrackedImageManager bağlandı!");
            }
        }

        UIManager uiMgr = FindFirstObjectByType<UIManager>();
        if (uiMgr != null)
        {
            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                SerializedObject so = new SerializedObject(uiMgr);
                so.FindProperty("arSessionOrigin").objectReferenceValue = xrOrigin.gameObject;
                so.ApplyModifiedProperties();
                Debug.Log("XR Origin bağlandı!");
            }
        }
    }

    [MenuItem("AYBU/Assign Avatar Prefabs to Locations")]
    public static void AssignAvatarPrefabs()
    {
        string[] dataGuids = AssetDatabase.FindAssets("t:CampusLocationsData");
        if (dataGuids.Length == 0) return;

        CampusLocationsData data = AssetDatabase.LoadAssetAtPath<CampusLocationsData>(
            AssetDatabase.GUIDToAssetPath(dataGuids[0]));

        string[] avatarGuids = AssetDatabase.FindAssets("CollectableAvatar t:Prefab");
        if (avatarGuids.Length == 0) return;

        GameObject avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(avatarGuids[0]));

        foreach (var loc in data.locations)
            loc.avatarPrefab = avatarPrefab;

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        Debug.Log($"Avatar prefab {data.locations.Count} lokasyona atandı!");
    }
}
#endif
