using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Xml.Serialization;
[Serializable]
public class VCSUGraph : ScriptableObject
{
    public List<Symbole> symboles;
    public List<ConnectionPoint> connectionPoints;
    public List<ConnectionCallPoint> callPoints;
    public XmlDictionary<string, VariableTest> variables = new XmlDictionary<string, VariableTest>();
    public List<string> CallsName = new List<string>();
    public List<int> CallsValue = new List<int>();
    public string XmlData = string.Empty;
    public int OriginalIID;
    public string OriginalPath;

    public void CallFunctionMethod(string name)
    {
        symboles[CallsValue[CallsName.IndexOf(name)]].CallerFunction();
    }

    public void AddCall(string Key, int Value)
    {
        CallsName.Add(Key);
        CallsValue.Add(Value);
    }

    public bool RemoveCall(string Key)
    {
        CallsValue.RemoveAt(CallsName.IndexOf(Key));
        return CallsName.Remove(Key);
    }

    public bool RemoveCall(int Value)
    {
        CallsName.RemoveAt(CallsValue.IndexOf(Value));
        return CallsValue.Remove(Value);
    }

    public bool ContainCall(string Key)
    {
        return CallsName.Contains(Key);
    }

    public bool ContainCall(int Value)
    {
        return CallsValue.Contains(Value);
    }
}