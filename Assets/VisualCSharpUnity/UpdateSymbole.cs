using UnityEngine;
using System.Collections;
[Callable]
[MethodSymbole]
public class UpdateSymbole : Symbole
{
    public override void Init()
    {
        NodeSize.width += 25;
        NodeSize.height -= 160;
        base.Init();
    }
    
    public override void CallerFunction()
    {
        Call.Function();
        base.CallerFunction();
    }

    public override string GetNameSpace()
    {
        return "using UnityEngine;";
    }

    public override string ToString()
    {
        return string.Format(@"public void Update (){{,
    {0},
}}", Call.FunctionBody());
    }
}