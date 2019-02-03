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
    [MenuItem("VisualCSUnity/FlowChart1")]
    public static void Init()
    {
        FlowchartEditorWindow few = GetWindow<FlowchartEditorWindow>();
        few.titleContent = new GUIContent("VisualCS Unity");
        if (few.symboleManager == null)
            few.symboleManager = new SymboleManager();
        if (few.eventManager == null)
            few.eventManager = new EventManager (few.symboleManager, few, OnPan);
        few.Show();
    }
    private void OnEnable()
    {
        _gridTex = LoadResources.GetTexture("Grid");
    }
    // Start is called before the first frame update
    private void OnGUI()
    {
        DrawEditor();
        symboleManager.Draw();
        GUILayout.BeginHorizontal("Toolbar");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(GUI.Button(new Rect(editorSize.width - 99,editorSize.height - 25,96,18), "Remove Nodes"))
            symboleManager.OnClickRemoveAllNode();
        if (GUI.Button(new Rect(editorSize.width - 99, editorSize.height - 60, 96, 18), "Reset"))
        {
            symboleManager.PanSymbole(-panOffset);
            panOffset = Vector2.zero;
        }
        eventManager.ProcessEvent(Event.current);
        if (GUI.changed) Repaint();
    }

    public void DrawEditor()
    {
            var size = editorSize.size;
            var center = editorSize.center;

            // Offset from origin in tile units
            float xOffset = -(center.x + panOffset.x) / _gridTex.width;
            float yOffset = ((center.y - size.y) + panOffset.y) / _gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(size.x) / _gridTex.width;
            float tileAmountY = Mathf.Round(size.y) / _gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(editorSize, _gridTex, new Rect(tileOffset, tileAmount));
    }

    public static void OnPan(Vector2 delta)
    {
        panOffset += delta;
    }
}
