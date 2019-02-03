using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

public enum ConnectionType { Input, Output}
public enum ValueProp { Show, Hide}
public class ConnectionPoint : ScriptableObject
{
    public Color knobeColor;
    public ConnectionType connectionType;
    private FieldInfo field;
    public ValueProp valueProp;
    public Symbole symbole;
    private Rect PointSize;
    private GUIStyle OutputStyle;
    public List<ConnectionPoint> Connections { get; private set; }

    public void AddConnection(ConnectionPoint connectionPoint)
    {
        if (!Connections.Contains(connectionPoint))
            Connections.Add(connectionPoint);
    }

    public bool RemoveConnection(ConnectionPoint connectionPoint)
    {
        return Connections.Remove(connectionPoint);
    }

    public void RemoveConnection(Symbole symbole)
    {
        foreach (var point in symbole.fieldPoints)
            Connections.Remove(point);
    }

    public Rect PointPos
    { get
        {
            var area = PointSize;
            area.position += symbole.NodeSize.position + new Vector2(-8, 35);
            return area;
        }
    }
    public void Init(Symbole symbole, FieldInfo field, PointAttribute attribute)
    {
        this.symbole = symbole;
        this.name = field.Name;
        this.field = field;
        this.connectionType = attribute.connectionType;
        this.valueProp = attribute.valueProp;
        this.knobeColor = attribute.color;
        Connections = new List<ConnectionPoint>();
        OutputStyle = new GUIStyle();
        OutputStyle.alignment = TextAnchor.MiddleRight;
    }

    public T GetValue<T>()
    {
        if (field != null)
            return (T)field.GetValue(symbole);

        return (T)default;
    }

    public void SetValue(object value)
    {
        if (field != null)
            field.SetValue(symbole, value);
    }

    public static void GetInputValue()
    {

    }

    // Use this for initialization
    public void Draw()
    {
        var rect = EditorGUILayout.BeginHorizontal();
        GUILayout.Space(13);
        if (valueProp == ValueProp.Show)
            SetValue(EditorGUILayout.IntField(field.Name, GetValue<int>(), GUILayout.Width(symbole.NodeSize.width - 13)));
        if (valueProp == ValueProp.Hide)
        {
            EditorGUILayout.LabelField(field.Name,OutputStyle, GUILayout.Width(symbole.NodeSize.width - 13));
        }
        EditorGUILayout.EndHorizontal();
        if (connectionType == ConnectionType.Input)
            PointSize = new Rect(2, rect.y + 3, 10, 10);
        if (connectionType == ConnectionType.Output)
            PointSize = new Rect(rect.width - 13, rect.y + 3, 10, 10);
        GUI.DrawTexture(PointSize, LoadResources.GetTexture("DotCircle", knobeColor));
    }
}

public static class GUIExtended
{
    public static int IntField(Symbole symbole, string name, int value)
    {
        return EditorGUILayout.IntField(name,value);
    }
}