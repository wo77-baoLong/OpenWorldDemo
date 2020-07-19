using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class TerrainGenerateWindow : EditorWindow
{
    private Vector2 scrollView;

    public string tempHeightPath;
    
    /*这里应该支持数组生成，时间比较紧先不管了*/
    private TerrainParameter terrainParameter = new TerrainParameter();

    private bool diffuseMapFoldout;
    private bool normalMapFoldout;

    [MenuItem("Terrain/地形白模生成")]
    public static void GenerateTerrain()
    {
        EditorWindow.GetWindow<TerrainGenerateWindow>("GenerateTerrain");
        TerrainGenerateWindow window = EditorWindow.GetWindow<TerrainGenerateWindow>();
        window.titleContent = new GUIContent("Terrain", "GenerateTerrain");
        window.minSize = new Vector2(320f, 290f);
        window.Show();
    }

    public void OnEnable()
    {
        Debug.Log("asad");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        scrollView = EditorGUILayout.BeginScrollView(scrollView, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("生成地形白模", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        DrawSavePathAndName(); 

        EditorGUILayout.Space();
        DrawSourceMaps();

        EditorGUILayout.Space();
        DrawMapParameter();

        EditorGUILayout.Space();
        DrawGenerateButton();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    void DrawSavePathAndName()
    {
        DrawSubTitle("Save");
        EditorGUILayout.BeginVertical(GUI.skin.box);
            terrainParameter.saveFolder = EditorGUILayout.TextField("保存文件夹", terrainParameter.saveFolder, new GUILayoutOption[0]);
            GUI.enabled = false;
            GUIStyle style = GUI.skin.label;
            style.fontStyle = FontStyle.Italic;
            GUILayout.Label($"Assets/{(string.IsNullOrEmpty(terrainParameter.saveFolder) ? string.Empty : terrainParameter.saveFolder)}", style);
            GUI.enabled = true;
            terrainParameter.saveName = EditorGUILayout.TextField("地形命名", terrainParameter.saveName, new GUILayoutOption[0]);
        EditorGUILayout.EndVertical();
    }
    void DrawSubTitle(string titleName)
    {
        //绘制底toolbar
        EditorGUILayout.LabelField(string.Empty, EditorStyles.toolbar, new GUILayoutOption[0]);
        //GUILayoutUtility.GetLastRect().yMin = ？
        GUI.Label(new Rect(6f, GUILayoutUtility.GetLastRect().yMin + 1f, 100f, 16f), titleName, new GUIStyle());
    }

    void DrawSourceMaps()
    {
        DrawSubTitle("导入资源图");
        EditorGUILayout.BeginVertical(GUI.skin.box);
            terrainParameter.heightMap = DrawMap<UnityEngine.Object>(terrainParameter.heightMap, "导入高度图");
            terrainParameter.splatMap = (Texture2D)DrawMap<Texture2D>(terrainParameter.splatMap, "导入分层图");
            terrainParameter.diffuseMap = (Texture2D)DrawMap<Texture2D>(terrainParameter.diffuseMap, "导入diffuse图");
            terrainParameter.normalMap = (Texture2D)DrawMap<Texture2D>(terrainParameter.normalMap, "导入法线图");
            GUI.enabled = false;
            GUILayout.Label("diffuseMap与normalMap会使用自定义着色器");
            GUI.enabled = true;

            EditorGUILayout.LabelField("四张细节图及分辨率");
            using (new EditorGUIIndentLevel(1))
            {
                terrainParameter.splatPrototyesSize = EditorGUILayout.Vector2Field("", terrainParameter.splatPrototyesSize);
                terrainParameter.splatPrototypes[0] = (Texture2D)DrawMap<Texture2D>(terrainParameter.splatPrototypes[0], "R");
                terrainParameter.splatPrototypes[1] = (Texture2D)DrawMap<Texture2D>(terrainParameter.splatPrototypes[1], "G");
                terrainParameter.splatPrototypes[2] = (Texture2D)DrawMap<Texture2D>(terrainParameter.splatPrototypes[2], "B");
                terrainParameter.splatPrototypes[3] = (Texture2D)DrawMap<Texture2D>(terrainParameter.splatPrototypes[3], "A");
            }

        EditorGUILayout.EndVertical();
    }

    UnityEngine.Object DrawMap<T>(UnityEngine.Object source, string nameMap)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(nameMap);
        GUILayout.FlexibleSpace();
        source = EditorGUILayout.ObjectField(source, typeof(T), true);
        GUILayout.EndHorizontal();
        return source;
    }

    void DrawMapParameter()
    {
        DrawSubTitle("参数设定");
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.LabelField("Terrain Resolution");
        terrainParameter.TerrainSize = DrawParameter("Terrain Size", terrainParameter.TerrainSize);
        terrainParameter.DetailResolutionPerPath = DrawParameter("Detail Per Path", terrainParameter.DetailResolutionPerPath);
        terrainParameter.DetailResolution = DrawParameter("Detail Resolution", terrainParameter.DetailResolution);

        EditorGUILayout.LabelField("Texture Resolution");
        terrainParameter.BaseTextureResolution = DrawParameter("Diffuse Resolution", terrainParameter.BaseTextureResolution);
        terrainParameter.ControlTextureResolution = DrawParameter("Splat Resolution", terrainParameter.ControlTextureResolution);
        terrainParameter.HeightMapResolution = DrawParameter("Height Map Resolution", terrainParameter.HeightMapResolution);

        EditorGUILayout.LabelField("Tree&Detail object");
        terrainParameter.DetailVisibleDistance = DrawParameter("Detail Visible Dist", terrainParameter.DetailVisibleDistance);
        terrainParameter.DetailDensity = DrawParameter("Detail Density", terrainParameter.DetailDensity);
        terrainParameter.TreeVisibleDistance = DrawParameter("Tree Visible Dist", terrainParameter.TreeVisibleDistance);
        terrainParameter.BillboardStart = DrawParameter("Billboard Start", terrainParameter.BillboardStart);
        terrainParameter.FadeLength = DrawParameter("Fade Length", terrainParameter.FadeLength);
        terrainParameter.MaxMeshTrees = DrawParameter("MaxMesh Trees", terrainParameter.MaxMeshTrees);

        EditorGUILayout.LabelField("Basic Terrain");
        terrainParameter.Material = DrawParameter("Terrain Material", terrainParameter.Material);

        EditorGUILayout.EndVertical();
    }

    int DrawParameter(string paramName, int value)
    {
        EditorGUILayout.BeginHorizontal();
        using (new EditorGUIIndentLevel(1))
        {
            value = EditorGUILayout.IntField(paramName, value);
        }
        EditorGUILayout.EndHorizontal();
        return value;
    }
    Vector3 DrawParameter(string paramName, Vector3 value)
    {
        EditorGUILayout.BeginHorizontal();
        using (new EditorGUIIndentLevel(1))
        {
            value = EditorGUILayout.Vector3Field(paramName, value);
        }
        EditorGUILayout.EndHorizontal();
        return value;
    }
    float DrawParameter(string paramName, float value)
    {
        EditorGUILayout.BeginHorizontal();
        using (new EditorGUIIndentLevel(1))
        {
            value = EditorGUILayout.FloatField(paramName, value);
        }
        EditorGUILayout.EndHorizontal();
        return value;
    }
    Material DrawParameter(string paramName, Material value)
    {
        EditorGUILayout.BeginHorizontal();
        using (new EditorGUIIndentLevel(1))
        {
            value = (Material)EditorGUILayout.ObjectField(paramName, value, typeof(Material), new GUILayoutOption[0]);
        }
        EditorGUILayout.EndHorizontal();
        return value;
    }

    void DrawGenerateButton()
    {
        GUILayout.Space(5);
        GUI.enabled = terrainParameter.IsAllFill();
        if (GUI.Button(new Rect(6f, this.position.height - 47f, this.position.width - 24f, 40f), "Generate"))
        {
            this.Close();
            EditorUtility.DisplayProgressBar("生成白模", "计算高度图", 0.1f);
            TerrainGenerator.InitTerrain(terrainParameter);
            TerrainGenerator.GenerateHeight(terrainParameter);
            EditorUtility.DisplayProgressBar("生成白模", "计算分层", 0.4f);
            TerrainGenerator.GenerateSplat(terrainParameter);
            EditorUtility.DisplayProgressBar("生成白模", "计算完毕", 0.7f);
            TerrainGenerator.ShowTerrain(terrainParameter);
            EditorUtility.DisplayProgressBar("生成白模", "显示", 0.99f);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }
    }
}

public class TerrainGenerator
{
    public static void InitTerrain(TerrainParameter param)
    {
        TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(param.GetPrefabSavePath());
        if (!data)
        {
            data = new TerrainData();
            AssetDatabase.CreateAsset(data,param.GetPrefabSavePath());
        }
        data.size = param.TerrainSize;
        data.SetDetailResolution(param.DetailResolution, param.DetailResolutionPerPath);
        data.baseMapResolution = param.BaseTextureResolution;
        data.alphamapResolution = param.ControlTextureResolution;
        param.TerrainData = data;
    }

    public static void GenerateHeight(TerrainParameter param)
    {
        byte[] buffer = File.ReadAllBytes(param.heightMapPath);
        float bitCount = 0f;
        if(param.heightMapPath.EndsWith(".r16"))
        {
            bitCount = 256.0f;
        }
        else
        {
            bitCount = 128.0f;
        }
        float doubelByte = bitCount * bitCount;
        int hflength = buffer.Length / 2;
        int hfWidth , hfHeight;
        hfWidth = hfHeight = Mathf.CeilToInt(Mathf.Sqrt(hflength));

        float[,] heights = new float[hfHeight, hfWidth];
        int i = 0;
        /* O-----x
         * |
         * |
         * y
         * 从上到下，从左至右
         */
        for (int x = 0; x < hfWidth; x++)
        {
            for (int y = 0; y < hfWidth; y++)
            {
                /*
                 * heightmap导出时是按行存储，左上角为(0,0)需要翻转y轴
                 * 大小端
                 * (x * bitCount) / doubelByte：在两个高度间做一次平滑
                 */
#if UNITY_EDITOR_WIN
                //小端
                heights[hfWidth - 1 - x, y] = (buffer[i++] + buffer[i++] * bitCount) / doubelByte;
#else
                //大端
                heights[hfWidth - 1 - x, y] = (buffer[i++] * bitCount + buffer[i++]) / doubelByte;
#endif
            }
        }

        param.TerrainData.heightmapResolution = hfWidth + 1;
        param.TerrainData.SetHeights(0, 0, heights);
        param.TerrainData.size = param.TerrainSize;
    }

    public static void SetSplatPrototype(TerrainParameter param)
    {
        SplatPrototype[] newSplatPrototypes = new SplatPrototype[param.splatPrototypes.Length];
        for (int i = 0; i < param.splatPrototypes.Length; i++)
        {
            newSplatPrototypes[i] = new SplatPrototype();
            newSplatPrototypes[i].texture = param.splatPrototypes[i];
            newSplatPrototypes[i].tileSize = param.splatPrototyesSize;
            newSplatPrototypes[i].texture.Apply(true);
        }
        param.TerrainData.splatPrototypes = newSplatPrototypes;
        param.TerrainData.RefreshPrototypes();
    }
    public static void GenerateSplat(TerrainParameter param)
    {
        SetSplatPrototype(param);

        int wh = param.splatMap.width;
        param.TerrainData.alphamapResolution = param.ControlTextureResolution;
        if (wh != param.ControlTextureResolution)
        {
            wh = param.ControlTextureResolution;
        }

        float[,,] splat = param.TerrainData.GetAlphamaps(0, 0, wh, wh);
        Color[] pixels = param.splatMap.GetPixels();
        for (int y = 0; y < wh; y++)
        {
            for (int x = 0; x < wh; x++)
            {
                Color pixel = pixels[x + y];
                float sum = pixel.r + pixel.g + pixel.b;
                if (sum > 1.0f)
                {
                    splat[x, y, 0] = pixel.r / sum;
                    splat[x, y, 1] = pixel.g / sum;
                    splat[x, y, 2] = pixel.b / sum;
                    splat[x, y, 3] = 0.0f;
                }
                else
                {
                    splat[x, y, 0] = pixel.r;
                    splat[x, y, 1] = pixel.g;
                    splat[x, y, 2] = pixel.b;
                    splat[x, y, 3] = pixel.a;
                }
            }
        }
        param.TerrainData.SetAlphamaps(0, 0, splat);
    }

    public static void ShowTerrain(TerrainParameter param)
    {
        GameObject terrainObj = new GameObject(param.saveName);
        Terrain terrain = terrainObj.AddComponent<Terrain>();
        TerrainCollider collider = terrainObj.AddComponent<TerrainCollider>();

        collider.terrainData = param.TerrainData;
        terrain.terrainData = param.TerrainData;
        terrain.drawHeightmap = param.BasicDraw;
        terrain.heightmapPixelError = 1;

        terrain.treeDistance = param.TreeVisibleDistance;
        terrain.treeCrossFadeLength = param.FadeLength;
        terrain.treeBillboardDistance = param.BillboardStart;
        terrain.bakeLightProbesForTrees = param.BakeLightProbesForTrees;
        terrain.preserveTreePrototypeLayers = param.preserveTreePrototypeLayers;
        terrain.drawTreesAndFoliage = param.TDDraw;
        if (param.Material)
        {
            terrain.materialType = Terrain.MaterialType.Custom;
            terrain.materialTemplate = param.Material;
        }
        else
        {
            terrain.materialType = Terrain.MaterialType.BuiltInStandard;
        }
        
        terrain.detailObjectDensity = param.DetailDensity;
        terrain.detailObjectDistance = param.DetailVisibleDistance;
        terrain.castShadows = param.CastShadows;
    }
}

public class TerrainParameter
{
    public TerrainData TerrainData;

    public string saveFolder;
    public string saveName;

    //private Texture2D[] maps;
    public UnityEngine.Object heightMap;
    public string heightMapPath
    {
        get { return AssetDatabase.GetAssetPath(heightMap); }
    }

    public Texture2D splatMap;
    public Texture2D diffuseMap;
    public Texture2D normalMap;

    public Texture2D[] splatPrototypes = new Texture2D[4];
    public Vector2 splatPrototyesSize = new Vector2(128,128);

    //Mesh Resolution
    public Vector3 TerrainSize;

    public int DetailResolutionPerPath = 16;
    public int DetailResolution = 1024;

    //Texture Resolution
    public int BaseTextureResolution;
    public int ControlTextureResolution = 512;
    public int HeightMapResolution;

    //tree & Detail object
    public bool TDDraw = true;
    public bool BakeLightProbesForTrees = true;
    public bool preserveTreePrototypeLayers = true;
    public float DetailVisibleDistance = 128;
    public float DetailDensity = 1;
    public float TreeVisibleDistance = 256;
    public float BillboardStart = 128;
    public float FadeLength = 10;
    public float MaxMeshTrees = 20;

    //basic
    public bool BasicDraw = true;
    public float PixelError = 1;
    public float BaseMapDist = 2000;
    public bool CastShadows = false;
    public Material Material;

    public string GetPrefabSaveFolder()
    {
        return $"{Application.dataPath}/{saveFolder}";
    }
    public string GetPrefabSavePath()
    {
        string folder = GetPrefabSaveFolder();
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return $"Assets/{saveFolder}/{saveName}.asset";
    }

    public bool IsAllFill()
    {
        return !string.IsNullOrEmpty(saveFolder)
               && !string.IsNullOrEmpty(saveName)
               && heightMap != null && splatMap != null 
               && diffuseMap != null && normalMap != null
               && PrototypeNotNull();
    }

    bool PrototypeNotNull()
    {
        bool flag = true;
        for (int i = 0; i < splatPrototypes.Length; i++)
        {
            flag = splatPrototypes[i] != null && flag;
        }

        return flag;
    }
}

public class EditorGUIIndentLevel : IDisposable
{
    [SerializeField]
    private int PreviousIndent { get; set; }

    public EditorGUIIndentLevel(int newIndent)
    {
        this.PreviousIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel += newIndent;
    }

    public void Dispose()
    {
        EditorGUI.indentLevel = this.PreviousIndent;
    }
}