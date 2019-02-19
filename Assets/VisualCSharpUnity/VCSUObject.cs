using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

[Serializable]
public class VCSUObject
{
    public string objectName;
    public bool isInScene { get { return SceneID != 0; } }
    public string Path;
    public long SceneID;
    public int TempID;

    public static VCSUGraph Clone(VCSUGraph graph)
    {
        var clone = ScriptableObject.CreateInstance<VCSUGraph>();
        if(graph.symboles != null)
        {
            clone.symboles = new List<Symbole>();

            foreach (var symbole in graph.symboles)
            {
                var tmp = UnityEngine.Object.Instantiate(symbole);
                tmp.fieldPoints.Clear();
                clone.symboles.Add(tmp);
            }

            if (graph.connectionPoints != null)
            {
                clone.connectionPoints = graph.connectionPoints.Select(x => UnityEngine.Object.Instantiate(x)).ToList();
                List<int> indexs = new List<int>();
                foreach (var point in clone.connectionPoints)
                {
                    var sIndex = graph.symboles.IndexOf(point.symbole);
                    var pIndex = clone.connectionPoints.IndexOf(point);
                    point.symbole = clone.symboles[sIndex];
                    if (graph.connectionPoints[pIndex].Connections != null && graph.connectionPoints[pIndex].Connections.Count > 0)
                    {
                        indexs = graph.connectionPoints[pIndex].Connections.Select(x => graph.connectionPoints.IndexOf(x)).ToList();
                        point.Connections.Clear();
                        foreach (var index in indexs)
                        {
                            point.AddConnection(clone.connectionPoints[index]);
                        }
                    }
                    point.symbole.fieldPoints.Add(point);
                }
            }
            clone.variables = graph.variables;
            SerializeData(clone);
            clone.OriginalIID = graph.GetInstanceID();
            clone.OriginalPath = AssetDatabase.GetAssetOrScenePath(clone);
        }
        return clone;
    }

    private static void SerializeData(VCSUGraph graph)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlDictionary<string, VariableTest>));
        using (StringWriter writer = new StringWriter())
        {
            xmlSerializer.Serialize(writer, graph.variables);
            graph.XmlData = writer.ToString();
        }
        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
    }

    public static void ApplyFromClone(VCSUGraph graph, VCSUGraph OriginalGraph)
    {
        if (graph.symboles != null)
        {
            OriginalGraph.symboles = new List<Symbole>();

            OriginalGraph.symboles.Clear();
            foreach (var symbole in graph.symboles)
            {
                var tmp = UnityEngine.Object.Instantiate(symbole);
                tmp.fieldPoints.Clear();
                OriginalGraph.symboles.Add(tmp);
            }

            if (graph.connectionPoints != null)
            {
                OriginalGraph.connectionPoints.Clear();
                OriginalGraph.connectionPoints = graph.connectionPoints.Select(x => UnityEngine.Object.Instantiate(x)).ToList();
                List<int> indexs = new List<int>();
                foreach (var point in OriginalGraph.connectionPoints)
                {
                    var sIndex = graph.symboles.IndexOf(point.symbole);
                    var pIndex = OriginalGraph.connectionPoints.IndexOf(point);
                    point.symbole = OriginalGraph.symboles[sIndex];
                    Debug.Log(graph.connectionPoints[pIndex].Connections != null && graph.connectionPoints[pIndex].Connections.Count > 0);
                    if (graph.connectionPoints[pIndex].Connections != null && graph.connectionPoints[pIndex].Connections.Count > 0)
                    {
                        indexs = graph.connectionPoints[pIndex].Connections.Select(x => graph.connectionPoints.IndexOf(x)).ToList();
                        point.Connections.Clear();
                        foreach (var index in indexs)
                        {
                            point.AddConnection(OriginalGraph.connectionPoints[index]);
                        }
                    }
                    point.symbole.fieldPoints.Add(point);
                }
            }
            OriginalGraph.variables = graph.variables;
            SerializeData(OriginalGraph);
        }
    }
}