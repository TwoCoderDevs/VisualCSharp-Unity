using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
[Serializable]
public struct Field
{
    public string name;
    public FieldInfo field;
    public bool isVariableEnum;
    public bool isEnumLable;
    public Field(string name, FieldInfo field, bool isVariableEnum = false, bool isEnumLable = false)
    {
        this.name = name;
        this.field = field;
        this.isVariableEnum = isVariableEnum;
        this.isEnumLable = isEnumLable;
    }

    public void Assign(FieldInfo field)
    {
        this.field = field;
    }
}
public abstract class Symbole : ScriptableObject
{
    public ConnectionCallPoint Receive;
    public ConnectionCallPoint Call;
    public bool shouldCall = false;
    public bool shouldReceive = false;
    public Rect NodeSize = new Rect(0,0,100,200);
    public List<ConnectionPoint> fieldPoints;
    [SerializeField]
    private List<Field> childFields;
    public bool NoOutput = true;
    public void GUIUpdate()
    {
        Function();
    }

    public void CloneRename()
    {
        if (name.Contains("(Clone)"))
        {
            var i = name.IndexOf("(Clone)");
            name = name.Remove(i);
        }
    }

    public virtual void Function()
    {

    }

    public virtual void CallerFunction()
    {

    }

    public void InitializeAttributes()
    {
        Type type = this.GetType();
        if (name == string.Empty)
            name = type.Name;
        if (type.GetCustomAttribute(typeof(ReceivableAttribute)) != null)
        {
            shouldReceive = true;
            Receive = CreateInstance<ConnectionCallPoint>();
            Receive.Init(this, ConnectionType.Receive);
            Receive.name = name+"Receive";
            SymboleManager.AddCallStatic(Receive);
        }
        if (type.GetCustomAttribute(typeof(CallableAttribute)) != null)
        {
            shouldCall = true;
            Call = CreateInstance<ConnectionCallPoint>();
            Call.Init(this, ConnectionType.Call);
            Call.name = name + "Receive";
            SymboleManager.AddCallStatic(Call);
        }
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                continue;
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
                if (field.IsPublic && !field.IsStatic && field.Name != "NodeSize" && field.Name != "fieldPoints" && field.Name != "NoOutput" && field.Name != "shouldCall" && field.Name != "shouldReceive")
                {
                    if (childFields == null)
                        childFields = new List<Field>();
                    if (field.GetCustomAttribute(typeof(VariableEnumAttribute)) != null)
                    {
                        childFields.Add(new Field(field.Name, field, true));
                    }
                    else if(field.GetCustomAttribute(typeof(EnumLableAttribute)) != null)
                    {
                        childFields.Add(new Field(field.Name, field, isEnumLable: true));
                    }
                    else
                    {
                        childFields.Add(new Field(field.Name, field, false));
                    }
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

    public void OnEnable()
    {
        if (childFields != null)
            foreach (var childField in childFields)
            {
                if (childField.field == null)
                    childField.Assign(GetField(childField.name));
            }
    }

    public virtual void OnGUI()
    {
        if (childFields != null)
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
                    continue;
                }
            }
        if (!EditorApplication.isPlaying)
        {
            /*foreach (var field in fieldPoints)
                if (field.connectionType == ConnectionType.Output && field.Connections.Count > 0)
                {
                    var c = GUIGetValue(field);
                    field.SetValue(c);
                }*/
            //if (NoOutput)
                GUIUpdate();
        }
    }

    public void GUIGetValue(ConnectionPoint point)
    {
        point.SetValue(GetValue(point));
    }

    public virtual object GetValue(ConnectionPoint point)
    {

        return default;
    }

    public virtual object SetValue(ConnectionPoint point)
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
                    field.Connections[0].Set();
                    field.SetValue(field.Connections[0].GetValue<T>());
                    return field.GetValue<T>();
                }
                else
                {
                    return field.GetValue<T>();
                }
            }
        return (T)default;
    }

    public ConnectionPoint GetInputPoint(string fieldname)
    {
        foreach (var field in fieldPoints)
            if (field.name == fieldname)
            {
                return field;
            }
        return default;
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

    public virtual string GetNameSpace()
    {
        return "using UnityEngine;";
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
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class VariableEnumAttribute : Attribute
{
    
    public VariableEnumAttribute()
    {

    }
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class EnumLableAttribute : Attribute
{

    public EnumLableAttribute()
    {

    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class IgnoreAttribute : Attribute
{

    public IgnoreAttribute()
    {

    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CallableAttribute : Attribute
{

    public CallableAttribute()
    {

    }
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ReceivableAttribute : Attribute
{

    public ReceivableAttribute()
    {

    }
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MethodSymboleAttribute : Attribute
{

    public MethodSymboleAttribute()
    {

    }
}