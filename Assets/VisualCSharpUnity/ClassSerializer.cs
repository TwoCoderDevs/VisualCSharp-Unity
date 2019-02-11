using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace System.ObjectSerializer
{
    public static class ClassSerializer
    {
        public class SerializedClass : object
        {
            public string name = "";
            public Dictionary<string, List<SerializedClass>> classArrayFields = new Dictionary<string, List<SerializedClass>>();
            public Dictionary<string, SerializedClass> classFields = new Dictionary<string, SerializedClass>();
            public Dictionary<string, List<object>> arrayFields = new Dictionary<string, List<object>>();
            public Dictionary<string, object> Fields = new Dictionary<string, object>();
            public SerializedClass(string name)
            {
                this.name = name;
            }

            public List<List<string>> Serialize()
            {
                List<List<string>> Objects = new List<List<string>>();
                var str = new List<string>();
                Objects.Add(str);
                str.Add(string.Format("class {0}",name));
                str.Add(name);
                foreach (var cafs in classArrayFields)
                {
                    str.Add(string.Format("public ArrayField {0}", cafs.Key));
                    Objects.Add(new List<string>{ string.Format("Array Field {0}", cafs.Key)});
                    foreach (var caf in cafs.Value)
                    {
                        var outscd = caf.Serialize();
                        foreach (var outcs in outscd)
                        {
                            Objects.Add(outcs);
                        }
                    }
                    Objects.Add(new List<string> { string.Format("End Array Field {0}", cafs.Key) });
                }
                foreach (var cf in classFields)
                {

                    str.Add(string.Format("public ArrayField {0}", cf.Key));
                    Objects.Add(new List<string> { string.Format("Array Field {0}", cf.Key) });
                    var outscd = cf.Value.Serialize();
                    foreach (var outcs in outscd)
                    {
                        Objects.Add(outcs);
                    }
                    Objects.Add(new List<string> { string.Format("End Array Field {0}", cf.Key) });
                }
                foreach (var afs in arrayFields)
                {
                    str.Add(string.Format("public ArrayField {0}", afs.Key));
                    Objects.Add(new List<string> { string.Format("Array Field {0}", afs.Key) });
                    var i = 0;
                    foreach (var af in afs.Value)
                    {
                        Objects.Add(new List<string>{string.Format("Element {0} {1}", i, afs.Key) });
                        i++;
                    }
                    Objects.Add(new List<string> { string.Format("End Array Field {0}", afs.Key) });
                }
                foreach (var f in Fields)
                {
                    str.Add(string.Format("public Field {0} = {1}", f.Key, f.Value));
                }
                str.Add(string.Format("End class {0}", name));
                return Objects;
            }
        }



        /*public static void SerializeClass(string pathToSave, object serializableClass)
        {
            Type classType = serializableClass.GetType();
            var fields = classType.GetFields();
            Lines.Add(string.Format("public class {0} {{", classType.Name));
            bool isObject = true;
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute(typeof(ClassField)) != null)
                {
                        var value = field.GetValue(fieldsClass);
                        if ((value.)
                        {
                            if (insert < Lines.Count - 1)
                            {
                                Lines.Insert(insert, string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value));
                            }
                            else
                            {
                                Lines.Add(string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value));
                            }
                        }
                        else
                        {
                            if (insert < Lines.Count - 1)
                            {
                                Lines.Insert(insert, string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value.GetHashCode()));
                            }
                            else
                            {
                                Lines.Add(string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value.GetHashCode()));
                            }
                        }
                    isObject = false;
                }
                if (field.GetCustomAttribute(typeof(ClassArrayField)) != null)
                {
                    insert++;
                    ActionsMaped.Add(() =>
                    {
                        if (insert < Lines.Count - 1)
                            Lines.Insert(insert, string.Format("public {0} {1} = new {0}{{", field.GetType().Name, field.Name));
                        else
                            Lines.Add(string.Format("public {0} {1} = new {0}{{", field.GetType().Name, field.Name));
                        var values = (IList)field.GetValue(fieldsClass);
                        if (values != null)
                            for (int i = 0; i < values.Count; i++)
                            {
                                insert++;
                                if (!ReadFields(values[i], values[i].GetType()))
                                    Lines.Insert(insert, string.Format("Element {0} = {1};", i, values[i]));
                                else
                                    Lines.Insert(insert, string.Format("Element {0} = {1};", i, values[i].GetHashCode()));
                            }
                        insert++;
                        if (insert < Lines.Count - 1)
                            Lines.Insert(insert, string.Format("}}; End Field {0}", field.Name));
                        else
                            Lines.Add(string.Format("}}; End Field {0}", field.Name));
                    });
                    isObject = false;
                }
            }
            if (isObject)
            {
                Lines.Remove(Lines.Last());
            }
            else
            {
                Lines.Add(string.Format("}} End class {0}", classType.Name));
            }
        }*/

        public static void CreateSerializedClass(string pathToSave, object serializableClass)
        {
            Lines = new List<string>();
            ActionsMaped = new List<Action>();
            if (!ReadFields(serializableClass, serializableClass.GetType()))
                throw new System.Exception(string.Format("{0} class is Not Serializable Class", serializableClass.GetType().Name));
            else
            {
                for (int i = 0; i < ActionsMaped.Count; i++)
                    ActionsMaped[i]?.Invoke();
                File.WriteAllLines(pathToSave, Lines);
            }
            Lines.Clear();
            Lines = null;
            ActionsMaped.Clear();
            ActionsMaped = null;
        }
        private static List<string> Lines;
        private static List<object> Classes;
        private static List<Action> ActionsMaped;

        private static bool ReadFields(object fieldsClass, Type classType)
        {
            var fields = classType.GetFields();
            Lines.Add(string.Format("public class {0} => {1} {{", classType.Name, fieldsClass.GetHashCode()));
            var insert = Lines.Count;
            bool isObject = true;
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute(typeof(FieldAttribute)) != null)
                {
                    insert++;
                    ActionsMaped.Add(() =>
                    {
                        var value = field.GetValue(fieldsClass);
                        if (!ReadFields(value, value.GetType()))
                        {
                            if (insert < Lines.Count - 1)
                            {
                                Lines.Insert(insert, string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value));
                            }
                            else
                            {
                                Lines.Add(string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value));
                            }
                        }
                        else
                        {
                            if (insert < Lines.Count - 1)
                            {
                                Lines.Insert(insert, string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value.GetHashCode()));
                            }
                            else
                            {
                                Lines.Add(string.Format("public {0} {1} = {2};", value.GetType().Name, field.Name, value.GetHashCode()));
                            }
                        }
                    });
                    isObject = false;
                }
                if (field.GetCustomAttribute(typeof(ArrayFieldAttribute)) != null)
                {
                    insert++;
                    ActionsMaped.Add(() =>
                    {
                        if (insert < Lines.Count-1)
                        Lines.Insert(insert, string.Format("public {0} {1} = new {0}{{", field.GetType().Name, field.Name));
                    else
                        Lines.Add(string.Format("public {0} {1} = new {0}{{", field.GetType().Name, field.Name));
                    var values = (IList)field.GetValue(fieldsClass);
                    if (values != null)
                        for (int i = 0; i < values.Count; i++)
                        {
                            insert++;
                            if (!ReadFields(values[i], values[i].GetType()))
                                Lines.Insert(insert, string.Format("Element {0} = {1};", i, values[i]));
                            else
                                Lines.Insert(insert, string.Format("Element {0} = {1};", i, values[i].GetHashCode()));
                        }
                    insert++;
                    if (insert < Lines.Count - 1)
                        Lines.Insert(insert, string.Format("}}; End Field {0}", field.Name));
                    else
                        Lines.Add(string.Format("}}; End Field {0}", field.Name));
                    });
                    isObject = false;
                }
            }
            if (isObject)
            {
                Lines.Remove(Lines.Last());
                return false;
            }
            Lines.Add(string.Format("}} End class {0}", classType.Name));
            return true;
        }

        public static void AddToFile()
        {

        }
    }

    public class FieldAttribute : Attribute
    {
        public FieldAttribute()
        {

        }
    }

    public class ClassFieldAttribute : Attribute
    {
        public ClassFieldAttribute()
        {

        }
    }

    public class ArrayFieldAttribute : Attribute
    {
        public ArrayFieldAttribute()
        {

        }
    }

    public class ClassArrayFieldAttribute : Attribute
    {
        public ClassArrayFieldAttribute(object master)
        {

        }
    }
}
