using UnityEngine;
using UnityEngine.EventSystems;

public class SceneSwitchButton : MonoBehaviour, IPointerClickHandler
{
    public string SceneName;

    public void OnPointerClick(PointerEventData eventData)
    {
        Scenes.SwitchTo(SceneName);
    }
}
