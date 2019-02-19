using UnityEngine;
using UnityEditor;
[Callable]
[Receivable]
public class DebugLogSymbole : Symbole
{
    [Input(1, 1, 0, 1,ValueProp.Hide)] public object A;

    public DebugLogType m_debugLogType = DebugLogType.Log;
    public enum DebugLogType { Log, Multiply, Add, Subtract }

    public override void Init()
    {
        base.Init();
        NodeSize.width += 50;
    }

    public override void Update()
    {
        if (InputConnected("A"))
        {
            switch (m_debugLogType)
            {
                case DebugLogType.Log:
                    SymboleManager.DrawSymbole(GetInstanceID());
                    object a = GetInputValue<object>("A");
                    Debug.Log(a);
                    break;
            }
        }
    }
}