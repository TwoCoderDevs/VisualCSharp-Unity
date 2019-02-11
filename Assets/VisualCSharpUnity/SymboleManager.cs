﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.ObjectSerializer;
[Serializable]
public class SymboleManager
{
    public List<Symbole> symboles { get{ if (graph) return graph.symboles; return null; } set{ graph.symboles = value; } }
    private GUIStyle background = LoadResources.GetNormalStyle("xnode_node");
    private GUIStyle highlighted = LoadResources.GetHiglightedStyle("xnode_node_highlight");
    public Symbole selectedSymbole;
    public ConnectionPoint selectedInputPoint;
    public ConnectionPoint selectedOutputPoint;
    public List<ConnectionPoint> points { get { if (graph) return graph.connectionPoints; return null; } set{ graph.connectionPoints = value; } }
    private GUIStyle textStyle;
    private FlowchartEditorWindow few;
    public VCSUGraph graph;
    private static Action<ConnectionPoint> actionAddPoint;
    public List<Symbole> RemovedS;
    public List<ConnectionPoint> RemovedCP;
    public SymboleManager(FlowchartEditorWindow few)
    {
        this.few = few;
        background.alignment = TextAnchor.UpperCenter;
        background.fontStyle = FontStyle.Bold;
        background.normal.textColor = Color.white;
        actionAddPoint = AddPoint;
    }

    public void AddPoint(ConnectionPoint connectionPoint)
    {
        if (points == null)
            points = new List<ConnectionPoint>();
        points.Add(connectionPoint);
    }

    public static void AddSymboleStatic(ConnectionPoint connectionPoint)
    {
        actionAddPoint(connectionPoint);
    }

    public void Save()
    {
        AssetDatabase.CreateAsset(graph, "Assets/test.asset");
        foreach (var symbole in graph.symboles)
        {
            //AssetDatabase.AddObjectToAsset(symbole, "Assets/test.asset");
        }
        foreach (var connectionPoint in graph.connectionPoints)
        {
            //AssetDatabase.AddObjectToAsset(connectionPoint, "Assets/test.asset");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ProjectWindowUtil.ShowCreatedAsset(graph);
    }

    public void Draw()
    {
        if (points != null)
            foreach (var point in points)
                if (point.connectionType == ConnectionType.Output)
                    foreach (var input in point.Connections)
                        if (input.connectionType == ConnectionType.Input)
                        {
                            //var col = Color.Lerp(point.knobeColor, input.knobeColor, 0.5f);
                            DrawConnectionBezier(few.GraphToScreenSpace(point.PointPos.center), few.GraphToScreenSpace(input.PointPos.center), point.knobeColor, input.knobeColor );
                        }
        if (symboles != null)
        {
            DrawSymboles();
            GUI.changed = true;
        }
        if (selectedOutputPoint)
        {
            DrawBezierPreview(selectedOutputPoint.PointPos.center);
            GUI.changed = true;
        }
    }

    public void DrawSymboles()
    {
        for (int i = 0; i < symboles.Count; i++)
        {
            var area = symboles[i].NodeSize;
            area.position = few.GraphToScreenSpace(area.position);
            GUI.Box(area, symboles[i].name.AddWordSpace(), background);
            GUILayout.BeginArea(new Rect(area.position.x - 8, area.position.y + 35, area.width + 16, area.height - 35));
            var r = EditorGUILayout.BeginVertical();
            var lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            foreach (var point in symboles[i].fieldPoints)
            {
                point.Draw();
            }
            symboles[i].OnGUI();
            EditorGUIUtility.labelWidth = lw;
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            if (r.height > 0)
                symboles[i].NodeSize.height = area.height = 43 + r.height;
            if (symboles[i] == selectedSymbole)
            {
                GUI.Box(area, "", highlighted);
            }
        }
    }

    public bool RemoveSymbole(Vector2 mousePosition)
    {
        if (symboles != null)
        {
            for (int i = symboles.Count - 1; i > -1; i--)
            {
                var area = symboles[i].NodeSize;
                if (area.Contains(few.InvGraphToScreenSpace(mousePosition)))
                {
                    selectedSymbole = symboles[i];
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("Remove node"), false, () => { Selection.activeObject = null; OnClickRemoveNode(); });
                    genericMenu.ShowAsContext();

                    return true;
                    //callback(symboles[i]);
                }
            }
        }
        return false;
    }

