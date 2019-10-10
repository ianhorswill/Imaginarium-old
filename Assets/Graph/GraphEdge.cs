#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphEdge.cs" company="Ian Horswill">
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

using UnityEngine;

public class GraphEdge : MonoBehaviour
{
    public GraphNode StartNode;
    public GraphNode EndNode;

    public bool IsSelfEdge => StartNode == EndNode;

    public string Label;
    public float Length = 25;
    public Color Color = Color.green;
    private TextMesh labelText;

    public static float Gain = 0.2f;

    public static Material Shader;

    public void Start()
    {
        labelText = gameObject.AddComponent<TextMesh>();
        labelText.text = Label;
        labelText.alignment = TextAlignment.Center;
        labelText.color = Color;
        if (Shader == null)
            Shader = new Material(UnityEngine.Shader.Find("GUI/Text Shader"));
    }

    public void Update()
    {
        var startPos = StartNode.transform.position;
        var endPos = EndNode.transform.position;
        var newPosition = 0.5f * (startPos + endPos);
        if (IsSelfEdge)
            newPosition.y += 3*labelText.lineSpacing;
        //transform.position = newPosition;
        var offset = endPos - startPos;
        var newRotation = Mathf.Atan2(offset.y, offset.x);
        //if (newRotation<0)
        //    newRotation = -newRotation;
        transform.SetPositionAndRotation(newPosition, Quaternion.Euler(0, 0 , Radians2Degrees(newRotation)));
        labelText.color = ProperColor;
    }

    float Radians2Degrees(float rad)
    {
        return rad * 180 / Mathf.PI;
    }

    public void FixedUpdate()
    {
        // Points from start to end
        var offset = EndNode.transform.position - StartNode.transform.position;
        var distance = offset.magnitude;
        // Positive means too close together, so push farther apart; zero means right distance
        var distanceError = Length - distance;
        // If distanceError positive, this pushes nodes apart.
        var forceVector = (distanceError * Gain) * offset.normalized;
        StartNode.RBody.AddForce(-forceVector);
        EndNode.RBody.AddForce(forceVector);
    }

    private Color ProperColor
    {
        get
        {
            var s = Graph.SelectedNode;
            return (s == null || StartNode == s || EndNode == s) ? Color : 0.5f * Color;
        }
    }

    public void OnRenderObject()
    {
        if (IsSelfEdge)
            RenderSelfEdge();
        else
            RenderNonSelfEdge();
    }

    private void RenderNonSelfEdge()
    {
        var startPos = StartNode.transform.position;
        var endPos = EndNode.transform.position;
        var unit = (endPos - startPos).normalized;
        // ReSharper disable once IdentifierTypo
        var perp = 0.2f * new Vector3(unit.y, -unit.x, 0);
        var corner1 = endPos - unit + perp;
        var corner2 = endPos - unit - perp;

        // Very inefficient
        Shader.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(ProperColor);
        // Body of arrow
        GL.Vertex(startPos);
        GL.Vertex(endPos);
        GL.End();

        GL.Begin(GL.TRIANGLES);
        GL.Color(ProperColor);
        // Arrowhead
        GL.Vertex(corner1);
        GL.Vertex(endPos);

        GL.Vertex(corner2);
        GL.End();
    }

    private void RenderSelfEdge()
    {
        var nodePos = StartNode.transform.position;
        var radius = labelText.lineSpacing * 0.8f;
        var center = nodePos + new Vector3(0, radius);
        
        var lineCount = 10;
        Vector3 CircleVertex(int n)
        {
            var theta = n * 2*Mathf.PI / lineCount;
            return center + new Vector3(radius * Mathf.Sin(theta), radius * Mathf.Cos(theta), 0);
        }

        // Very inefficient
        Shader.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(ProperColor);
        // Body of arrow
        for (int i = 0; i < lineCount; i++)
        {
            var circleVertex = CircleVertex(i);
            GL.Vertex(circleVertex);
            GL.Vertex(CircleVertex(i+1));
        }
        GL.End();

        var endPos = nodePos;
        var unit = Vector3.left;
        // ReSharper disable once IdentifierTypo
        var perp = 0.2f * new Vector3(unit.y, -unit.x, 0);
        var corner1 = endPos - unit + perp;
        var corner2 = endPos - unit - perp;
        GL.Begin(GL.TRIANGLES);
        GL.Color(ProperColor);
        // Arrowhead
        GL.Vertex(corner1);
        GL.Vertex(endPos);

        GL.Vertex(corner2);
        GL.End();
    }
}
