using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
[Serializable]
public class SymboleManager
{
    public List<Symbole> symboles { get{ if (graph) return graph.symboles; return null; } set{ graph.symboles = value; } }
    private GUIStyle background = LoadResources.GetNormalStyle("xnode_node");
    private static GUIStyle highlighted = LoadResources.GetHiglightedStyle("xnode_node_highlight");
    public  Symbole selectedSymbole;
    public ConnectionPoint selectedInputPoint;
    public ConnectionPoint selectedOutputPoint;
    public ConnectionCallPoint selectedInputCallPoint;
    public ConnectionCallPoint selectedOutputCallPoint;
    public List<ConnectionPoint> points { get { if (graph) return graph.connectionPoints; return null; } set{ graph.connectionPoints = value; } }
    public List<ConnectionCallPoint> callPoints { get { if (graph) return graph.callPoints; return null; } set { graph.callPoints = value; } }
    private GUIStyle textStyle;
    private FlowchartEditorWindow few;
    public VCSUGraph graph;
    private static VCSUGraph _graph;
    private static Action<ConnectionPoint> actionAddPoint;
    private static Action<ConnectionCallPoint> actionAddCallPoint;
    public List<Symbole> RemovedS;
    public List<ConnectionPoint> RemovedCP;
    public List<ConnectionCallPoint> RemovedCCP;
    private static Action SSR;
    private static Action<Symbole> setselection;
    public static List<int> selections;
    public bool ShowThread = false;
    public float UpdateTime = 0f;
    public static VariableTest GetVariable(string Name)
    {
        if (!_graph)
            SSR?.Invoke();
        if (_graph)
            return _graph.variables[Name];
        return null;
    }

    public static VariableTest SetVariable(string Name, object value)
    {
        if (!_graph)
            SSR?.Invoke();
        if (_graph)
        {
            _graph.variables[Name].value = value;
            return _graph.variables[Name];
        }
        return null;
    }

    public static List<string> GetVariableEnum()
    {
        if (!_graph)
            SSR?.Invoke();
        if (_graph)
            return _graph.variables.Keys;
        return null;
    }

    public SymboleManager(FlowchartEditorWindow _few)
    {
        few = _few;
        background.alignment = TextAnchor.UpperCenter;
        background.fontStyle = FontStyle.Bold;
        background.normal.textColor = Color.white;
        actionAddPoint = AddPoint;
        actionAddCallPoint = AddCallPoint;
        SSR = SetStaticReferance;
        setselection = SetSelection;
    }

    public void SetSelection(Symbole symbole)
    {
        selectedSymbole = symbole;
    }

    public void SetStaticReferance()
    {
        if (graph)
        {
            _graph = graph;
        }
    }

    public void AddPoint(ConnectionPoint connectionPoint)
    {
        if (points == null)
            points = new List<ConnectionPoint>();
        points.Add(connectionPoint);
    }

    public void AddCallPoint(ConnectionCallPoint callPoint)
    {
        if (callPoints == null)
            callPoints = new List<ConnectionCallPoint>();
        callPoints.Add(callPoint);
    }

    public static void AddSymboleStatic(ConnectionPoint connectionPoint)
    {
        actionAddPoint(connectionPoint);
    }

