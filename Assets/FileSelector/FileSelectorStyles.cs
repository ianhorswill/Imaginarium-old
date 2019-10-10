using UnityEngine;
using System.Collections;

public class FileSelectorStyles : MonoBehaviour {
	
	public GUIStyle windowStyle;
	public GUIStyle buttonStyle;
	public GUIStyle titleStyle;
	public GUIStyle labelStyle;
	public GUIStyle textFieldStyle;
	
	void Start () 
	{
		FileSelector.windowStyle = windowStyle;
		FileSelector.buttonStyle = buttonStyle;
		FileSelector.titleStyle = titleStyle;
		FileSelector.labelStyle = labelStyle;
		FileSelector.textFieldStyle = textFieldStyle;
	}
}
