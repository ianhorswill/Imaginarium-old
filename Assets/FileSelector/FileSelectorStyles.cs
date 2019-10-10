using JetBrains.Annotations;
using UnityEngine;

public class FileSelectorStyles : MonoBehaviour {
	
	public GUIStyle WindowStyle;
	public GUIStyle ButtonStyle;
	public GUIStyle TitleStyle;
	public GUIStyle LabelStyle;
	public GUIStyle TextFieldStyle;
	
	[UsedImplicitly]
    void Start () 
	{
		FileSelector.WindowStyle = WindowStyle;
		FileSelector.ButtonStyle = ButtonStyle;
		FileSelector.TitleStyle = TitleStyle;
		FileSelector.LabelStyle = LabelStyle;
		FileSelector.TextFieldStyle = TextFieldStyle;
	}
}
