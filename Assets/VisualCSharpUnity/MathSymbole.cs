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
}
