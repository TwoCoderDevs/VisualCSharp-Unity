using UnityEngine;
using System.Collections;

public class MathSymbole : Symbole
{
    [Input(0,1,0,1)] public int A;
    [Input(0, 1, 0, 1)] public int B;

    [Output(1, 0.96f, 0.016f, 1,ValueProp.Hide)] public int Result;

    public MathType m_mathType = MathType.Add;
    public enum MathType { Divide, Multiply, Add, Subtract }

    public override void Init()
    {
        base.Init();
        NodeSize.width += 50;
    }

    public override object GetValue(ConnectionPoint point)
    {
        int a = GetInputValue<int>("A");
        int b = GetInputValue<int>("B");
        // After you've gotten your input values, you can perform your calculations and return a value
        if (point.name == "Result")
            switch (m_mathType)
            {
                case MathType.Divide: return a / b;
                case MathType.Multiply: return a * b;
                case MathType.Add: default: return a + b;
                case MathType.Subtract: return a - b;
            }
        else return 0;
    }
}
