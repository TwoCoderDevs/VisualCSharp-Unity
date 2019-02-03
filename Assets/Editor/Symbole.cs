using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public abstract class Symbole : ScriptableObject, IDisposable
{
    public Rect NodeSize = new Rect(0,0,100,200);
    public List<ConnectionPoint> fieldPoints { get; private set; }
    private List<FieldInfo> childFields;
    private bool NoOutput = true;
    public virtual void Update()
    {

    }

    public void InitializeAttributes()
    {
        Type type = this.GetType();
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            var attris = field.GetCustomAttributes(true);
            if (attris != null && attris.Length > 0)
            {
                var attri = attris[0];
                var cp = CreateInstance<ConnectionPoint>();
                cp.Init(this, field, (PointAttribute)attri);
                if (SymboleManager.points == null)
                    SymboleManager.points = new List<ConnectionPoint>();
                if (fieldPoints == null)
                    fieldPoints = new List<ConnectionPoint>();
                if (cp.connectionType == ConnectionType.Output)
                    NoOutput = false;
                fieldPoints.Add(cp);
                SymboleManager.points.Add(cp);
            }
            else
            {
                if (field.IsPublic && !field.IsStatic && field.Name != "NodeSize")
                {
                    if (childFields == null)
                        childFields = new List<FieldInfo>();
                    childFields.Add(field);
                }
            }
        }
        Init();
    }

    public virtual void Init()
    {

    }

    public void Serializeds()
    {
        
    }

    public virtual void OnGUI()
    {
        foreach (var childField in childFields)
        {
            if (childField.FieldType.IsEnum)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(13);
                SetValue(childField, EditorGUILayout.EnumPopup(childField.Name.AddWordSpace(), GetValue<Enum>(childField), GUILayout.Width(NodeSize.width - 13)));
                EditorGUILayout.EndHorizontal();
            } 
        }
        foreach (var field in fieldPoints)
            if (field.connectionType == ConnectionType.Output && field.Connections.Count > 0)
            {
                var c = GetValue(field);
                field.SetValue(c);
            }
        if (NoOutput)
            Update();
    }

    public virtual object GetValue(ConnectionPoint point)
    {

        return default;
    }

    public T GetInputValue<T>(string fieldname)
    {
        foreach (var field in fieldPoints)
            if (field.name == fieldname)
            {
                if (field && field.Connections.Count > 0)
                {
                    field.SetValue(field.Connections[0].GetValue<T>());
                    return field.GetValue<T>();
                }
                else
                    return field.GetValue<T>();
            }

        return (T)default;
    }

    public bool InputConnected(string fieldname)
    {
        foreach (var field in fieldPoints)
            if (field.name == fieldname)
            {
                if (field && field.Connections.Count > 0)
                {
                    return true;
                }
            }

        return false;
    }

    public T GetOutputValue<T>(string fieldname)
    {
        foreach (var field in childFields)
            if (field.Name == fieldname)
                if (field != null)
                    return (T)field.GetValue(this);

        return (T)default;
    }

    public void SetOutputValue(FieldInfo field, object value)
    {
        if (field != null)
            field.SetValue(this, value);
    }

    public T GetValue<T>(FieldInfo field)
    {
        if (field != null)
            return (T)field.GetValue(this);

        return (T)default;
    }

    public void SetValue(FieldInfo field, object value)
    {
        if (field != null)
            field.SetValue(this, value);
    }

    public void Dispose()
    {
        fieldPoints.Clear();
        fieldPoints = null;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InputAttribute : PointAttribute
    {
        public InputAttribute(float r = 0, float g = 0, float b = 0, float a = 1, ValueProp valueProp = ValueProp.Show, ConnectionType connectionType = ConnectionType.Input)
        {
            this.color = new Color(r,g,b,a);
            this.connectionType = connectionType;
            this.valueProp = valueProp;
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OutputAttribute : PointAttribute
    {
        public OutputAttribute(float r = 0, float g = 0, float b = 0, float a = 1, ValueProp valueProp = ValueProp.Show, ConnectionType connectionType = ConnectionType.Output)
        {
            this.color = new Color(r, g, b, a);
            this.connectionType = connectionType;
            this.valueProp = valueProp;
        }
    }
}

public static class StringExtend
{
    public static string AddWordSpace(this string str)
    {
        string result = string.Empty;
        char[] letters = str.ToCharArray();
        for (int i = 0; i < letters.Length; i++)
        {
            if (letters.Length > 3)
                if (i == 0 && letters[i] == 'm' && letters[1] == '_')
                {
                    result += letters[2].ToString().ToUpper();
                    i = 3;
                }
            if (letters[i].ToString() != letters[i].ToString().ToLower())
                result += " " + letters[i];
            else
                result += letters[i].ToString();
        }
        return result;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class PointAttribute : Attribute
{
    public Color color;
    public ConnectionType connectionType;
    public ValueProp valueProp;
    public PointAttribute(float r = 0, float g = 0, float b = 0, float a = 1, ValueProp valueProp = ValueProp.Hide, ConnectionType connectionType = ConnectionType.Input)
    {
        this.color = new Color(r, g, b, a);
        this.connectionType = connectionType;
        this.valueProp = valueProp;
    }
}
