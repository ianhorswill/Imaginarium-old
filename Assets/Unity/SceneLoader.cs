using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// It seems unimaginably stupid that Unity doesn't give me a way of doing this in the stock editor
/// </summary>
public class SceneLoader : MonoBehaviour, IPointerClickHandler
{
    public string SceneName;

    public void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadSceneAsync(SceneName);
    }
}
