using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Xml.Serialization;
[CustomEditor(typeof(Flowchart))]
public class VCSUGraphDrawer : Editor
{
    private Flowchart flowchart { get { return (Flowchart)serializedObject.targetObject; } }
    private XmlDictionary<string, VariableTest> variables { get { return flowchart.variables; } set { flowchart.variables = value; } }
    private string TempVariable = string.Empty;
    private bool GUIChanged = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        flowchart.graph = (VCSUGraph)EditorGUILayout.ObjectField("Graph", flowchart.graph, typeof(VCSUGraph),true);
        if (GUILayout.Button("Open Graph"))
        {
            FlowchartEditorWindow.Init(flowchart.graph);
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
            if (value.Item1 == true)
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
        if (!string.IsNullOrEmpty(flowchart.graph.XmlName) && File.Exists(flowchart.graph.XmlName))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlDictionary<string, VariableTest>));
            using (StreamReader reader = new StreamReader(flowchart.graph.XmlName))
            {
                variables = (XmlDictionary<string, VariableTest>)xmlSerializer.Deserialize(reader);
            }
        }
    }

    public void OnDisable()
    {
        if (string.IsNullOrEmpty(flowchart.graph.XmlName) || !File.Exists(flowchart.graph.XmlName))
        {
            flowchart.graph.XmlName = AssetDatabase.GenerateUniqueAssetPath("Assets/Serialized Data/" + flowchart.graph.GetInstanceID() + ".txt");
            EditorUtility.SetDirty(flowchart.graph);
            AssetDatabase.SaveAssets();
        }
        if (GUIChanged)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlDictionary<string, VariableTest>));
            using (StreamWriter writer = new StreamWriter(flowchart.graph.XmlName))
            {
                xmlSerializer.Serialize(writer, variables);
            }
            AssetDatabase.Refresh();
            GUIChanged = false;
        }
    }
}