    public void DrawBezierPreview(Vector2 knobeCenter)
    {
        DrawBezier(few.GraphToScreenSpace(knobeCenter), Event.current.mousePosition, Color.white);
    }

    private void DrawConnectionBezier(Vector2 start, Vector2 end, Color inputColor, Color outputColor)
    {


        Vector2 Startquart = Vector2.Lerp(start, end, 0.25f);
        Vector2 Midquart = Vector2.Lerp(start, end, 0.5f);
        Vector2 Endquart = Vector2.Lerp(start, end, 0.75f);

        Vector2 endToStart = (end - start);
        float dirFactor = Mathf.Clamp(endToStart.magnitude, 20f, 40f);

        endToStart.Normalize();
        Vector2 project = Vector3.Project(endToStart, Vector3.right);

        Vector2 startTan = start + project * dirFactor;
        Vector2 endTan = end - project * dirFactor;
        //Handles.DrawBezier(start, few.GraphToScreenSpace(Startquart), startTan, few.GraphToScreenSpace(Startquart), inputColor, null, 3f);
        //Handles.DrawBezier(few.GraphToScreenSpace(Startquart), few.GraphToScreenSpace(Midquart), few.GraphToScreenSpace(Startquart), few.GraphToScreenSpace(Midquart), inputColor, null, 3f);
        //Handles.DrawBezier(few.GraphToScreenSpace(Midquart), few.GraphToScreenSpace(Endquart), few.GraphToScreenSpace(Midquart), few.GraphToScreenSpace(Endquart), outputColor, null, 3f);
        //Handles.DrawBezier(few.GraphToScreenSpace(Endquart), end, few.GraphToScreenSpace(Endquart), endTan, outputColor, null, 3f);
        Handles.DrawBezier(start, few.GraphToScreenSpace(Midquart), startTan, few.GraphToScreenSpace(Midquart), inputColor, null, 3f);
        Handles.DrawBezier(few.GraphToScreenSpace(Midquart), end, few.GraphToScreenSpace(Midquart), endTan, outputColor, null, 3f);
    }

    private void DrawBezier(Vector2 start, Vector2 end, Color color)
    {
        Vector2 endToStart = (end - start);
        float dirFactor = Mathf.Clamp(endToStart.magnitude, 20f, 80f);

        endToStart.Normalize();
        Vector2 project = Vector3.Project(endToStart, Vector3.right);

        Vector2 startTan = start + project * dirFactor;
        Vector2 endTan = end - project * dirFactor;

        Handles.DrawBezier(start, end, startTan, endTan, color, null, 3f);
    }

    private void OnClickRemoveNode()
    {
        var Removeables = points.Where(x => x.symbole == selectedSymbole).ToList();
        if (points != null)
            foreach (var point in points)
                    point.RemoveConnection(selectedSymbole);
        foreach (var removable in Removeables)
        {
            AssetDatabase.RemoveObjectFromAsset(removable);
            points.Remove(removable);
        }
        AssetDatabase.RemoveObjectFromAsset(selectedSymbole);
        symboles.Remove(selectedSymbole);
        selectedSymbole = null;
        AssetDatabase.SaveAssets();
    }

