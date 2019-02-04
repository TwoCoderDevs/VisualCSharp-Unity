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
        if (few.symboleManager == null)
            few.symboleManager = new SymboleManager(few);
        if (few.eventManager == null)
            few.eventManager = new EventManager (few.symboleManager, few, OnPan);
        few.Show();
    }
    private void OnEnable()
    {
        GUIScaleUtility.CheckInit();
        _gridTex = LoadResources.GetTexture("Grid");
    }
    // Start is called before the first frame update
    private void OnGUI()
    {
        ZoomEvent();
        DrawEditor();
        Rect graphRect = editorSize;
        var center = graphRect.center;
        _zoomAdjustment = GUIScaleUtility.BeginScale(ref graphRect, center, ZoomScale, true, false);
        symboleManager.Draw();
        GUIScaleUtility.EndScale();
        GUILayout.BeginHorizontal("Toolbar");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(GUI.Button(new Rect(editorSize.width - 99,editorSize.height - 25,96,18), "Remove Nodes"))
            symboleManager.OnClickRemoveAllNode();
        if (GUI.Button(new Rect(editorSize.width - 99, editorSize.height - 60, 96, 18), "Reset"))
        {
            panOffset = Vector2.zero;
            _zoom = Vector2.one;
        }
        eventManager.ProcessEvent(Event.current);
        if (GUI.changed) Repaint();
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
            Zoom(e.delta.y);
            panOffset = Vector2.MoveTowards(panOffset, e.mousePosition, panSpeed);
            GUI.changed = true;
        }
    }

    public void Zoom(float zoomDirection)
    {
        float scale = (zoomDirection < 0f) ? (1f - zoomDelta) : (1f + zoomDelta);
        Debug.Log(scale);
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
