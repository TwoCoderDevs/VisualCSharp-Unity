using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class SymboleCalls : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //Call<StartSymbole>("Start");
    }

    public void Call<T>(string name)
    {
        Type type = typeof(T);
        Assembly assembly = Assembly.GetAssembly(type);

        assembly
            .GetTypes()
            .Where(t =>
                t.GetMethod(name).Invoke(t,null) != null
                );
    }

    // Update is called once per frame
    void Update()
    {
        Call<UpdateSymbole>("DoIt");
    }
}