    public void OnClickRemoveAllNode()
    {
        selectedSymbole = null;
        selectedInputPoint = null;
        selectedOutputPoint = null;
        if(RemovedCP != null)
        {
            foreach (var remove in RemovedCP)
            {
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        if (RemovedS != null)
        {
            foreach (var remove in RemovedS)
            {
                Debug.Log(remove.name);
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RemovedCP = points;
        RemovedS = symboles;
        if (symboles != null)
            symboles = null;
        if (points != null)
            points = null;
        Selection.activeObject = null;
    }

    public void RemoveComplete()
    {
        if (RemovedCP != null)
        {
            foreach (var remove in RemovedCP)
            {
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        if (RemovedS != null)
        {
            foreach (var remove in RemovedS)
            {
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void OnClickRestoreAllNodes()
    {
        selectedSymbole = null;
        selectedInputPoint = null;
        selectedOutputPoint = null;
        points = RemovedCP;
        symboles = RemovedS;
        if (RemovedS != null)
            RemovedS = null;
        if (RemovedCP != null)
            RemovedCP = null;
        Selection.activeObject = null;
    }

    public void PanSymbole(Vector2 delta)
    {
        if (symboles != null)
        {
            for (int i = 0; i < symboles.Count; i++)
            {
                symboles[i].NodeSize.position += delta;
            }
        }
    }

    public Rect DragSymbole(Rect rect)
    {
        rect.position += Event.current.delta * few.zScale;
        return rect;
    }

    public bool OnMouseOverSymbole(Vector2 mousePosition)
    {
        mousePosition = few.InvGraphToScreenSpace(mousePosition);
        if (symboles != null)
        {
            selectedSymbole = null;
            for (int i = symboles.Count - 1; i > -1; i--)
            {
                var area = symboles[i].NodeSize;
                if (area.Contains(mousePosition))
                {
                    selectedSymbole = symboles[i];
                    Selection.activeObject = selectedSymbole;
                    //callback(symboles[i]);
                    break;
                }
            }
            if (selectedSymbole)
            {
                MoveSelectedTotop(selectedSymbole);
                return true;
            }
        }
        return false;
    }

    private void MoveSelectedTotop(Symbole symbole)
    {
        var p = points;
        if (symbole != symboles.LastOrDefault())
        {
            symboles.Remove(symbole);
            symboles.Add(symbole);
            foreach (var point in symbole.fieldPoints)
            {
                p.Remove(point);
                p.Add(point);
            }
            points = p;
            GUI.changed = true;
        }
    }

    public bool OnMouseOverInputPoint(Vector2 mousePosition)
    {
        mousePosition = few.InvGraphToScreenSpace(mousePosition);
        if (points != null)
        {
            selectedInputPoint = null;
            for (int i = points.Count - 1; i > -1; i--)
            {
                if (points[i].connectionType == ConnectionType.Output)
                    continue;
                var area = points[i].PointPos;
                if (area.Contains(mousePosition))
                {
                    selectedInputPoint = points[i];
                    //callback(symboles[i]);
                    return true;
                }
            }
        }
        return false;
    }

    public bool OnMouseOverPoint_Symbole(Vector2 mousePosition)
    {
        if (OnMouseOverInputPoint(mousePosition) && !selectedOutputPoint)
        {
            selectedInputPoint.RemoveConnections();
            selectedInputPoint = null;
        }
        if (OnMouseOverOutputPoint(mousePosition))
            return true;
        if (!selectedOutputPoint)
            if (OnMouseOverSymbole(mousePosition))
                return true;
        return false;
    }

    private bool OnMouseOverOutputPoint(Vector2 mousePosition)
    {
        mousePosition = few.InvGraphToScreenSpace(mousePosition);
        if (points != null)
        {
            selectedInputPoint = null;
            for (int i = points.Count - 1; i > -1; i--)
            {
                if (points[i].connectionType == ConnectionType.Input)
                    continue;
                var area = points[i].PointPos;
                if (area.Contains(mousePosition))
                {
                    selectedOutputPoint = points[i];
                    //callback(symboles[i]);
                    break;
                }
            }
            if (selectedOutputPoint)
            {
                MoveSelectedTotop(selectedOutputPoint.symbole);
                return true;
            }
        }
        return false;
    }

    public void AddNewSymbole(Type symboleType, Vector2 mousePosition)
    {
        mousePosition = few.InvGraphToScreenSpace(mousePosition);
        if (symboles == null)
        {
            symboles = new List<Symbole>();
        }
        var symbole = ScriptableObject.CreateInstance(symboleType) as Symbole;
        symbole.NodeSize.position = mousePosition;
        symbole.InitializeAttributes();
        if (symbole.name == string.Empty)
            symbole.name = symbole.GetType().Name;
        Selection.activeObject = symbole;
        symboles.Add(symbole);
        AssetDatabase.AddObjectToAsset(symbole, graph);
        foreach (var connectionPoint in symbole.fieldPoints)
            AssetDatabase.AddObjectToAsset(connectionPoint, graph);
        AssetDatabase.SaveAssets();
    }
}