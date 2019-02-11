using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VCSUGraph))]
public class VCSUGraphDrawer : Editor
{
    private VCSUGraph graph { get { return (VCSUGraph)serializedObject.targetObject; } }
    private XmlDictionary<string, VariableTest> variables { get { return graph.variables; } set { graph.variables = value; } }
    private string TempVariable = string.Empty;
    private bool GUIChanged = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (GUILayout.Button("Open Graph"))
        {
            FlowchartEditorWindow.Init(graph);
        }
        XmlDictionary<string, VariableTest> renameds = new XmlDictionary<string, VariableTest>();
        foreach (var variable in variables)
        {
            var name = variable.Value.name;
            variable.Value.name = EditorGUILayout.TextField(variable.Value.name);
            var variableType = variable.Value.variableType;
            variable.Value.variableType = (VariableType)EditorGUILayout.EnumPopup(variable.Value.variableType);
            if (variableType != variable.Value.variableType)
                GUIChanged = true;
            var value = GUIExtended.DrawGUISwitch(variable.Value.variableType.ToString(), variable.Value.value);
            if(value.Item1 == true)
                GUIChanged = true;
            variable.Value.value = value.Item2;
            if (name != variable.Value.name)
                renameds.Add(variable.Key, variable.Value);
        }
        foreach (var renamed in renameds)
        {
            var index = variables.Values.IndexOf(renamed.Value);
            variables.RemoveAt(index);
            if (variables.Count < index)
                variables.Add(renamed.Key, renamed.Value);
            else
                variables.Insert(index, new XmlKeyValuePair<string, VariableTest>(renamed.Key, renamed.Value));
        }
        TempVariable = EditorGUILayout.TextField(TempVariable);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ADD") && !string.IsNullOrEmpty(TempVariable))
        {
            var variable = new VariableTest();
            variable.name = TempVariable;
            variables.Add(TempVariable, variable);
            TempVariable = string.Empty;
            GUIChanged = true;
        }
        if (GUILayout.Button("REMOVE"))
        {
            if (variables.Count > 0)
            {
                variables.RemoveAt(variables.IndexOf(variables.Last()));
                GUIChanged = true;
            }
        }
        GUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    public void OnEnable()
    {
        if (!string.IsNullOrEmpty(graph.XmlName) && File.Exists(graph.XmlName))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlDictionary<string, VariableTest>));
            using (StreamReader reader = new StreamReader(graph.XmlName))
            {
                variables = (XmlDictionary<string, VariableTest>)xmlSerializer.Deserialize(reader);
            }
        }
    }

    public void OnDisable()
    {
        if (string.IsNullOrEmpty(graph.XmlName) || !File.Exists(graph.XmlName))
        {
            graph.XmlName = AssetDatabase.GenerateUniqueAssetPath("Assets/Serialized Data/" + graph.GetInstanceID() + ".txt");
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
        }
        if (GUIChanged)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlDictionary<string, VariableTest>));
            using (StreamWriter writer = new StreamWriter(graph.XmlName))
            {
                xmlSerializer.Serialize(writer, variables);
            }
            AssetDatabase.Refresh();
            GUIChanged = false;
        }
    }
}
[Serializable]
public class XmlKeyValuePair<TKey, Tvalue>
{
    public TKey Key;
    public Tvalue Value;

    public XmlKeyValuePair()
    {
    }

    public XmlKeyValuePair(TKey key, Tvalue value)
    {
        this.Key = key;
        this.Value = value;
    }
}
[Serializable]
public class XmlDictionary<TKey, Tvalue> : List<XmlKeyValuePair<TKey, Tvalue>>
{
    public List<TKey> Keys { get { var keys = base.ToArray();  return keys.Select(x => x.Key).ToList(); }}
    public List<Tvalue> Values { get { var values = base.ToArray(); return values.Select(x => x.Value).ToList(); } }

