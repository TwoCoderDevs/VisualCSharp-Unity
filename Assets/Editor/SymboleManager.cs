using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SymboleManager
{
    public List<Symbole> symboles;
    private GUIStyle background = LoadResources.GetNormalStyle("xnode_node");
    private GUIStyle highlighted = LoadResources.GetHiglightedStyle("xnode_node_highlight");
    public Symbole selectedSymbole;
    public ConnectionPoint selectedInputPoint;
    public ConnectionPoint selectedOutputPoint;
    public Vector2 graphSymboleOffset = Vector2.zero;
    public static List<ConnectionPoint> points;
    private GUIStyle textStyle;
    public SymboleManager()
    {
        background.alignment = TextAnchor.UpperCenter;
        background.fontStyle = FontStyle.Bold;
        background.normal.textColor = Color.white;
        points = null;
    }
    public void Draw()
    {
        if (symboles != null)
        {
            DrawSymboles();
        }
        if (selectedOutputPoint)
        {
            DrawBezierPreview(selectedOutputPoint.PointPos.center);
            GUI.changed = true;
        }
        if (points != null)
            foreach (var point in points)
                if (point.connectionType == ConnectionType.Output)
                    foreach (var input in point.Connections)
                        if (input.connectionType == ConnectionType.Input)
                            DrawBezier(point.PointPos.center, input.PointPos.center, point.knobeColor);
    }

    public void DrawSymboles()
    {
        for (int i = 0; i < symboles.Count; i++)
        {
            var area = symboles[i].NodeSize;
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
                area.height = 43 + r.height;
            symboles[i].NodeSize = area;
            if (symboles[i] == selectedSymbole)
            {
                GUI.Box(symboles[i].NodeSize, "", highlighted);
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
                if (area.Contains(mousePosition))
                {
                    selectedSymbole = symboles[i];
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("Remove node"), false, () => new System.Threading.Thread(OnClickRemoveNode).Start());
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
        DrawBezier(knobeCenter, Event.current.mousePosition, Color.white);
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
            points.Remove(removable);
        symboles.Remove(selectedSymbole);
        selectedSymbole = null;
    }

    public void OnClickRemoveAllNode()
    {
        selectedSymbole = null;
        if (symboles != null)
            symboles.Clear();
        if (points != null)
            points.Clear();
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
        rect.position += Event.current.delta;
        return rect;
    }

    public bool OnMouseOverSymbole(Vector2 mousePosition)
    {
        if (symboles != null)
        {
            selectedSymbole = null;
            for (int i = symboles.Count - 1; i > -1; i--)
            {
                var area = symboles[i].NodeSize;
                if (area.Contains(mousePosition))
                {
                    selectedSymbole = symboles[i];
                    //callback(symboles[i]);
                    return true;
                }
            }
        }
        return false;
    }

    public bool OnMouseOverInputPoint(Vector2 mousePosition)
    {
        if (points != null)
        {
            selectedInputPoint = null;
            for (int i = 0; i < points.Count; i++)
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
        if (OnMouseOverOutputPoint(mousePosition))
            return true;
        if (!selectedOutputPoint)
            if (OnMouseOverSymbole(mousePosition))
                return true;
        return false;
    }

    private bool OnMouseOverOutputPoint(Vector2 mousePosition)
    {
        if (points != null)
        {
            selectedInputPoint = null;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].connectionType == ConnectionType.Input)
                    continue;
                var area = points[i].PointPos;
                if (area.Contains(mousePosition))
                {
                    selectedOutputPoint = points[i];
                    //callback(symboles[i]);
                    return true;
                }
            }
        }
        return false;
    }

    public void AddNewSymbole(Type symboleType, Vector2 mousePosition)
    {
        if (symboles == null)
        {
            symboles = new List<Symbole>();
        }
        var symbole = ScriptableObject.CreateInstance(symboleType) as Symbole;
        symbole.NodeSize.position = mousePosition;
        symbole.InitializeAttributes();
        if (symbole.name == string.Empty)
            symbole.name = symbole.GetType().Name;
        symboles.Add(symbole);
    }
}