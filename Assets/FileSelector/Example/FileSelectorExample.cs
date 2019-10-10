using UnityEngine;
using System.Collections;

public class FileSelectorExample : MonoBehaviour {
	
	private GUIStyle style;
	private string path = "";
	private bool windowOpen;
	
	void Start()
	{
		style = new GUIStyle();
		style.fontSize = 40;
		
		style.normal.textColor = Color.white;
	}
	
	void OnGUI(){
		//Instructions
		GUI.Label(new Rect(10, 10, 1000, 1000), "Press [spacebar] to open File Selection Window.", style);
		GUI.Label(new Rect(10, 60, 1000, 1000), "Path : "+path, style);
	}
	
	// Update is called once per frame
	void Update () {
		
		//if we don't have an open window yet, and the spacebar is down...
		if(!windowOpen && Input.GetKeyDown(KeyCode.Space))
		{
			FileSelector.GetFile(GotFile, ".txt"); //generate a new FileSelector window
			windowOpen = true; //record that we have a window open
		}
	}
				
	//This is called when the FileSelector window closes for any reason.
	//'Status' is an enumeration that tells us why the window closed and if 'path' is valid.
	void GotFile(FileSelector.Status status, string path){
		Debug.Log("File Status : "+status+", Path : "+path);
		this.path = path;
		this.windowOpen = false;
	}
}
