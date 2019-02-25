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
    public List<ConnectionCallPoint> connections;
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

    public void AddConnection(ConnectionCallPoint connectionCallPoint)
    {
        connections.Add(connectionCallPoint);
    }

    public void RemoveConnections()
    {
        if (connections != null)
        {
            foreach (var connection in connections)
            {
                connection.RemoveConnection(this);
            }
            connections.Clear();
        }
    }

    public bool RemoveConnection(ConnectionCallPoint connectionCallPoint)
    {
        return connections.Remove(connectionCallPoint);
    }

    public bool RemoveConnection(Symbole symbole)
    {
        if (this.symbole == symbole)
            return true;
        connections.Remove(symbole.Call);
        connections.Remove(symbole.Receive);
        return false;
    }

    public void Init(Symbole symbole, ConnectionType connectionType)
    {
        this.symbole = symbole;
        this.connectionType = connectionType;
        this.connections = new List<ConnectionCallPoint>();
        this.knobeColor = Color.green;
    }

    public void Function()
    {
        if (connectionType == ConnectionType.Call)
            foreach (var connection in connections)
                connection.Function();
        if (connectionType == ConnectionType.Receive)
        {
            symbole.Function();
            if (symbole.Call)
                symbole.Call.Function();
        }
    }

    public string FunctionBody()
    {
        string result = @"";
        if (connectionType == ConnectionType.Call)
            foreach (var connection in connections)
                result += connection.FunctionBody();
        if (connectionType == ConnectionType.Receive)
        {
            result = symbole.ToString();
            if (symbole.Call)
                result += "," + symbole.Call.FunctionBody();
        }
        return result;
    }

    // Use this for initialization
    public void Draw(Vector2 panOffset)
    {
        if (!noCon)
            noCon = LoadResources.GetTexture("DotCircle", knobeColor);
        if (!hasCon)
            hasCon = LoadResources.GetTexture("Dot", knobeColor);
        if (connectionType == ConnectionType.Receive)
            Point = new Rect((symbole.NodeSize.position.x - 8) + 2, symbole.NodeSize.y + 19, 10, 10);
        if (connectionType == ConnectionType.Call)
            Point = new Rect(((symbole.NodeSize.position.x - 8) + (symbole.NodeSize.width + 16)) - 13, symbole.NodeSize.y + 19, 10, 10);
        Point.position += panOffset;
        if (connections != null && connections.Count > 0)
            tex = hasCon;
        else
            tex = noCon;
        GUI.DrawTexture(Point, tex);
    }
}