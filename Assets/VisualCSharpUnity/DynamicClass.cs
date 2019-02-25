using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace System.CodeDom.Compiler
{
    public partial class DynamicClass
    {
        public string Name;
        public List<string> NameSpace = new List<string>();
        public List<string> Fields = new List<string>();
        public List<string> Methods = new List<string>();
        public Stream Base;
        public void AssignLibrary(GameObject GObj)
        {
            var assambly = Compile(DynamicFile(),Name);
            var t = assambly.GetType(Name);
            GObj.AddComponent(t);
        }

        public void Import(VCSUGraph graph)
        {
            for (int i = 0; i < graph.symboles.Count; i++)
            {
                if (!NameSpace.Contains(graph.symboles[i].GetNameSpace()))
                    NameSpace.Add(graph.symboles[i].GetNameSpace());
            }
            Name = graph.name;
            if (string.IsNullOrEmpty(Name))
                Name = "Test" + ((GetHashCode() < 0) ? GetHashCode() * -1 : GetHashCode());
            foreach (var variable in graph.variables)
                Fields.Add(string.Format("public {0} {1};", variable.Value.variableType, variable.Key));
            for(int i = 0; i < graph.CallsValue.Count; i++)
            {
                Debug.Log(graph.symboles[graph.CallsValue[i]].name);
                Methods.Add(graph.symboles[graph.CallsValue[i]].ToString());
            }
        }

        public string DynamicFile()
        {
            string text = "";
            using (StringWriter writer = new StringWriter())
            {
                if (NameSpace != null && NameSpace.Count > 0)
                {
                    foreach (var np in NameSpace)
                        writer.WriteLine(np);
                    writer.WriteLine();
                }
                writer.WriteLine("public class {0} : MonoBehaviour {{", Name);
                if (Fields != null && Fields.Count > 0)
                {
                    foreach (var f in Fields)
                        writer.WriteLine(f);
                    writer.WriteLine();
                }
                if (Methods != null && Methods.Count > 0)
                {
                    foreach (var m in Methods)
                    {
                        var list = m.Split(',').Distinct().ToList();
                        var l = new List<string>();
                        foreach (var line in list)
                        {
                            if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
                            {
                                if (!l.Contains(line.TrimEnd().TrimStart()))
                                {
                                    l.Add(line.TrimEnd().TrimStart());
                                    writer.WriteLine(line.TrimEnd().TrimStart());
                                }
                            }
                        }
                    }
                }
                writer.WriteLine("}");
                writer.Flush();
                text = writer.ToString();
            }
            return text;
        }

        public void Export()
        {
            if (Base == null)
            {
                Base = new MemoryStream();
                var path = AssetDatabase.GenerateUniqueAssetPath("Assets/VisualCSharpUnity/Graphs/Temp/" + Name + ".cs");
                Name = Path.GetFileNameWithoutExtension(path);
                using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.OpenOrCreate,FileAccess.ReadWrite)))
                {
                    if (NameSpace != null && NameSpace.Count > 0)
                    {
                        foreach (var np in NameSpace)
                            writer.WriteLine(np);
                        writer.WriteLine();
                    }
                    writer.WriteLine("public class {0} : MonoBehaviour {{", Name);
                    if (Fields != null && Fields.Count > 0)
                    {
                        foreach (var f in Fields)
                            writer.WriteLine(f);
                        writer.WriteLine();
                    }
                    if (Methods != null && Methods.Count > 0)
                    {
                        foreach (var m in Methods)
                        {
                            var list = m.Split(',').Distinct().ToList();
                            var l = new List<string>();
                            foreach (var line in list)
                            {
                                if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
                                {
                                    if (!l.Contains(line.TrimEnd().TrimStart()))
                                    {
                                        l.Add(line.TrimEnd().TrimStart());
                                        writer.WriteLine(line.TrimEnd().TrimStart());
                                    }
                                }
                            }
                        }
                    }
                    writer.WriteLine("}");
                    writer.BaseStream.CopyTo(Base);
                }
            }
            else
            {
                var path = AssetDatabase.GenerateUniqueAssetPath("Assets/VisualCSharpUnity/Graphs/Temp/" + Name + ".cs");
                Name = Path.GetFileNameWithoutExtension(path);
                using (Stream stream = File.OpenWrite(path))
                {
                    Base.Seek(0,SeekOrigin.Begin);
                    Base.CopyToAsync(stream);
                }
            }
        }
    }

    partial class DynamicClass
    {
        public Assembly Compile(string source, string name)
        {
            Assembly assembly = null;
            var tree = CSharpSyntaxTree.ParseText(source);
            MetadataReference[] references = new MetadataReference[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location) };
            var compilation = CSharpCompilation.Create(name, new[] { tree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (MemoryStream ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    result.Diagnostics.Where(x => Print(x));
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(ms.ToArray());
                }
            }
            return assembly;
        }

        public bool Print(Diagnostic diagnostic)
        {
            if (diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
            {
                Debug.LogErrorFormat("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                return true;
            }
            return false;
        }
    }
}
