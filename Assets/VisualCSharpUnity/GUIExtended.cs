using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

public static class GUIExtended
{
    private static T GetValue<T>(object value)
    {
        if (value != null)
        {
            if (typeof(T) != value.GetType())
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)string.Empty;
                }
                var init = Activator.CreateInstance<T>();
                if (!init.GetType().IsAssignableFrom(value.GetType()))
                    return init;
            }
            return (T)value;
        }
        else
            return Activator.CreateInstance<T>();
    }

    private static UnityEngine.Object GetObjectValue(object value)
    {
        UnityEngine.Object gobj = null;
        if (value != null && value.GetType() == typeof(VCSUObject))
        {
            var file = (VCSUObject)value;
            if (!string.IsNullOrEmpty(file.objectName))
            {
                if (file.isInScene)
                {
                    gobj = UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name == file.objectName && GetLocalIdentifierInFile(x) == file.SceneID).FirstOrDefault();
                }
                if (!gobj)
                    gobj = UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name == file.objectName && x.GetInstanceID() == file.TempID).FirstOrDefault();
            }
            if (!gobj)
                gobj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file.Path);
            if (gobj)
            {
                return gobj;
            }
        }

        return new UnityEngine.Object();
    }

    private static long GetLocalIdentifierInFile(UnityEngine.Object obj)
    {
        long fID = 0;
        SerializedObject sObj = new SerializedObject(obj);
        PropertyInfo pInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        if (pInfo != null)
            pInfo.SetValue(sObj, InspectorMode.Debug, null);
        SerializedProperty sProp = sObj.FindProperty("m_LocalIdentfierInFile");
        fID = sProp.longValue;
        return fID;
    }

    private static Tuple<bool, object> SetObjectValue(UnityEngine.Object value, object tmp)
    {
        if (value != null)
        {
            if (tmp != null && tmp.GetType() == typeof(VCSUObject))
            {
                var val = (VCSUObject)tmp;
                if (val.SceneID == GetLocalIdentifierInFile(value) && val.objectName == value.name && val.Path == AssetDatabase.GetAssetPath(value))
                    return new Tuple<bool, object>(true, tmp);
            }
            var file = new VCSUObject { SceneID = GetLocalIdentifierInFile(value), objectName = value.name, Path = AssetDatabase.GetAssetPath(value), TempID = value.GetInstanceID() };
            return new Tuple<bool, object>(true, file);
        }

        return new Tuple<bool, object>(true, new VCSUObject());
    }

    private static Tuple<bool, object> SetValue(object value, object last)
    {
        if (value != null && value != last)
            return new Tuple<bool, object>(true, value);

        return new Tuple<bool, object>(false, last);
    }

    private static object SetValueGUI(object value, object last)
    {
        if (value != null && value != last)
            return value;
        if (value == last)
            return last;
        return new object();
    }

    public static Tuple<bool, object> DrawGUIUObject(object value)
    {
        var tmp = value;
        return SetObjectValue(EditorGUILayout.ObjectField(GetObjectValue(value), typeof(UnityEngine.Object), true), tmp);
    }
    public static Tuple<bool, object> DrawGUISwitch(string type, object value)
    {
        var tmp = value;
        switch (type)
        {
            case "AnimationCurve":
                return SetValue(EditorGUILayout.CurveField(GetValue<AnimationCurve>(value)), tmp);

            case "Bounds":
                return SetValue(EditorGUILayout.BoundsField(GetValue<Bounds>(value)), tmp);

            case "BoundsInt":
                return SetValue(EditorGUILayout.BoundsIntField(GetValue<BoundsInt>(value)), tmp);

            case "Color":
                return SetValue(EditorGUILayout.ColorField(GetValue<Color>(value)), tmp);

            case "Color32":
                return SetValue(EditorGUILayout.ColorField(GetValue<Color32>(value)), tmp);

            case "Double":
                return SetValue(EditorGUILayout.DoubleField(GetValue<double>(value)), tmp);

            case "Enum":
                return SetValue(EditorGUILayout.EnumPopup(GetValue<Enum>(value)), tmp);

            case "Single":
                return SetValue(EditorGUILayout.FloatField(GetValue<float>(value)), tmp);

            case "Gradient":
                return SetValue(EditorGUILayout.GradientField(GetValue<Gradient>(value)), tmp);

            case "Rect":
                return SetValue(EditorGUILayout.RectField(GetValue<Rect>(value)), tmp);

            case "RectInt":
                return SetValue(EditorGUILayout.RectIntField(GetValue<RectInt>(value)), tmp);

            case "String":
                return SetValue(EditorGUILayout.TextField(GetValue<string>(value)), tmp);

            case "Boolean":
                return SetValue(EditorGUILayout.Toggle(GetValue<bool>(value)), tmp);

            case "Vector2":
                return SetValue(EditorGUILayout.Vector2Field("", GetValue<Vector2>(value)), tmp);

            case "Vector2Int":
                return SetValue(EditorGUILayout.Vector2IntField("", GetValue<Vector2Int>(value)), tmp);

            case "Vector3":
                return SetValue(EditorGUILayout.Vector3Field("", GetValue<Vector3>(value)), tmp);

            case "Vector3Int":
                return SetValue(EditorGUILayout.Vector3IntField("", GetValue<Vector3Int>(value)), tmp);

            case "Vector4":
                return SetValue(EditorGUILayout.Vector4Field("", GetValue<Vector4>(value)), tmp);

            case "Quaternion":
                return SetValue(EditorGUILayout.Vector4Field("", GetValue<Quaternion>(value).GetVector4()).GetQuaternion(), tmp);

            //All Integer Types
            case "Byte":
            case "Int16":
            case "Int32":
            case "SByte":
            case "UInt16":
            case "UInt32":
                return SetValue(EditorGUILayout.IntField(GetValue<int>(value)), tmp);

            case "UInt64":
            case "Int64":
                return SetValue(EditorGUILayout.LongField(GetValue<long>(value)), tmp);

                //
        }
        return new Tuple<bool, object>(false, value);
    }

    public static object GUISwitch(string type, object value)
    {
        var tmp = value;
        switch (type)
        {
            case "AnimationCurve":
                return SetValueGUI(EditorGUILayout.CurveField(GetValue<AnimationCurve>(value)), tmp);

            case "Bounds":
                return SetValueGUI(EditorGUILayout.BoundsField(GetValue<Bounds>(value)), tmp);

            case "BoundsInt":
                return SetValueGUI(EditorGUILayout.BoundsIntField(GetValue<BoundsInt>(value)), tmp);

            case "Color":
                return SetValueGUI(EditorGUILayout.ColorField(GetValue<Color>(value)), tmp);

            case "Color32":
                return SetValueGUI(EditorGUILayout.ColorField(GetValue<Color32>(value)), tmp);

            case "Double":
                return SetValueGUI(EditorGUILayout.DoubleField(GetValue<double>(value)), tmp);

            case "Enum":
                return SetValueGUI(EditorGUILayout.EnumPopup(GetValue<Enum>(value)), tmp);

            case "Single":
                return SetValueGUI(EditorGUILayout.FloatField(GetValue<float>(value)), tmp);

            case "Gradient":
                return SetValueGUI(EditorGUILayout.GradientField(GetValue<Gradient>(value)), tmp);

            case "Rect":
                return SetValueGUI(EditorGUILayout.RectField(GetValue<Rect>(value)), tmp);

            case "RectInt":
                return SetValueGUI(EditorGUILayout.RectIntField(GetValue<RectInt>(value)), tmp);

            case "String":
                return SetValueGUI(EditorGUILayout.TextField(GetValue<string>(value)), tmp);

            case "Boolean":
                return SetValueGUI(EditorGUILayout.Toggle(GetValue<bool>(value)), tmp);

            case "Vector2":
                return SetValueGUI(EditorGUILayout.Vector2Field("", GetValue<Vector2>(value)), tmp);

            case "Vector2Int":
                return SetValueGUI(EditorGUILayout.Vector2IntField("", GetValue<Vector2Int>(value)), tmp);

            case "Vector3":
                return SetValueGUI(EditorGUILayout.Vector3Field("", GetValue<Vector3>(value)), tmp);

            case "Vector3Int":
                return SetValueGUI(EditorGUILayout.Vector3IntField("", GetValue<Vector3Int>(value)), tmp);

            case "Vector4":
                return SetValueGUI(EditorGUILayout.Vector4Field("", GetValue<Vector4>(value)), tmp);

            case "Quaternion":
                return SetValueGUI(EditorGUILayout.Vector4Field("", GetValue<Quaternion>(value).GetVector4()).GetQuaternion(), tmp);

            //All Integer Types
            case "Byte":
            case "Int16":
            case "Int32":
            case "SByte":
            case "UInt16":
            case "UInt32":
                return SetValueGUI(EditorGUILayout.IntField(GetValue<int>(value)), tmp);

            case "UInt64":
            case "Int64":
                return SetValueGUI(EditorGUILayout.LongField(GetValue<long>(value)), tmp);

                //
        }
        return new object();
    }

    public static Vector4 GetVector4(this Quaternion quaternion)
    {
        return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
    }

    public static Quaternion GetQuaternion(this Vector4 vector4)
    {
        return new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
    }
}