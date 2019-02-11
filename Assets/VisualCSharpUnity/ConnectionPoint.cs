using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ObjectSerializer;

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
    public Texture2D tex;
    public Texture2D hasCon;
    public Texture2D noCon;
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

    public void RemoveConnection(Symbole symbole)
    {
        foreach (var point in symbole.fieldPoints)
            Connections.Remove(point);
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
                    if(field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
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
        if (valueProp == ValueProp.Hide)
        {
            EditorGUILayout.LabelField(name, OutputStyle, GUILayout.Width(symbole.NodeSize.width - 13));
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

public static class GUIExtended
{
    private static T GetValue<T>(object value)
    {
        if (value != null)
        {
            if (typeof(T) != value.GetType())
                return Activator.CreateInstance<T>();
            return (T)value;
        }
        else
            return Activator.CreateInstance<T>();
    }

    private static Tuple<bool, object> SetValue(object value, object last)
    {
        if (value != null && value != last)
            return new Tuple<bool, object>(true, value);

        return new Tuple<bool, object>(false, last);
    }

    public static Tuple<bool, object> DrawGUISwitch(string type, object value)
    {
        var tmp = value;
        switch (type)
        {
            case "AnimationCurve":
                return SetValue(EditorGUILayout.CurveField( GetValue<AnimationCurve>(value)), tmp);

            case "Bounds":
                return SetValue(EditorGUILayout.BoundsField( GetValue<Bounds>(value)), tmp);

            case "BoundsInt":
                return SetValue(EditorGUILayout.BoundsIntField( GetValue<BoundsInt>(value)), tmp);

            case "Color":
                return SetValue(EditorGUILayout.ColorField( GetValue<Color>(value)), tmp);

            case "Color32":
                return SetValue(EditorGUILayout.ColorField( GetValue<Color32>(value)), tmp);

            case "Double":
                return SetValue(EditorGUILayout.DoubleField( GetValue<double>(value)), tmp);

            case "Enum":
                return SetValue(EditorGUILayout.EnumPopup( GetValue<Enum>(value)), tmp);

            case "Single":
                return SetValue(EditorGUILayout.FloatField( GetValue<float>(value)), tmp);

            case "Gradient":
                return SetValue(EditorGUILayout.GradientField( GetValue<Gradient>(value)), tmp);

            case "Object":
                return SetValue(EditorGUILayout.ObjectField( GetValue<UnityEngine.Object>(value), typeof(UnityEngine.Object), true), tmp);

            case "Rect":
                return SetValue(EditorGUILayout.RectField( GetValue<Rect>(value)), tmp);

            case "RectInt":
                return SetValue(EditorGUILayout.RectIntField( GetValue<RectInt>(value)), tmp);

            case "String":
                return SetValue(EditorGUILayout.TextField( GetValue<string>(value)), tmp);

            case "Boolean":
                return SetValue(EditorGUILayout.Toggle( GetValue<bool>(value)), tmp);

            case "Vector2":
                return SetValue(EditorGUILayout.Vector2Field("", GetValue<Vector2>(value)), tmp);

            case "Vector2Int":
                return SetValue(EditorGUILayout.Vector2IntField("", GetValue<Vector2Int>(value)), tmp);

            case "Vector3":
                return SetValue(EditorGUILayout.Vector3Field("", GetValue<Vector3>(value)), tmp);

            case "Vector3Int":
                return SetValue(EditorGUILayout.Vector3IntField("", GetValue<Vector3Int>(value)), tmp);

            case "Vector4":
                return SetValue(EditorGUILayout.Vector4Field("", GetValue<Vector4>(value)), tmp);

            case "Quaternion":
                return SetValue(EditorGUILayout.Vector4Field("", GetValue<Quaternion>(value).GetVector4()).GetQuaternion(), tmp);

            //All Integer Types
            case "Byte":
            case "Int16":
            case "Int32":
            case "SByte":
            case "UInt16":
            case "UInt32":
                return SetValue(EditorGUILayout.IntField( GetValue<int>(value)), tmp);

            case "UInt64":
            case "Int64":
                return SetValue(EditorGUILayout.LongField( GetValue<long>(value)), tmp);

                //
        }
        return new Tuple<bool, object>(false, value);
    }

public static Vector4 GetVector4(this Quaternion quaternion)
    {
        return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
    }

    public static Quaternion GetQuaternion(this Vector4 vector4)
    {
        return new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
    }
}