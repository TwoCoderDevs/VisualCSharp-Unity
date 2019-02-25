using UnityEngine;
using UnityEditor;

public class SetVariableSymbole : Symbole
{
    [Ignore] public VariableTest Variable;
    [Input(1, 0.5f, 0, 1, ValueProp.Name)] public string Name;
    [Input(0, 1, 0, 1, ValueProp.Hide)] public object A;
    [Output(0, 1, 0, 1, ValueProp.Hide)] public object Value;

    public override void Init()
    {
        base.Init();
        NodeSize.width += 60;
    }

    public override object GetValue(ConnectionPoint point)
    {
        if (EditorApplication.isPlaying)
        {
            SymboleManager.DrawSymbole(GetInstanceID());
            string name = GetInputValue<string>("Name");
            object value = GetInputValue<object>("A");
            if (!string.IsNullOrEmpty(name))
                Name = name;
            if (point.name == "Value")
            {
                //var Variable = SymboleManager.GetVariable(Name);
                if (Variable != null)
                {
                    return Variable.value = value;
                }
            }
        }
        return base.GetValue(point);
    }

    public override void OnGUI()
    {
        var variableEnum = SymboleManager.GetVariableEnum();
        var stringValue = Name;
        int index = 0;
        if (variableEnum.Contains(stringValue))
            index = variableEnum.IndexOf(stringValue);
        var tmp = Name;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(13);
        if (variableEnum != null && variableEnum.Count > 0)
            Name = variableEnum[EditorGUILayout.Popup(index, variableEnum.ToArray(), GUILayout.Width(NodeSize.width - 13))];
        else
            EditorGUILayout.LabelField("No Variables", GUILayout.Width(NodeSize.width - 13));
        EditorGUILayout.EndHorizontal();
        if (Variable == null)
            Variable = SymboleManager.GetVariable(Name);
        if (Name == tmp)
        {
            Variable = SymboleManager.GetVariable(Name);
        }
        base.OnGUI();
    }


    public override string GetNameSpace()
    {
        return "using System;";
    }

    public override string ToString()
    {
        string b = string.Format("var Value_{0} = {1} = default;", (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), Name);
        if (InputConnected("A"))
        {
            var pa = GetInputPoint("A");
            var id = pa.Connections[0].symbole.GetInstanceID();
            b = string.Format("{0}, var Value_{1} = {2} = {3}_{4};", pa.Connections[0].symbole.ToString(), (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), Name, pa.Connections[0].name, (id < 0) ? id * -1 : id);
        }
        return b;
    }
}