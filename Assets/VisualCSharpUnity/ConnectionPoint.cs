using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
public enum ConnectionType { Input, Output, Call, Receive}
public enum ValueProp { Show, Name, Hide, }
public class ConnectionPoint : ScriptableObject
{
    public Color knobeColor;
    public ConnectionType connectionType;
    private FieldInfo field;
    public ValueProp valueProp;
    public Symbole symbole;
    private Rect PointSize;
    private GUIStyle OutputStyle;
    public List<ConnectionPoint> Connections;
    public Texture2D tex;
    public Texture2D hasCon;
    public Texture2D noCon;
    public void OnEnable()
    {
        if (Connections == null)
            Connections = new List<ConnectionPoint>();
        OutputStyle = new GUIStyle();
        OutputStyle.alignment = TextAnchor.MiddleRight;
        if (name.Contains("(Clone)"))
        {
            var i = name.IndexOf("(Clone)");
            name = name.Remove(i);
        }
        if (field == null && !string.IsNullOrEmpty(name))
            field = symbole.GetField(name);
    }

    public void AddConnection(ConnectionPoint connectionPoint)
    {
        if (!Connections.Contains(connectionPoint))
            Connections.Add(connectionPoint);
    }

    public bool RemoveConnection(ConnectionPoint connectionPoint)
    {
        return Connections.Remove(connectionPoint);
    }

    public void RemoveConnections()
    {
        foreach (var con in Connections)
            con.RemoveConnection(this);
        Connections.Clear();
    }

    public bool RemoveConnection(Symbole symbole)
    {
        if (this.symbole == symbole)
            return true;
        if (symbole.fieldPoints != null && symbole.fieldPoints.Count > 0)
            foreach (var point in symbole.fieldPoints)
                Connections.Remove(point);
        return false;
    }

    public Rect PointPos;

    public void SetCenter()
    {
        var area = PointSize;
        area.position += symbole.NodeSize.position + new Vector2(-8, 35);
        PointPos = area;
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
        {
            return (T)field.GetValue(symbole);
        }

        return (T)default;
    }

    public void Set()
    {
        symbole.GUIGetValue(this);
    }

    public void SetValue(object value)
    {
        if (field != null)
        {
            field.SetValue(symbole, value);
        }
    }

    public static void GetInputValue()
    {

    }

    // Use this for initialization
    public void Draw()
    {
        if (!noCon)
            noCon = LoadResources.GetTexture("DotCircle", knobeColor);
        if (!hasCon)
            hasCon = LoadResources.GetTexture("Dot", knobeColor);
        if (field == null)
            field = symbole.GetField(name);
        var rect = EditorGUILayout.BeginHorizontal();
        GUILayout.Space(13);
        if (valueProp == ValueProp.Show)
            switch (field.FieldType.Name)
            {
                case "AnimationCurve":
                    SetValue(EditorGUILayout.CurveField(name, GetValue<AnimationCurve>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Bounds":
                    SetValue(EditorGUILayout.BoundsField(name, GetValue<Bounds>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "BoundsInt":
                    SetValue(EditorGUILayout.BoundsIntField(name, GetValue<BoundsInt>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Color":
                    SetValue(EditorGUILayout.ColorField(name, GetValue<Color>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Color32":
                    SetValue(EditorGUILayout.ColorField(name, GetValue<Color32>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Double":
                    SetValue(EditorGUILayout.DoubleField(name, GetValue<double>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Enum":
                    if (field.GetCustomAttribute<FlagsAttribute>() != null)
                        SetValue(EditorGUILayout.EnumFlagsField(name, GetValue<Enum>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    else
                        SetValue(EditorGUILayout.EnumPopup(name, GetValue<Enum>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Single":
                    var rangeInt = field.GetCustomAttribute<RangeAttribute>();
                    if (rangeInt != null)
                        SetValue(EditorGUILayout.Slider(name, GetValue<float>(), rangeInt.min, rangeInt.max, GUILayout.Width(symbole.NodeSize.width - 13)));
                    else
                        SetValue(EditorGUILayout.FloatField(name, GetValue<float>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Gradient":
                    SetValue(EditorGUILayout.GradientField(name, GetValue<Gradient>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Object":
                    if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                        SetValue(EditorGUILayout.ObjectField(name, GetValue<UnityEngine.Object>(), field.FieldType, true, GUILayout.Width(symbole.NodeSize.width - 13)));
                    else
                        SetValue(EditorGUILayout.ObjectField(name, GetValue<UnityEngine.Object>(), typeof(UnityEngine.Object), true, GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Rect":
                    SetValue(EditorGUILayout.RectField(name, GetValue<Rect>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "RectInt":
                    SetValue(EditorGUILayout.RectIntField(name, GetValue<RectInt>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "String":
                    SetValue(EditorGUILayout.TextField(name, GetValue<string>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Boolean":
                    SetValue(EditorGUILayout.Toggle(name, GetValue<bool>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Vector2":
                    SetValue(EditorGUILayout.Vector2Field(name, GetValue<Vector2>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Vector2Int":
                    SetValue(EditorGUILayout.Vector2IntField(name, GetValue<Vector2Int>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Vector3":
                    SetValue(EditorGUILayout.Vector3Field(name, GetValue<Vector3>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Vector3Int":
                    SetValue(EditorGUILayout.Vector3IntField(name, GetValue<Vector3Int>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Vector4":
                    SetValue(EditorGUILayout.Vector4Field(name, GetValue<Vector4>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "Quaternion":
                    SetValue(EditorGUILayout.Vector4Field(name, GetValue<Quaternion>().GetVector4(), GUILayout.Width(symbole.NodeSize.width - 13)).GetQuaternion());
                    break;
                //All Integer Types
                case "Byte":
                case "Int16":
                case "Int32":
                case "SByte":
                case "UInt16":
                case "UInt32":
                    var rangeFloat = field.GetCustomAttribute<RangeAttribute>();
                    if (rangeFloat != null)
                        SetValue(EditorGUILayout.IntSlider(name, GetValue<int>(), (int)rangeFloat.min, (int)rangeFloat.max, GUILayout.Width(symbole.NodeSize.width - 13)));
                    else
                        SetValue(EditorGUILayout.IntField(name, GetValue<int>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                case "UInt64":
                case "Int64":
                    SetValue(EditorGUILayout.LongField(name, GetValue<long>(), GUILayout.Width(symbole.NodeSize.width - 13)));
                    break;
                    //

            }
        if (valueProp == ValueProp.Name)
        {
            if (connectionType == ConnectionType.Output)
                EditorGUILayout.LabelField(name, OutputStyle, GUILayout.Width(symbole.NodeSize.width - 13));
            else
                EditorGUILayout.LabelField(name, GUILayout.Width(symbole.NodeSize.width - 13));
        }
        EditorGUILayout.EndHorizontal();
        if (rect != Rect.zero)
        {
            if (connectionType == ConnectionType.Input)
                PointSize = new Rect(2, rect.y + 3, 10, 10);
            if (connectionType == ConnectionType.Output)
                PointSize = new Rect(rect.width - 13, rect.y + 3, 10, 10);
        }
        if (Connections.Count > 0)
            tex = hasCon;
        else
            tex = noCon;
        GUI.DrawTexture(PointSize, tex);
        SetCenter();
    }
}