#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuDriver.cs" company="Ian Horswill">
// Copyright (C) 2019, 2020 Ian Horswill
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
