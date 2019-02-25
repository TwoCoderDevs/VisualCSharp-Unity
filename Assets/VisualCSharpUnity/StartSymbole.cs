using UnityEngine;
using UnityEditor;
[Callable]
[MethodSymbole]
public partial class StartSymbole : Symbole 
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
        return string.Format(@"public void Start (){{,
    {0},
}}", Call.FunctionBody());
    }
}