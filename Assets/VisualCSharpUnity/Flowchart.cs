using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum VariableType
{
    AnimationCurve = 0,
    Bounds = 1,
    BoundsInt = 2,
    Color = 3,
    Color32 = 4,
    Double = 5,
    Enum = 6,
    Single = 7,
    Gradient = 8,
    Object = 9,
    Rect = 10,
    RectInt = 11,
    String = 12,
    Boolean = 13,
    Vector2 = 14,
    Vector2Int = 15,
    Vector3 = 16,
    Vector3Int = 17,
    Vector4 = 18,
    Quaternion = 19,
    Byte = 20,
    Int16 = 21,
    Int32 = 22,
    SByte = 23,
    UInt16 = 24,
    UInt32 = 25,
    UInt64 = 26,
    Int64 = 27
}/*
[Serializable]
public class Variable
{
    public VariableType variableType;
    public string name;
    [SerializeField]
    public VCSUObject value = new VCSUAnimationCurve();
    [HideInInspector]
    public AnimationCurve AnimationCurve;
    [HideInInspector]
    public Bounds Bounds;
    [HideInInspector]
    public BoundsInt BoundsInt;
    [HideInInspector]
    public Color Color;
    [HideInInspector]
    public Color32 Color32;
    [HideInInspector]
    public double Double;
    [HideInInspector]
    public Enum Enum;
    [HideInInspector]
    public float Single;
    [HideInInspector]
    public Gradient Gradient;
    [HideInInspector]
    public UnityEngine.Object Object;
    [HideInInspector]
    public Rect Rect;
    [HideInInspector]
    public RectInt RectInt;
    [HideInInspector]
    public string String;
    [HideInInspector]
    public bool Boolean;
    [HideInInspector]
    public Vector2 Vector2;
    [HideInInspector]
    public Vector2Int Vector2Int;
    [HideInInspector]
    public Vector3 Vector3;
    [HideInInspector]
    public Vector3Int Vector3Int;
    [HideInInspector]
    public Vector4 Vector4;
    [HideInInspector]
    public Quaternion Quaternion;
    [HideInInspector]
    public int Int32;
    [HideInInspector]
    public uint UInt32;
    [HideInInspector]
    public ulong UInt64;
    [HideInInspector]
    public long Int64;
}*/
public class Flowchart : MonoBehaviour
{
    public VCSUGraph graph;
    public XmlDictionary<string, VariableTest> variables { get { return graph.variables; } set { graph.variables = value; } }
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
