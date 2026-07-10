using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameplayPrefabSetup
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PlayerPrefabPath = PrefabFolder + "/SimpleFirstPersonPlayer.prefab";
    private const string BallPrefabPath = PrefabFolder + "/PushableBall.prefab";
    private const string GiftBoxPrefabPath = PrefabFolder + "/GiftBox.prefab";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Setup Simple First Person Giftbox Demo")]
    public static void Setup()
    {
        Directory.CreateDirectory(PrefabFolder);

        GameObject ballPrefab = CreateBallPrefab();
        GameObject giftBoxPrefab = CreateGiftBoxPrefab(ballPrefab);
        GameObject playerPrefab = CreatePlayerPrefab();

        Scene scene = EditorSceneManager.OpenScene(ScenePath);
        RemoveSceneObject("Main Camera");
        RemoveSceneObject("SimpleFirstPersonPlayer");
        RemoveSceneObject("GiftBox");

        EnsureGround();
        EnsureLight();

        PrefabUtility.InstantiatePrefab(playerPrefab);
        GameObject player = GameObject.Find("SimpleFirstPersonPlayer");
        player.transform.SetPositionAndRotation(new Vector3(0f, 1.1f, -5f), Quaternion.identity);

        PrefabUtility.InstantiatePrefab(giftBoxPrefab);
        GameObject giftBox = GameObject.Find("GiftBox");
        giftBox.transform.SetPositionAndRotation(new Vector3(0f, 0.5f, 2f), Quaternion.identity);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static GameObject CreatePlayerPrefab()
    {
        GameObject player = new GameObject("SimpleFirstPersonPlayer");
        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 1.8f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 0.9f, 0f);
        player.AddComponent<SimpleFirstPersonController>();
        player.AddComponent<PlayerRigidbodyPusher>();

        GameObject camera = new GameObject("Player Camera");
        camera.transform.SetParent(player.transform);
        camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        camera.transform.localRotation = Quaternion.identity;
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();

        GameObject prefab = SavePrefab(player, PlayerPrefabPath);
        Object.DestroyImmediate(player);
        return prefab;
    }

    private static GameObject CreateGiftBoxPrefab(GameObject ballPrefab)
    {
        GameObject giftBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        giftBox.name = "GiftBox";
        giftBox.transform.localScale = Vector3.one;
        GiftBox giftBoxScript = giftBox.AddComponent<GiftBox>();
        SetPrivateObject(giftBoxScript, "ballPrefab", ballPrefab);

        Renderer renderer = giftBox.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial("GiftBox_Red", new Color(0.85f, 0.08f, 0.12f));

        GameObject ribbonVertical = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ribbonVertical.name = "Ribbon_Vertical";
        ribbonVertical.transform.SetParent(giftBox.transform);
        ribbonVertical.transform.localPosition = Vector3.zero;
        ribbonVertical.transform.localScale = new Vector3(0.18f, 1.03f, 1.05f);
        ribbonVertical.GetComponent<Renderer>().sharedMaterial = CreateMaterial("GiftBox_Gold", new Color(1f, 0.78f, 0.12f));

        GameObject ribbonHorizontal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ribbonHorizontal.name = "Ribbon_Horizontal";
        ribbonHorizontal.transform.SetParent(giftBox.transform);
        ribbonHorizontal.transform.localPosition = Vector3.zero;
        ribbonHorizontal.transform.localScale = new Vector3(1.05f, 1.04f, 0.18f);
        ribbonHorizontal.GetComponent<Renderer>().sharedMaterial = ribbonVertical.GetComponent<Renderer>().sharedMaterial;

        GameObject prefab = SavePrefab(giftBox, GiftBoxPrefabPath);
        Object.DestroyImmediate(giftBox);
        return prefab;
    }

    private static GameObject CreateBallPrefab()
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "PushableBall";
        ball.transform.localScale = Vector3.one;
        Rigidbody rigidbody = ball.AddComponent<Rigidbody>();
        rigidbody.mass = 1.2f;
        rigidbody.linearDamping = 0.15f;
        rigidbody.angularDamping = 0.05f;
        ball.GetComponent<Renderer>().sharedMaterial = CreateMaterial("PushableBall_Blue", new Color(0.1f, 0.45f, 0.95f));

        GameObject prefab = SavePrefab(ball, BallPrefabPath);
        Object.DestroyImmediate(ball);
        return prefab;
    }

    private static void EnsureGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }

        ground.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
    }

    private static void EnsureLight()
    {
        if (Object.FindFirstObjectByType<Light>() != null)
        {
            return;
        }

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void RemoveSceneObject(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
    }

    private static GameObject SavePrefab(GameObject source, string path)
    {
        return PrefabUtility.SaveAsPrefabAsset(source, path);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        string folder = "Assets/Materials";
        Directory.CreateDirectory(folder);
        string path = folder + "/" + materialName + ".mat";

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetPrivateObject(Object target, string fieldName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
