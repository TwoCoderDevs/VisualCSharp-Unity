using UnityEngine;
using UnityEditor;

public class SetVariableSymbole : Symbole
{
    [Ignore] public VariableTest Variable;
    [Input(1, 0.5f, 0, 1, ValueProp.Hide)] public string Name;
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
}