    public static void AddCallStatic(ConnectionCallPoint callPoint)
    {
        actionAddCallPoint(callPoint);
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
                            DrawConnectionBezier(FlowchartEditorWindow.GraphToScreenSpace(point.PointPos.center), FlowchartEditorWindow.GraphToScreenSpace(input.PointPos.center), point.knobeColor, input.knobeColor );
                        }
        if (callPoints != null)
            foreach (var point in callPoints)
                if (point.connectionType == ConnectionType.Call)
                    foreach (var receive in point.connections)
                        if (receive.connectionType == ConnectionType.Receive)
                        {
                            //var col = Color.Lerp(point.knobeColor, input.knobeColor, 0.5f);
                            DrawBezier(point.Point.center, receive.Point.center, point.knobeColor);
                        }
        if (symboles != null)
        {
            DrawSymboles();
            GUI.changed = true;
        }
        if (selectedOutputCallPoint)
        {
            DrawBezierPreview(few.InvGraphToScreenSpace(selectedOutputCallPoint.Point.center));
            GUI.changed = true;
        }
        if (selectedOutputPoint)
        {
            DrawBezierPreview(selectedOutputPoint.PointPos.center);
            GUI.changed = true;
        }
    }
    private float time = 0;
    public void DrawSymboles()
    {
        int id = 0;
        if (selections != null && selections.Count > 0)
            id = selections[0];
        for (int i = 0; i < symboles.Count; i++)
        {
            var area = symboles[i].NodeSize;
            area.position = FlowchartEditorWindow.GraphToScreenSpace(area.position);
            GUI.Box(area, symboles[i].name.AddWordSpace(), background);
            GUILayout.BeginArea(new Rect(area.position.x - 8, area.position.y + 35, area.width + 16, area.height - 35));
            var r = EditorGUILayout.BeginVertical();
            var lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            if (symboles[i].fieldPoints != null)
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
            if (symboles[i].shouldCall && symboles[i].Call)
                symboles[i].Call.Draw(few.pOffset);
            if (symboles[i].shouldReceive && symboles[i].Receive)
                symboles[i].Receive.Draw(few.pOffset);
            if (symboles[i] == selectedSymbole)
            {
                GUI.Box(area, "", highlighted);
            }
        }
        if (ShowThread)
            time += Time.deltaTime;
        for (int i = 0; i < symboles.Count; i++)
        {
            var area = symboles[i].NodeSize;
            area.position = FlowchartEditorWindow.GraphToScreenSpace(area.position);
            if (symboles[i].GetInstanceID() == id && ShowThread && time > Mathf.Clamp(UpdateTime, 0.4f, 0.6f))
            {
                GUI.Box(area, "", highlighted);
                selections.Remove(id);
                time = 0;
                GUI.changed = true;
            }
        }
    }

    public static void DrawSymbole(int ID)
    {
        if (selections == null)
            selections = new List<int>();
        if (!selections.Contains(ID))
        {
            selections.Add(ID);
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
        DrawBezier(FlowchartEditorWindow.GraphToScreenSpace(knobeCenter), Event.current.mousePosition, Color.white);
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
        Handles.DrawBezier(start, Midquart, startTan, Midquart, inputColor, null, 3f);
        Handles.DrawBezier(Midquart, end, Midquart, endTan, outputColor, null, 3f);
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
        if (selections != null && selections.Count > 0)
            selections.Clear();
        if (points != null && points.Count > 0)
        {
            var PRemoveables = points.Where(x => x.RemoveConnection(selectedSymbole)).ToList();
            foreach (var removable in PRemoveables)
            {
                AssetDatabase.RemoveObjectFromAsset(removable);
                points.Remove(removable);
            }
        }
        if (callPoints != null && points.Count > 0)
        {
            var CPRemoveables = callPoints.Where(x => x.RemoveConnection(selectedSymbole)).ToList();
            foreach (var removable in CPRemoveables)
            {
                AssetDatabase.RemoveObjectFromAsset(removable);
                callPoints.Remove(removable);
            }
        }
        if (selectedSymbole.shouldCall && selectedSymbole.Call)
        {
            AssetDatabase.RemoveObjectFromAsset(selectedSymbole.Call);
        }
        if (selectedSymbole.shouldReceive && selectedSymbole.Receive)
        {
            AssetDatabase.RemoveObjectFromAsset(selectedSymbole.Receive);
        }
        if (graph.ContainCall(selectedSymbole.name))
        {
            graph.RemoveCall(graph.CallsValue[graph.CallsName.IndexOf(selectedSymbole.name)]);
            graph.RemoveCall(selectedSymbole.name);
        }
        AssetDatabase.RemoveObjectFromAsset(selectedSymbole);
        symboles.Remove(selectedSymbole);
        selectedSymbole = null;
        AssetDatabase.SaveAssets();
    }

    public void OnClickRemoveAllNode()
    {
        if (selections != null && selections.Count > 0)
            selections.Clear();
        selectedSymbole = null;
        selectedInputPoint = null;
        selectedOutputPoint = null;
        selectedInputCallPoint = null;
        selectedOutputCallPoint = null;
        if (RemovedCP != null)
        {
            foreach (var remove in RemovedCP)
            {
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        if (RemovedCCP != null)
        {
            foreach (var remove in RemovedCCP)
            {
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        if (RemovedS != null)
        {
            foreach (var remove in RemovedS)
            {
                Debug.Log(remove.name);
                if (remove.shouldCall && remove.Call)
                {
                    AssetDatabase.RemoveObjectFromAsset(remove.Call);
                }
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RemovedCP = points;
        RemovedCCP = callPoints;
        RemovedS = symboles;
        if (symboles != null)
            symboles = null;
        if (points != null)
            points = null;
        if (callPoints != null)
            callPoints = null;
        Selection.activeObject = null;
    }

    public void RemoveComplete()
    {
        if (selections != null && selections.Count > 0)
            selections.Clear();
        if (RemovedCP != null)
        {
            foreach (var remove in RemovedCP)
            {
                AssetDatabase.RemoveObjectFromAsset(remove);
            }
        }
        if (RemovedCCP != null)
        {
            foreach (var remove in RemovedCCP)
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
        selectedInputCallPoint = null;
        selectedOutputCallPoint = null;
        points = RemovedCP;
        callPoints = RemovedCCP;
        symboles = RemovedS;
        if (RemovedS != null)
            RemovedS = null;
        if (RemovedCP != null)
            RemovedCP = null;
        if (RemovedCCP != null)
            RemovedCCP = null;
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
        if (symbole != symboles.LastOrDefault())
        {
            symboles.Remove(symbole);
            symboles.Add(symbole);
            GUI.changed = true;
        }
        new System.Threading.Thread(() =>
        {
            int i = 0;
            foreach (var s in symboles)
            {
                if (s.GetType().GetCustomAttribute(typeof(MethodSymboleAttribute)) != null)
                {
                    graph.ChangeIndex(s.GetType().Name, i);
                }
                i++;
            }
        })
        { IsBackground = true}.Start();
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

    public bool OnMouseOverInputCallPoint(Vector2 mousePosition)
    {
        //mousePosition = few.InvGraphToScreenSpace(mousePosition);
        if (callPoints != null)
        {
            selectedInputPoint = null;
            for (int i = callPoints.Count - 1; i > -1; i--)
            {
                if (callPoints[i].connectionType == ConnectionType.Call)
                    continue;
                var area = callPoints[i].Point;
                if (area.Contains(mousePosition))
                {
                    selectedInputCallPoint = callPoints[i];
                    //callback(symboles[i]);
                    return true;
                }
            }
        }
        return false;
    }

    public bool OnMouseOverPoint_Symbole(Vector2 mousePosition)
    {
        if (OnMouseOverInputCallPoint(mousePosition) && !selectedOutputCallPoint)
        {
            selectedInputCallPoint.RemoveConnections();
            selectedInputCallPoint = null;
        }
        if (OnMouseOverOutputCallPoint(mousePosition))
            return true;
        if (OnMouseOverInputPoint(mousePosition) && !selectedOutputPoint)
        {
            selectedInputPoint.RemoveConnections();
            selectedInputPoint = null;
        }
        if (OnMouseOverOutputPoint(mousePosition))
            return true;
        if (!selectedOutputPoint && !selectedOutputCallPoint)
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

    private bool OnMouseOverOutputCallPoint(Vector2 mousePosition)
    {
        //mousePosition = few.InvGraphToScreenSpace(mousePosition);
        if (callPoints != null)
        {
            selectedInputPoint = null;
            for (int i = callPoints.Count - 1; i > -1; i--)
            {
                if (callPoints[i].connectionType == ConnectionType.Receive)
                    continue;
                var area = callPoints[i].Point;
                if (area.Contains(mousePosition))
                {
                    selectedOutputCallPoint = callPoints[i];
                    break;
                }
            }
            if (selectedOutputCallPoint)
            {
                MoveSelectedTotop(selectedOutputCallPoint.symbole);
                return true;
            }
        }
        return false;
    }

    public void AddNewSymbole(Type symboleType, Vector2 mousePosition)
    {
        if (!graph.ContainCall(symboleType.Name))
        {
            if (selections != null && selections.Count > 0)
                selections.Clear();
            mousePosition = few.InvGraphToScreenSpace(mousePosition);
            if (symboles == null)
            {
                symboles = new List<Symbole>();
            }
            var symbole = ScriptableObject.CreateInstance(symboleType) as Symbole;
            symbole.NodeSize.position = mousePosition;
            symbole.InitializeAttributes();
            Selection.activeObject = symbole;
            symboles.Add(symbole);
            symbole.CloneRename();
            if (symbole.GetType().GetCustomAttribute(typeof(MethodSymboleAttribute)) != null)
            {
                graph.AddCall(symbole.name, symboles.IndexOf(symbole));
            }
            AssetDatabase.AddObjectToAsset(symbole, graph);
            if (symbole.fieldPoints != null)
                foreach (var connectionPoint in symbole.fieldPoints)
                    AssetDatabase.AddObjectToAsset(connectionPoint, graph);
            if (symbole.Call)
                AssetDatabase.AddObjectToAsset(symbole.Call, graph);
            if (symbole.Receive)
                AssetDatabase.AddObjectToAsset(symbole.Receive, graph);
            AssetDatabase.SaveAssets();
        }
    }
}