using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.ObjectSerializer;
[Serializable]
public struct Field
{
    public string name;
    public FieldInfo field;
    public Field(string name, FieldInfo field)
    {
        this.name = name;
        this.field = field;
    }

    public void Assign(FieldInfo field)
    {
        this.field = field;
    }
}
public abstract class Symbole : ScriptableObject, IDisposable
{
    public Rect NodeSize = new Rect(0,0,100,200);
    public List<ConnectionPoint> fieldPoints;
    [SerializeField]
    private List<Field> childFields;
    public bool NoOutput = true;
    public void GUIUpdate()
    {
        Update();
    }

    public virtual void Update()
    {

    }

    public void InitializeAttributes()
    {
        Type type = this.GetType();
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            var attri = field.GetCustomAttribute(typeof(PointAttribute));
            if (attri != null)
            {
                //var attri = attris[0];
                    var cp = CreateInstance<ConnectionPoint>();
                    cp.Init(this, field, (PointAttribute)attri);
                    if (fieldPoints == null)
                        fieldPoints = new List<ConnectionPoint>();
                    if (cp.connectionType == ConnectionType.Output)
                        NoOutput = false;
                    fieldPoints.Add(cp);
                    SymboleManager.AddSymboleStatic(cp);
            }
            else
            {
                if (field.IsPublic && !field.IsStatic && field.Name != "NodeSize")
                {
                    if (childFields == null)
                        childFields = new List<Field>();
                    childFields.Add(new Field(field.Name,field));
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
            if (childField.field == null)
                childField.Assign(GetField(childField.name));
            if (childField.field.FieldType.IsEnum)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(13);
                SetValue(childField.field, EditorGUILayout.EnumPopup(childField.name.AddWordSpace(), GetValue<Enum>(childField), GUILayout.Width(NodeSize.width - 13)));
                EditorGUILayout.EndHorizontal();
            } 
        }
        if (!EditorApplication.isPlaying)
        {
            foreach (var field in fieldPoints)
                if (field.connectionType == ConnectionType.Output && field.Connections.Count > 0)
                {
                    var c = GUIGetValue(field);
                    field.SetValue(c);
                }
            if (NoOutput)
                GUIUpdate();
        }
    }

    public object GUIGetValue(ConnectionPoint point)
    {
        return GetValue(point);
    }

    public virtual object GetValue(ConnectionPoint point)
    {

        return default;
    }

    public FieldInfo GetField(string fieldname)
    {
        return GetType().GetField(fieldname);
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
            if (field.name == fieldname)
                if (field.field != null)
                    return (T)field.field.GetValue(this);

        return (T)default;
    }

    public void SetOutputValue(FieldInfo field, object value)
    {
        if (field != null)
            field.SetValue(this, value);
    }

    public T GetValue<T>(Field field)
    {
        if (field.field != null)
            return (T)field.field.GetValue(this);

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
