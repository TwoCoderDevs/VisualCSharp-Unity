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
    public XmlDictionary<string, VariableTest> variables = new XmlDictionary<string, VariableTest>();
    public string XmlName = string.Empty;
}