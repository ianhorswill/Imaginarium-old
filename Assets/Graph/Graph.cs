#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Graph.cs" company="Ian Horswill">
// Copyright (C) 2019 Ian Horswill
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Graph : MonoBehaviour
{
    public static Graph Singleton;
    public static GraphNode SelectedNode;

    readonly Dictionary<object, GraphNode> nodes = new Dictionary<object, GraphNode>();
    public Rect ScreenInWorldCoordinates;

    public static void ConstrainToScreen(Rigidbody2D r)
    {
        var screen = Singleton.ScreenInWorldCoordinates;
        var p = r.position;
        var changed = false;
        if (p.x > screen.xMax)
        {
            p.x = screen.xMax;
            changed = true;
        }

        if (p.x < screen.xMin)
        {
            p.x = screen.xMin;
            changed = true;
        }

        if (p.y > screen.yMax)
        {
            p.y = screen.yMax;
            changed = true;
        }

        if (p.y < screen.yMin)
        {
            p.y = screen.yMin;
            changed = true;
        }

        if (changed)
            r.MovePosition(p);
    }

    public static void Create()
    {
        if (Singleton != null)
            Singleton.Exit();
        var go = new GameObject("Graph visualization");
        Singleton = go.AddComponent<Graph>();
        var screenRect = FindObjectOfType<Driver>().GraphAreaRect;
        var lowerLeft = Camera.main.ScreenToWorldPoint(new Vector2(screenRect.xMin+50, 50));
        var upperRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width-50, Screen.height-50, 0));
        var difference = upperRight - lowerLeft;
        //var middle = lowerLeft + 0.5f * difference;
        Singleton.ScreenInWorldCoordinates = new Rect(lowerLeft, difference);
    }

    public static void AddNode(object id, string label)
    {
        Singleton.FindNode(id, label);
    }

    public static void SetColor(object id, string label, string color)
    {
        Singleton.FindNode(id, label).SetColor(ColorNamed(color));
    }

    private GraphNode FindNode(object id, string label)
    {
        if (nodes.ContainsKey(id))
            return nodes[id];

        var child = new GameObject(label);
        child.transform.parent = gameObject.transform;
        child.transform.position = new Vector3(
            Place(ScreenInWorldCoordinates.xMin, ScreenInWorldCoordinates.xMax), 
            Place(ScreenInWorldCoordinates.yMin, ScreenInWorldCoordinates.yMax)); 
        
        return nodes[id] = child.AddComponent<GraphNode>();
    }

    private float Place(float min, float max)
    {
        var count = 2;
        var sum = 0f;
        for (var i = 0; i < count; i++)
            sum += Random.Range(min, max);
        return sum / count;
    }

    public static void AddEdge(object from, string fromLabel, object to, string toLabel, string edgeLabel, string color)
    {
        Singleton.MakeEdge(from, fromLabel, to, toLabel, edgeLabel, ColorNamed(color));
    }

    static Color ColorNamed(string name)
    {
        switch (name)
        {
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "white": return Color.white;
            case "black": return Color.black;
            case "gray":
                case "grey":
                return Color.gray;
            case "cyan": return Color.cyan;
            case "magenta": return Color.magenta;
            case "yellow": return Color.yellow;
            default:
                throw new ArgumentException($"Unknown color {name}");
        }
    }

    private void MakeEdge(object from, string fromLabel, object to, string toLabel, string edgeLabel, Color c)
    {
        var child = new GameObject($"{from}->{to}");
        child.transform.parent = gameObject.transform;
        var edge = child.AddComponent<GraphEdge>();
        edge.StartNode = FindNode(from, fromLabel);
        edge.EndNode = FindNode(to, toLabel);
        edge.Label = edgeLabel;
        edge.Color = c;
    }

    // Start is called before the first frame update
    internal void Start()
    {
        FindObjectOfType<Draw>().Visible = false;
        Singleton = this;
    }

    internal void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            Exit();
    }

    private void Exit()
    {
        FindObjectOfType<Draw>().Visible = true;
        Singleton = null;
        Destroy(gameObject);
    }
}