    public Tvalue this[TKey key]
    {
        get
        {
            if (Keys.Contains(key))
                return base[Keys.IndexOf(key)].Value;
            return default;
        }

        set
        {
            if (Keys.Contains(key))
                base[Keys.IndexOf(key)].Value = value;
            else
            {
                Keys.Add(key);
                base.Add(new XmlKeyValuePair<TKey, Tvalue>(key,value));
            }
        }
    }

    public void Add(TKey key, Tvalue value)
    {
        if (!Keys.Contains(key))
        {
            base.Add(new XmlKeyValuePair<TKey, Tvalue>(key, value));
        }
    }

    public new bool RemoveAt(int index)
    {
        Debug.Log(base.Count > 0 && Keys.Count > 0);
        if (base.Count > 0 && Keys.Count > 0)
        {
            base.RemoveAt(index);
            return true;
        }
        return false;
    }
}
/*[CustomEditor(typeof(VCSUGraph))]
public class VCSUGraphDrawer : Editor
{
    private VCSUGraph graph { get { return (VCSUGraph)serializedObject.targetObject; } }
    private List<VariableTest> variables { get { return graph.variables; } set { graph.variables = value; } }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        foreach (var variable in variables)
        {
            variable.name = EditorGUILayout.TextField(variable.name);
            variable.variableType = (VariableType)EditorGUILayout.EnumPopup(variable.variableType);
            variable.value = GUIExtended.DrawGUISwitch(variable.variableType.ToString(),variable.value).Item2;
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ADD"))
        {
            variables.Add(new VariableTest());
        }
        if (GUILayout.Button("REMOVE"))
            variables.Remove(variables.Last());
        GUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    public void OnEnable()
    {
        if (!string.IsNullOrEmpty(graph.XmlName) && File.Exists(graph.XmlName))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VariableTest>), new XmlRootAttribute("VariableTest"));
            using (StreamReader reader = new StreamReader(graph.XmlName))
            {
                variables = (List<VariableTest>)xmlSerializer.Deserialize(reader);
            }
        }
    }

    public void OnDisable()
    {
        if (string.IsNullOrEmpty(graph.XmlName) || !File.Exists(graph.XmlName))
        {
            graph.XmlName = AssetDatabase.GenerateUniqueAssetPath("Assets/Serialized Data/" + graph.GetInstanceID() + ".txt");
            Debug.Log(graph.XmlName);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
        }
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VariableTest>), new XmlRootAttribute("VariableTest"));
        using (StreamWriter writer = new StreamWriter(graph.XmlName))
        {
            xmlSerializer.Serialize(writer, variables);
        }
    }
}*/

[XmlInclude(typeof(AnimationCurve))]
[XmlInclude(typeof(Bounds))]
[XmlInclude(typeof(BoundsInt))]
[XmlInclude(typeof(Color))]
[XmlInclude(typeof(Color32))]
[XmlInclude(typeof(double))]
[XmlInclude(typeof(Enum))]
[XmlInclude(typeof(float))]
[XmlInclude(typeof(Gradient))]
[XmlInclude(typeof(UnityEngine.Object))]
[XmlInclude(typeof(Rect))]
[XmlInclude(typeof(RectInt))]
[XmlInclude(typeof(string))]
[XmlInclude(typeof(bool))]
[XmlInclude(typeof(Vector2))]
[XmlInclude(typeof(Vector2Int))]
[XmlInclude(typeof(Vector3))]
[XmlInclude(typeof(Vector3Int))]
[XmlInclude(typeof(Vector4))]
[XmlInclude(typeof(Quaternion))]
[XmlInclude(typeof(byte))]
[XmlInclude(typeof(short))]
[XmlInclude(typeof(int))]
[XmlInclude(typeof(sbyte))]
[XmlInclude(typeof(ushort))]
[XmlInclude(typeof(uint))]
[XmlInclude(typeof(ulong))]
[XmlInclude(typeof(long))]
[XmlInclude(typeof(VariableTest))]
[Serializable]
public class VariableTest
{
    public string name;
    public VariableType variableType = VariableType.AnimationCurve;
    public object value = new AnimationCurve();
}