using UnityEngine;
using System.Collections;

public class MathSymbole : Symbole
{
    [Input(0,1,0,1)] public double A;
    [Input(0, 1, 0, 1)] public double B;

    [Output(1, 0, 0, 1,ValueProp.Name)] public double Result;

    public MathType m_mathType = MathType.Add;
    public enum MathType { Divide, Multiply, Add, Subtract }

    public override void Init()
    {
        base.Init();
        NodeSize.width += 50;
    }

    public override object GetValue(ConnectionPoint point)
    {
        SymboleManager.DrawSymbole(GetInstanceID());
        double a = System.Convert.ToDouble(GetInputValue<object>("A"));
        double b = System.Convert.ToDouble(GetInputValue<object>("B"));
        // After you've gotten your input values, you can perform your calculations and return a value
        if (point.name == "Result")
            switch (m_mathType)
            {
                case MathType.Divide: if(b != 0) return a / b; return 0;
                case MathType.Multiply: return a * b;
                case MathType.Add: default: return a + b;
                case MathType.Subtract: return a - b;
            }
        else return base.GetValue(point);
    }
    public override string GetNameSpace()
    {
        return "using System;";
    }

    public override string ToString()
    {
        string symbole = "";
        switch (m_mathType)
        {
            case MathType.Divide: symbole = "/"; break;
            case MathType.Multiply: symbole = "*"; break;
            case MathType.Add: default: symbole = "+"; break;
            case MathType.Subtract: symbole = "-"; break;
        }
        string a = A.ToString();
        string b = B.ToString();
        string sa = "";
        string sb = "";
        string c = string.Format(@"var Result_{0} = {1} {2} {3};", (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), a, symbole, b);
        var pa = GetInputPoint("A");
        var pb = GetInputPoint("B");
        if (InputConnected("A"))
        {
            var id = pa.Connections[0].symbole.GetInstanceID();
            sa = pa.Connections[0].symbole.ToString();
            a = string.Format("{0}_{1}", pa.Connections[0].name, (id < 0) ? id * -1 : id);
            c = string.Format(@"{0},
    var Result_{1} = {2} {3} {4};",sa, (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), a, symbole, b);
        }
        if (InputConnected("B"))
        {
            var id = pb.Connections[0].symbole.GetInstanceID();
            sb = pb.Connections[0].symbole.ToString();
            b = string.Format("{0}_{1}", pb.Connections[0].name, (id < 0) ? id * -1 : id);
            c = string.Format(@"{0},
    var Result_{1} = {2} {3} {4};", sb, (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), a, symbole, b);
        }

        if (InputConnected("A") && InputConnected("B"))
        {
            c = string.Format(@"{0},
    {1}, 
    var Result_{2} = {3} {4} {5};", sa, sb, (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), a, symbole, b);
            if (sa.Contains(sb))
                c = string.Format(@"{0},
    var Result_{1} = {2} {3} {4};", sa, (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), a, symbole, b);
            if (sb.Contains(sa))
                c = string.Format(@"{0},
    var Result_{1} = {2} {3} {4};", sb, (GetInstanceID() < 0) ? GetInstanceID() * -1 : GetInstanceID(), a, symbole, b);
        }
        return c;
    }
}
