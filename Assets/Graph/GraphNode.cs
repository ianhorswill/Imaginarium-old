
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    public Rigidbody2D rBody;
    private TextMesh textMesh;
    private CircleCollider2D hitBox;

    public Color Color = Color.white;

    private bool mouseDrag;

    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        rBody = gameObject.AddComponent<Rigidbody2D>();
        rBody.gravityScale = 0;
        rBody.drag = 0.75f;
        rBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.alignment = TextAlignment.Center;
        //textMesh.fontStyle = FontStyle.Bold;
        hitBox = gameObject.AddComponent<CircleCollider2D>();
        hitBox.radius = 3;
        textMesh.text = gameObject.name;
        textMesh.color = Color;
    }

    public void SetColor(Color c)
    {
        Color = c;
        if (textMesh != null)
            textMesh.color = c;
    }

    internal void OnMouseDown()
    {
        mouseDrag = true;
        Graph.SelectedNode = this;
    }

    internal void OnMouseUp()
    {
        mouseDrag = false;
        Graph.SelectedNode = null;
    }

    internal void OnMouseEnter()
    {
        Graph.SelectedNode = this;
    }

    internal void OnMouseExit()
    {
        Graph.SelectedNode = null;
    }

    public void FixedUpdate()
    {
        Graph.ConstrainToScreen(rBody);

        if (mouseDrag)
            rBody.MovePosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
}
