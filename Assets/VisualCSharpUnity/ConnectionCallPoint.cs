using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class ConnectionCallPoint : ScriptableObject
{
    public Color knobeColor;
    public Symbole symbole;
    public Rect Point;
    public ConnectionType connectionType = ConnectionType.Call;
    public ConnectionCallPoint connection;
    public Texture2D tex;
    public Texture2D hasCon;
    public Texture2D noCon;

    public void OnEnable()
    {
        if (name.Contains("(Clone)"))
        {
            var i = name.IndexOf("(Clone)");
            name = name.Remove(i);
        }
    }

    public void RemoveConnection()
    {
        if(connection)
        {
            connection.connection = null;
            connection = null;
        }
    }

    public void Init(Symbole symbole, ConnectionType connectionType)
    {
        this.symbole = symbole;
        this.connectionType = connectionType;
        this.knobeColor = Color.green;
    }

    // Use this for initialization
    public void Draw(Vector2 panOffset)
    {
        if (!noCon)
            noCon = LoadResources.GetTexture("DotCircle", knobeColor);
        if (!hasCon)
            hasCon = LoadResources.GetTexture("Dot", knobeColor);
        if (connectionType == ConnectionType.Call)
            Point = new Rect((symbole.NodeSize.position.x - 8) + 2, symbole.NodeSize.y + 19, 10, 10);
        if (connectionType == ConnectionType.Receive)
            Point = new Rect(((symbole.NodeSize.position.x - 8) + (symbole.NodeSize.width + 16)) - 13, symbole.NodeSize.y + 19, 10, 10);
        Point.position += panOffset;
        if (connection)
            tex = hasCon;
        else
            tex = noCon;
        GUI.DrawTexture(Point, tex);
    }
}