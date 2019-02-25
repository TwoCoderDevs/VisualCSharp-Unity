using UnityEngine;
using UnityEditor;

public class GetVariableSymbole : Symbole
{
    [Ignore]public VariableTest Variable;
    [Input(0, 1, 0, 1, ValueProp.Hide)] public string Name;
    [Output(0, 1, 0, 1, ValueProp.Hide)] public object Value;

    public override void Init()
    {
        base.Init();
        NodeSize.width += 60;
    }

    public override object GetValue(ConnectionPoint point)
    {
        SymboleManager.DrawSymbole(GetInstanceID());
        string name = GetInputValue<string>("Name");
        if (!string.IsNullOrEmpty(name))
            Name = name;
        if (point.name == "Value")
        {
            //var Variable = SymboleManager.GetVariable(Name);
            if (Variable != null)
            {
                return Value = Variable.value;
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
            Name = variableEnum[EditorGUILayout.Popup("Name", index, variableEnum.ToArray(), GUILayout.Width(NodeSize.width - 13))];
        else
            EditorGUILayout.LabelField("Name", "No Variables", GUILayout.Width(NodeSize.width - 13));
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
        return string.Format("var Value_{0} = {1};",(GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(),Name);
    }
}