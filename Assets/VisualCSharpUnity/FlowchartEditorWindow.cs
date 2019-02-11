using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FlowchartEditorWindow : EditorWindow
{
    //Create a New instance or show current
    //public SymbolesManager symbolesManager;
    public SymboleManager symboleManager;
    public EventManager eventManager;
    private int ToolbarItem = -1;
    public Rect editorSize { get {return new Rect(Vector2.zero, position.size); } }
    public Texture2D _gridTex;
    private static Vector2 panOffset = Vector2.zero;
    public static float zoomDelta = 0.01f;
    public static float minZoom = 1f;
    public static float maxZoom = 4f;
    public static float panSpeed = 1.2f;
    private Vector2 _zoomAdjustment;
    private static Vector2 _zoom = Vector2.one;
    public float zScale { get { return ZoomScale;} }
    public Vector2 pOffset { get { return panOffset; } }
    [MenuItem("VisualCSUnity/FlowChart1")]
    public static void Init()
    {
        FlowchartEditorWindow few = GetWindow<FlowchartEditorWindow>();
        few.titleContent = new GUIContent("VisualCS Unity");
        few.Show();
    }

    public static void Init(VCSUGraph graph)
    {
        FlowchartEditorWindow few = GetWindow<FlowchartEditorWindow>();
        few.titleContent = new GUIContent("VisualCS Unity");
        few.symboleManager = new SymboleManager(few);
        few.symboleManager.graph = graph;
        few.eventManager = new EventManager(few.symboleManager, few, OnPan);
        few.Show();
    }

    private void OnEnable()
    {
        GUIScaleUtility.CheckInit();
        _gridTex = LoadResources.GetTexture("Grid");
        if (EditorPrefs.HasKey("VCSU Graph Last"))
        {
            symboleManager = new SymboleManager(this);
            symboleManager.graph = AssetDatabase.LoadAssetAtPath<VCSUGraph>(EditorPrefs.GetString("VCSU Graph Last"));
            eventManager = new EventManager(symboleManager, this, OnPan);
            EditorPrefs.DeleteKey("VCSU Graph Last");
        }
    }
    private void OnDisable()
    {
        if (symboleManager != null && symboleManager.graph)
            EditorPrefs.SetString("VCSU Graph Last", (AssetDatabase.GetAssetPath(symboleManager.graph)));
    }
    // Start is called before the first frame update
    private void OnGUI()
    {
        ZoomEvent();
        DrawEditor();
        Rect graphRect = editorSize;
        var center = graphRect.center;
        _zoomAdjustment = GUIScaleUtility.BeginScale(ref graphRect, center, ZoomScale, true, false);
        if (symboleManager != null)
            symboleManager.Draw();
        GUIScaleUtility.EndScale();
        GUILayout.BeginHorizontal("Toolbar");
        if (GUILayout.Button("New", "ToolbarButton"))
        {
            symboleManager = new SymboleManager(this);
            var graph = symboleManager.graph = CreateInstance<VCSUGraph>();
            if (!System.IO.Directory.Exists("Assets/VisualCSharpUnity/Graphs/Temp"))
                System.IO.Directory.CreateDirectory("Assets/VisualCSharpUnity/Graphs/Temp");
            AssetDatabase.CreateAsset(symboleManager.graph, "Assets/VisualCSharpUnity/Graphs/Temp/test.asset");
            if(graph.symboles != null)
            foreach (var symbole in graph.symboles)
            {
                AssetDatabase.AddObjectToAsset(symbole, "Assets/VisualCSharpUnity/Graphs/Temp/test.asset");
            }
            if (graph.connectionPoints != null)
                foreach (var connectionPoint in graph.connectionPoints)
            {
                AssetDatabase.AddObjectToAsset(connectionPoint, "Assets/VisualCSharpUnity/Graphs/Temp/test.asset");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ProjectWindowUtil.ShowCreatedAsset(graph);
            eventManager = new EventManager(symboleManager, this, OnPan);
        }
        if (GUILayout.Button("Save", "ToolbarButton"))
            symboleManager.Save();
        if (GUILayout.Button("Load", "ToolbarButton"))
            Load();
        if (GUILayout.Button("Close", "ToolbarButton"))
            CloseGraph();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (symboleManager != null)
        {
            if (GUI.Button(new Rect(editorSize.width - 99, editorSize.height - 25, 96, 18), "Remove Nodes"))
                symboleManager.OnClickRemoveAllNode();
            if (symboleManager.RemovedCP != null && symboleManager.RemovedCP.Count > 0 && symboleManager.RemovedS != null && symboleManager.RemovedS.Count > 0)
                if (GUI.Button(new Rect(editorSize.width - 291, editorSize.height - 25, 182, 18), "Restore Last Removed Nodes"))
                    symboleManager.OnClickRestoreAllNodes();
            if (GUI.Button(new Rect(editorSize.width - 99, editorSize.height - 60, 96, 18), "Reset"))
            {
                panOffset = Vector2.zero;
                _zoom = Vector2.one;
            }
        }
        if (eventManager != null)
            eventManager.ProcessEvent(Event.current);
        if (GUI.changed) Repaint();
    }

    public void CloseGraph()
    {
        symboleManager.RemoveComplete();
        symboleManager = null;
        eventManager = null;
    }

    public void Load()
    {
        symboleManager = new SymboleManager(this);
        eventManager = new EventManager(symboleManager, this, OnPan);
        var asset = AssetDatabase.LoadAssetAtPath<VCSUGraph>("Assets/test.asset");
        symboleManager.graph = asset;
    }

    public void DrawEditor()
    {
        var size = editorSize.size;
        var center = editorSize.center;
        float zoom = ZoomScale;
        // Offset from origin in tile units
        float xOffset = -(center.x * zoom + panOffset.x) / _gridTex.width;
        float yOffset = ((center.y - size.y) * zoom + panOffset.y) / _gridTex.height;

        Vector2 tileOffset = new Vector2(xOffset, yOffset);

        // Amount of tiles
        float tileAmountX = Mathf.Round(size.x * zoom) / _gridTex.width;
        float tileAmountY = Mathf.Round(size.y * zoom) / _gridTex.height;

        Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

        // Draw tiled background
        GUI.DrawTextureWithTexCoords(editorSize, _gridTex, new Rect(tileOffset, tileAmount));
    }

    public static void OnPan(Vector2 delta)
    {
        panOffset += delta * ZoomScale * panSpeed;
    }

    public void ZoomEvent()
    {
        var e = Event.current;
        if (e.type == EventType.ScrollWheel)
        {
            Zoom(e.delta.y, e.mousePosition);
            GUI.changed = true;
        }
    }

    public void Zoom(float zoomDirection, Vector2 mousePosition)
    {
        float scale = (zoomDirection < 0f) ? (1f - zoomDelta) : (1f + zoomDelta);
        if (ZoomScale < 4 && ZoomScale > 1)
        {
            var des = Vector2.MoveTowards(editorSize.center, mousePosition, 10f) - editorSize.center;
            if (zoomDirection < 0f)
                panOffset -= des;
            if (zoomDirection > 0f)
                panOffset += des;
        }
        _zoom *= scale;

        float cap = Mathf.Clamp(_zoom.x, minZoom, maxZoom);
        _zoom.Set(cap, cap);
    }

    private static float ZoomScale
    {
        get { return _zoom.x; }
        set
        {
            float z = Mathf.Clamp(value, minZoom, maxZoom);
            _zoom.Set(z, z);
        }
    }

    public Vector2 GraphToScreenSpace(Vector2 graphPos)
    {
        return graphPos + panOffset;
    }

    public Vector2 InvGraphToScreenSpace(Vector2 graphPos)
    {
        return graphPos * ZoomScale - panOffset;
    }

    public Vector2 ScreenToGraphSpace(Vector2 screenPos)
    {
        var graphRect = editorSize;
        var center = graphRect.center;
        return (screenPos - center) * ZoomScale - panOffset;
    }
}
