using UnityEngine;
using UnityEngine.UI;

public class MenuDriver : MonoBehaviour
{
    public GameObject ButtonPrefab;

    private Transform buttons;

    private GameObject selections;

    public void Start()
    {
        FindChildren();
        foreach (Transform child in selections.transform)
            AddChild(child.gameObject);
    }

    private void FindChildren()
    {
        if (buttons != null)
            return;
        buttons = transform.Find("Buttons");
        selections = transform.Find("Selections").gameObject;
    }

    public void OnEnable()
    {
        FindChildren();
        buttons.gameObject.SetActive(true);
        selections.SetActive(false);
    }

    private void AddChild(GameObject childGameObject)
    {
        var go = Instantiate(ButtonPrefab, buttons);
        go.GetComponent<Button>().onClick.AddListener(() => Select(childGameObject));
        go.transform.Find("Text").GetComponent<Text>().text = childGameObject.name;
    }

    private void Select(GameObject child)
    {
        buttons.gameObject.SetActive(false);
        selections.SetActive(true);
        foreach (GameObject c in selections.transform)
            c.SetActive(c == child);
    }
}
