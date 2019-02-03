using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class EventManager
{
    public SymboleManager symboleManager;
    public GenericMenu menu;
    public Vector2 mousePosition;
    public bool isDragged;
    public bool isPaned;
    private FlowchartEditorWindow few;
    public Rect viewSize
    {
        get
        {
            var size = few.editorSize;
            size.y = 18f;
            return size;
        }
    }
    public Action<Vector2> OnPan;
    public EventManager(SymboleManager symboleManager, FlowchartEditorWindow few, Action<Vector2> OnPan)
    {
        this.symboleManager = symboleManager;
        this.few = few;
        this.OnPan = OnPan;
        SetupSymboleMenu();
    }

    public void SetupSymboleMenu()
    {
        menu = new GenericMenu();
        Type derivedType = typeof(Symbole);
        Assembly assembly = Assembly.GetAssembly(derivedType);

        List<Type> nodeTypes = assembly
            .GetTypes()
            .Where(t =>
                t != derivedType &&
                derivedType.IsAssignableFrom(t)
                ).ToList();

        //Populate canvasContext with entries for all node types

        for (int i = 0; i < nodeTypes.Count; i++)
        {
            Type nodeType = nodeTypes[i];
            string name = ObjectNames.NicifyVariableName(nodeType.Name);

            menu.AddItem(new GUIContent(name), false, () => symboleManager.AddNewSymbole(nodeType, mousePosition));
        }
    }

    public void ProcessEvent(Event @event)
    {
        bool overS_P = false;
        if ( @event.type == EventType.MouseDown && @event.button == 0 && @event.mousePosition.y > 18)
            overS_P = symboleManager.OnMouseOverPoint_Symbole(@event.mousePosition);
        if (symboleManager.selectedOutputPoint)
            overS_P = OnPointSelected(symboleManager.selectedOutputPoint);
        if (symboleManager.selectedSymbole)
            overS_P = OnNodeSelected(symboleManager.selectedSymbole);
        if (@event.type == EventType.MouseDown && @event.button == 1 && @event.mousePosition.y > 18)
            overS_P = symboleManager.RemoveSymbole(@event.mousePosition);
        if (!overS_P && !symboleManager.selectedOutputPoint)
            switch (@event.type)
            {
                case EventType.MouseDown:
                    if (@event.button == 1)
                    {
                        if (viewSize.Contains(@event.mousePosition))
                        {
                            symboleManager.selectedSymbole = null;
                            mousePosition = @event.mousePosition;
                            menu.ShowAsContext();
                        }
                    }

                    if (@event.button == 0)
                    {
                        if (viewSize.Contains(@event.mousePosition))
                        {
                            symboleManager.selectedSymbole = null;
                            isPaned = true;
                            GUI.changed = true;
                        }
                        else
                        {
                            GUI.changed = true;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    isPaned = false;
                    break;

                case EventType.MouseDrag:
                    if (Event.current.button == 0 && isPaned)
                    {
                        var delta = @event.delta;
                        OnPan(delta);
                        symboleManager.PanSymbole(delta);
                        @event.Use();
                    }
                    break;
            }
    }

    

    public bool OnNodeSelected(Symbole symbole)
    {
        var e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (symbole.NodeSize.Contains(e.mousePosition) && e.mousePosition.y > 18 && !symboleManager.selectedOutputPoint)
                    {
                        isDragged = true;
                        GUI.changed = true;
                    }
                    else
                    {
                        symboleManager.selectedSymbole = null;
                        GUI.changed = true;
                        return false;
                    }
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    symbole.NodeSize = symboleManager.DragSymbole(symbole.NodeSize);
                    e.Use();
                }
                break;
        }
        if (symbole.NodeSize.Contains(e.mousePosition) && e.mousePosition.y > 18 && !symboleManager.selectedOutputPoint)
            return true;
        return false;
    }

    public bool OnPointSelected(ConnectionPoint point)
    {
        var e = Event.current;

        symboleManager.OnMouseOverInputPoint(e.mousePosition);

        switch (e.type)
        {
            case EventType.MouseUp:
                if (symboleManager.selectedInputPoint)
                    if (point.symbole != symboleManager.selectedInputPoint.symbole && symboleManager.selectedInputPoint.PointPos.Contains(e.mousePosition))
                    {
                        point.AddConnection(symboleManager.selectedInputPoint);
                        symboleManager.selectedInputPoint.AddConnection(point);
                    }
                symboleManager.selectedInputPoint = null;
                symboleManager.selectedOutputPoint = null;
                break;
        }
        if (point.PointPos.Contains(e.mousePosition) && e.mousePosition.y > 18)
            return true;
        return false;
    }
}