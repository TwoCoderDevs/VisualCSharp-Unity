using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VCSUObject
{
}
[Serializable]
public class VCSUInt : VCSUObject
{
    [SerializeField]
    private int Integer = 0;

    public VCSUInt(int Integer)
    {
        this.Integer = Integer;
    }

    public static implicit operator VCSUInt (int Integer)
    {
        return new VCSUInt(Integer);
    }
    public static implicit operator int(VCSUInt Integer)
    {
        return Integer.Integer;
    }
}
[Serializable]
public class VCSUAnimationCurve : VCSUObject
{
    [SerializeField]
    public AnimationCurve AnimationCurve;

    public VCSUAnimationCurve(AnimationCurve AnimationCurve)
    {
        if (AnimationCurve == null)
            AnimationCurve = new AnimationCurve();
        this.AnimationCurve = AnimationCurve;
    }

    public VCSUAnimationCurve()
    {
        AnimationCurve = new AnimationCurve();
    }

    public static implicit operator VCSUAnimationCurve(AnimationCurve AnimationCurve)
    {
        if (AnimationCurve == null)
            AnimationCurve = new AnimationCurve();
        return new VCSUAnimationCurve(AnimationCurve);
    }
    public static implicit operator AnimationCurve(VCSUAnimationCurve AnimationCurve)
    {
        if (AnimationCurve.AnimationCurve == null)
            AnimationCurve.AnimationCurve = new AnimationCurve();
        return AnimationCurve.AnimationCurve;
    }
}

[CustomPropertyDrawer(typeof(VCSUAnimationCurve))]
public class VCSUAnimationCurveDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.FindPropertyRelative("AnimationCurve").animationCurveValue = EditorGUI.CurveField(position, property.FindPropertyRelative("AnimationCurve").animationCurveValue);
        //base.OnGUI(position, property, label);
    }
}

/*public class VCSUObject
{
    private Type AssignedType;
    private object AssignedObject = new object();

    public new Type GetType()
    {
        return AssignedType;
    }

    public VCSUObject(UnityEngine.Object obj, Type type)
    {
        this.AssignedObject = obj;
        this.AssignedType = type;
    }

    public VCSUObject(Func<object> obj)
    {
        this.AssignedObject = obj();
        this.AssignedType = obj().GetType();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static implicit operator VCSUObject(UnityEngine.Object type)
    {
        return new VCSUObject(type, type.GetType());
    }

    public static implicit operator UnityEngine.Object(VCSUObject VObject)
    {
        return (UnityEngine.Object)VObject.AssignedObject;
    }

    public static implicit operator VCSUObject(int VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator VCSUObject(string VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator VCSUObject(long VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator VCSUObject(double VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator VCSUObject(Single VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator int(VCSUObject VObject)
    {
        return (int)VObject.AssignedObject;
    }

    public static implicit operator string(VCSUObject VObject)
    {
        return (string)VObject.AssignedObject;
    }

    public static implicit operator long(VCSUObject VObject)
    {
        return (long)VObject.AssignedObject;
    }

    public static implicit operator double(VCSUObject VObject)
    {
        return (double)VObject.AssignedObject;
    }

    public static implicit operator Single(VCSUObject VObject)
    {
        return (Single)VObject.AssignedObject;
    }

    public static implicit operator VCSUObject(Rect VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator Rect(VCSUObject VObject)
    {
        return (Rect)VObject.AssignedObject;
    }

    public static implicit operator VCSUObject(AnimationCurve VObject)
    {
        return new VCSUObject(() => VObject);
    }

    public static implicit operator AnimationCurve(VCSUObject VObject)
    {
        return (AnimationCurve)VObject.AssignedObject;
    }
}
*/