/*[CustomEditor(typeof(Flowchart))]
public class FlowchartEditor : Editor
{
    private ReorderableList reorderableList;
    public Flowchart listPropt { get { return (Flowchart)serializedObject.targetObject; } }

    private void OnEnable()
    {
        var listProp = serializedObject.FindProperty("variables");
        reorderableList = new ReorderableList(listPropt.variables, typeof(Variable), true, true, true, true);
        reorderableList.elementHeight += 20;
        reorderableList.drawElementCallback += DrawElement;
        reorderableList.onRemoveCallback += OnRemove;
    }

    private void OnDisable()
    {
        reorderableList.drawElementCallback -= DrawElement;
        reorderableList.onRemoveCallback -= OnRemove;
    }

    private void OnRemove(ReorderableList list)
    {
        listPropt.variables.RemoveAt(list.index);
    }

    private void DrawElement(Rect rect, int index, bool active, bool focused)
    {
        var listElementTypeProp = serializedObject.FindProperty("variables").GetArrayElementAtIndex(index).FindPropertyRelative("variableType");
        var listElementNameProp = serializedObject.FindProperty("variables").GetArrayElementAtIndex(index).FindPropertyRelative("name");
        var listElementProp = serializedObject.FindProperty("variables").GetArrayElementAtIndex(index);
        listElementNameProp.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + 1.5f, rect.width, 16),"Variable Name", listElementNameProp.stringValue);
        SerializedProperty variable = null;
        switch (((VariableType)listElementTypeProp.enumValueIndex).ToString())
        {
            case "AnimationCurve":
                variable = listElementProp.FindPropertyRelative("AnimationCurve");
                variable.animationCurveValue = EditorGUI.CurveField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.animationCurveValue);
                break;
            case "Bounds":
                variable = listElementProp.FindPropertyRelative("Bounds");
                variable.boundsValue = EditorGUI.BoundsField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.boundsValue);
                break;
            case "BoundsInt":
                variable = listElementProp.FindPropertyRelative("BoundsInt");
                variable.boundsIntValue = EditorGUI.BoundsIntField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.boundsIntValue);
                break;
            case "Color32":
            case "Color":
                variable = listElementProp.FindPropertyRelative("Color");
                variable.colorValue = EditorGUI.ColorField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.colorValue);
                break;
            case "Double":
                variable = listElementProp.FindPropertyRelative("Double");
                variable.doubleValue= EditorGUI.DoubleField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.doubleValue);
                break;
            case "Enum":
                variable = listElementProp.FindPropertyRelative("Enum");
                variable.enumValueIndex = EditorGUI.Popup(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.enumValueIndex, variable.enumDisplayNames);
                break;
            case "Single":
                variable = listElementProp.FindPropertyRelative("Single");
                variable.floatValue = EditorGUI.FloatField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.floatValue);
                break;
            case "Gradient":
                listPropt.variables[index].Gradient = EditorGUI.GradientField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, listPropt.variables[index].Gradient);
                break;
            case "Object":
                variable = listElementProp.FindPropertyRelative("Object");
                variable.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.objectReferenceValue, typeof(UnityEngine.Object), true);
                break;
            case "Rect":
                variable = listElementProp.FindPropertyRelative("Rect");
                variable.rectValue = EditorGUI.RectField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.rectValue);
                break;
            case "RectInt":
                variable = listElementProp.FindPropertyRelative("RectInt");
                variable.rectIntValue = EditorGUI.RectIntField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.rectIntValue);
                break;
            case "String":
                listElementProp.FindPropertyRelative("String");
                variable.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.stringValue);
                break;
            case "Boolean":
                variable = listElementProp.FindPropertyRelative("Boolean");
                variable.boolValue = EditorGUI.Toggle(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.boolValue);
                break;
            case "Vector2":
                variable = listElementProp.FindPropertyRelative("Vector2");
                variable.vector2Value= EditorGUI.Vector2Field(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.vector2Value);
                break;
            case "Vector2Int":
                variable = listElementProp.FindPropertyRelative("Vector2Int");
                variable.vector2IntValue = EditorGUI.Vector2IntField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.vector2IntValue);
                break;
            case "Vector3":
                variable = listElementProp.FindPropertyRelative("Vector3");
                variable.vector3Value = EditorGUI.Vector3Field(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.vector3Value);
                break;
            case "Vector3Int":
                variable = listElementProp.FindPropertyRelative("Vector3Int");
                variable.vector3IntValue = EditorGUI.Vector3IntField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.vector3IntValue);
                break;
            case "Vector4":
                variable = listElementProp.FindPropertyRelative("Vector4");
                variable.vector4Value = EditorGUI.Vector4Field(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.vector4Value);
                break;
            case "Quaternion":
                variable = listElementProp.FindPropertyRelative("Quaternion");
                variable.quaternionValue = EditorGUI.Vector4Field(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.quaternionValue.GetVector4()).GetQuaternion();
                break;
            //All Integer Types
            case "Byte":
            case "Int16":
            case "Int32":
                variable = listElementProp.FindPropertyRelative("Int32");
                variable.intValue = EditorGUI.IntField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.intValue);
                break;
            case "SByte":
            case "UInt16":
            case "UInt32":
                variable = listElementProp.FindPropertyRelative("UInt32");
                variable.intValue = EditorGUI.IntField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.intValue);
                break;
            case "UInt64":
                variable = listElementProp.FindPropertyRelative("UInt64");
                variable.longValue = EditorGUI.LongField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.longValue);
                break;
            case "Int64":
                variable = listElementProp.FindPropertyRelative("Int64");
                variable.longValue = EditorGUI.LongField(new Rect(rect.x, rect.y + 18.5f, rect.width, 16), listElementNameProp.stringValue, variable.longValue);
                break;
                //
        }
        EditorUtility.SetDirty(listPropt);
    }



    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        reorderableList.DoLayoutList();
        /*var listProp = serializedObject.FindProperty("variables");
        var rect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(rect, Color.grey);
        EditorGUILayout.PropertyField(listProp, new GUIContent("Input&Outputs"),true);
        DrawList(listProp);
        EditorGUILayout.EndVertical();*/
        /*serializedObject.ApplyModifiedProperties();
    }

    public void DrawList(SerializedProperty property)
    {
        for (int i = 0; i < property.arraySize; i++)
        {
            var element = property.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("name").stringValue = EditorGUILayout.TextField("Variable Name", element.FindPropertyRelative("name").stringValue);
        }
    }
}*/