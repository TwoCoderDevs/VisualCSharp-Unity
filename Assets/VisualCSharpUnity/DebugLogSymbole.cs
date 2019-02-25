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
        NodeSize.width += 50;
        base.Init();
    }

    public override void Function()
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

    public override string GetNameSpace()
    {
        return "using UnityEngine;";
    }

    public override string ToString()
    {
        if (InputConnected("A"))
        {
            var point = GetInputPoint("A");
            var Id = point.Connections[0].symbole.GetInstanceID();
            return string.Format(@"{0},
    Debug.Log({1}_{2});", point.Connections[0].symbole.ToString(), point.Connections[0].name, (Id < 0)? Id * -1 : Id);
        }

        return "Debug.Log(\"\");";
    }
}