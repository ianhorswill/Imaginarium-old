#region UsingStatements

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

#endregion

/// <summary>
/// 	- Allows the user to select a file with the option to only allow certain file types.
/// 	- The window will be drawn with designated styles, or use defaults stored in GUI.skin.
/// 	- Use static 'GetFile' method to open self-drawing FileSelector window.
/// 	- Instances created this way will destroy on close by default. This behaviour can be changed using the 'destroyOneClose' variable.
/// </summary>
public class FileSelector : MonoBehaviour 
{	
	#region PublicEnumerations
	
	/// <summary>
	/// 	- Describes whether or not the file selection was successful.
	/// </summary>
	public enum Status
	{
		Successful, //Used if we successfully got a file
		Cancelled, //Used if the 'Cancel' button is clicked
		Failed, //Used if the Close() method is called while the window is open
		Destroyed, //Used if the instance is destroyed while the window is open
	}
	
	#endregion
	
	#region PublicFields
	
	/// <summary>
	/// 	- What file type we are searching for.
	/// </summary>
	public string Extension = ".*";
	
	/// <summary>
	/// 	- The function to be called when the window closes.
	/// </summary>
	public SelectFileFunction Callback;
	
	/// <summary>
	/// 	- Gets or sets a value indicating whether the window is open.
	/// </summary>
	public bool IsOpen { get; private set; }
	
	/// <summary>
	/// 	- If set to true, the window will be centered every OnGUI call.
	/// </summary>
	public bool Center = true;
	
	/// <summary>
	/// 	- If set to true, this instance will destroy itself when the window closes.
	/// </summary>
	public bool DestroyOnClose;
	
	/// <summary>
	/// 	- The window dimensions.
	/// </summary>
	public Rect WindowDimensions = new Rect(0,0,600,600);
	
	#endregion
	
	#region PublicStaticParameters
	
	/// <summary>
	/// 	- The window style.
	/// </summary>
	private static GUIStyle _windowStyle;
	public static GUIStyle WindowStyle
	{
		get => _windowStyle ?? (_windowStyle = GUI.skin.window);
        set => _windowStyle = value ?? GUI.skin.window;
    }
	
	/// <summary>
	/// 	- The style for buttons in the window.
	/// </summary>
	private static GUIStyle _buttonStyle;
	public static GUIStyle ButtonStyle
	{
		get => _buttonStyle ?? (_buttonStyle = GUI.skin.button);
        set => _buttonStyle = value ?? GUI.skin.button;
    }
	
	/// <summary>
	/// 	- The style for labels in the window.
	/// </summary>
	private static GUIStyle _labelStyle;
	public static GUIStyle LabelStyle
	{
		get => _labelStyle ?? (_labelStyle = GUI.skin.label);
        set => _labelStyle = value ?? GUI.skin.label;
    }
	
	/// <summary>
	/// 	- The style for titles in the window.
	/// </summary>
	private static GUIStyle _titleStyle;
	public static GUIStyle TitleStyle
	{
		get => _titleStyle ?? (_titleStyle = GUI.skin.label);
        set => _titleStyle = value ?? GUI.skin.label;
    }
	
	/// <summary>
	/// 	- The style for text fields in the window.
	/// </summary>
	private static GUIStyle _textFieldStyle;
	public static GUIStyle TextFieldStyle
	{
		get => _textFieldStyle ?? (_textFieldStyle = GUI.skin.textField);
        set => _textFieldStyle = value ?? GUI.skin.textField;
    }

	#endregion
	
	#region PublicDelegates
	
	/// <summary>
	/// 	- Delegate for the Callback function.
	/// </summary>
	public delegate void SelectFileFunction(Status status, string path);
	
	#endregion
	
	#region PrivateFields
	
	private string path = "";
	private string file = "";
	
	private int selection;
	private Vector2 scrollPosition;
	
	#endregion
	
	#region PrivateStaticFields
		
	private static GameObject updater;
	
	#endregion

	#region MonoBehaviourFunctions
	
	[UsedImplicitly]
    private void OnGUI()
	{	
		if(IsOpen)
		{
			if(Center) WindowDimensions.center = new Vector2(Screen.width*0.5f, Screen.height*0.5f);
			GUI.Window(0, WindowDimensions, DrawFileSelector, "Select a "+Extension+" File");
		}
	}
	
	[UsedImplicitly]
    private void OnDestroy()
	{
		if(IsOpen) Callback?.Invoke(Status.Destroyed, "");
	}
	
	#endregion
	
	#region PublicFunctions
	
	/// <summary>
	/// 	- Opens this instance.
	/// </summary>
	public void Open()
	{
		IsOpen = true;
	}
	
	/// <summary>
	/// 	- Opens this instance to the specified startingDirectory.
	/// </summary>
	/// <param name='startingDirectory'>
	/// 	- The directory to start in.
	/// </param>
	public void Open(string startingDirectory)
	{
		path = startingDirectory;
		IsOpen = true;
	}
	
	/// <summary>
	/// 	- Closes this instance.
	/// </summary>
	public void Close()
	{
		if(IsOpen) Callback?.Invoke(Status.Failed, "");
		IsOpen = false;
		if(DestroyOnClose) Destroy(this);
	}
	
	#endregion
	
	#region PublicStaticFunctions
	
	/// <summary>
	/// 	- Gets a file with a specified extension.
	/// </summary>
	/// <param name='startingDirectory'>
	/// 	- The directory to start the search in.
	/// </param>
	/// <param name='callback'>
	/// 	- The function to call when the file has been selected.
	/// </param>
	/// <param name='extension'>
	/// 	- The extension of the desired file type.
	/// </param>
	public static void GetFile(string startingDirectory, SelectFileFunction callback = null, string extension = ".*")
	{
		if(updater == null) 
		{
			updater = new GameObject("Select File");
			updater.AddComponent<FsCleanup>();
			updater.hideFlags = HideFlags.HideInHierarchy;
		}
		
		FileSelector instance = updater.AddComponent<FileSelector>();
		
		instance.Callback = callback;
		instance.Extension = extension;
		instance.path = startingDirectory;
		instance.DestroyOnClose = true;
		instance.IsOpen = true;
	}
	
	/// <summary>
	/// 	- Gets a file with a specified extension.
	/// </summary>
	/// <param name='callback'>
	/// 	- The function to call when the file has been selected.
	/// </param>
	/// <param name='extension'>
	/// 	- The extension of the desired file type.
	/// </param>
	public static void GetFile(SelectFileFunction callback = null, string extension = ".*")
	{
		GetFile(Application.dataPath, callback, extension);
	}
	
	#endregion
	
	#region PrivateFunctions
	
	private void DrawFileSelector(int id)
	{
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		//Path Buttons
		GUILayout.Label("Path : ", TitleStyle);
		
		GUILayout.BeginHorizontal();
		
			string[] parentDirectories = GetParentDirectories(path);
			
			float maximumWidth = WindowDimensions.width - 30;
			float totalWidth = 0;
			float width;
			float spacingWidth = 11; //public variable?
			float arrowWidth = LabelStyle.CalcSize(new GUIContent(" > ")).x;
			float arrowSpacing = arrowWidth + spacingWidth;
		
			for(int i = 0; i < parentDirectories.Length; i++){
				width = ButtonStyle.CalcSize(new GUIContent(parentDirectories[i])).x;
				
				totalWidth += (width + spacingWidth);
				if(totalWidth > maximumWidth)
				{
					totalWidth = width;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
				
				if(GUILayout.Button(parentDirectories[i], ButtonStyle, GUILayout.Width(width)))
				{
					path = GetParentDirectories(path, true)[i];
					break;
				}
	
				GUILayout.Label(" > ", LabelStyle, GUILayout.Width(arrowWidth));
				totalWidth += (arrowSpacing);
			
				if(totalWidth > maximumWidth)
				{
					totalWidth = 0;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}
			
			string currentDirectory = new FileInfo(path).Name;
			width = ButtonStyle.CalcSize(new GUIContent(currentDirectory)).x;
			GUILayout.Label(currentDirectory, ButtonStyle, GUILayout.Width(width));
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(1);
		
		//Directory Buttons
		GUILayout.Label("Directories : ", TitleStyle);
		
		GUILayout.BeginHorizontal();
		
			string[] childDirectories = GetChildDirectories(path);
			float buttonWidth = (WindowDimensions.width - 80) / 4f;
			
			for(int i = 0; i < childDirectories.Length; i++){
				if(GUILayout.Button(childDirectories[i], ButtonStyle, GUILayout.Width(buttonWidth)))
				{
					path = GetChildDirectories(path, true)[i];
					break;
				}
				
				GUILayout.Space(10);
				
				if((i % 4) == 3)
				{
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(1);
		
		//File Buttons
		GUILayout.Label("Files : ", TitleStyle);
		
		GUILayout.BeginHorizontal();
		
			string[] files = GetFiles(path, extension : Extension);
			
			for(int i = 0; i < files.Length; i++){
				if(GUILayout.Button(files[i], ButtonStyle, GUILayout.Width(buttonWidth)))
				{
					file = files[i];
					break;
				}
				
				GUILayout.Space(10);
				
				if((i % 4) == 3)
				{
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(1);
		
		//Returning values
		GUILayout.BeginHorizontal();
		
			GUILayout.Label("Selected File : ", TitleStyle, GUILayout.Width(TitleStyle.CalcSize(new GUIContent("Selected File : ")).x));
			file = GUILayout.TextField(file);
		
		GUILayout.EndHorizontal();
		
		if(File.Exists(path+@"\"+file) && Path.GetExtension(path+@"\"+file) == Extension)
		{
			if(GUILayout.Button("Select"))
			{
                Callback?.Invoke(Status.Successful, path+@"\"+file);
                IsOpen = false;
				
				if(DestroyOnClose) Destroy(this);
			}
		}
		
		if(GUILayout.Button("Cancel"))
		{
            Callback?.Invoke(Status.Cancelled, "");
            IsOpen = false;
		
			if(DestroyOnClose) Destroy(this);
		}
		
		GUILayout.EndScrollView();
	}
	
	#endregion
	
	#region PrivateStaticFunctions
	
	private static string[] GetParentDirectories(string filePath, bool includePaths = false)
	{
		List<string> parents = new List<string>();

        while(true){
			try{
				var fileInfo = new FileInfo(filePath);
                // ReSharper disable PossibleNullReferenceException
                parents.Add(!includePaths ? fileInfo.Directory.Name : fileInfo.Directory.FullName);
                // ReSharper restore PossibleNullReferenceException

				filePath = fileInfo.Directory.FullName;
			}
			
			catch{ break; }
		}
		
		parents.Reverse();
		return parents.ToArray();		
	}
	
	private static string[] GetChildDirectories(string directoryPath, bool includePaths = false)
	{
		DirectoryInfo directory;
		
		if(Directory.Exists(directoryPath)) directory = new DirectoryInfo(directoryPath);
		else
		{
			try{ directory = new FileInfo(directoryPath).Directory;	}
			catch{ return new string[0]; }
		}
		
		List<string> children = new List<string>();
		
		try{
            if (directory != null)
            {
                DirectoryInfo[] directories = directory.GetDirectories();
			
                foreach(DirectoryInfo childDir in directories)
                {
                    children.Add(!includePaths ? childDir.Name : childDir.FullName);
                }
            }
        }
		
		catch{
			children = new List<string>();
		}
		
		return children.ToArray();
	}
	
	private static string[] GetFiles(string directoryPath, bool includePaths = false, string extension = ".*")
	{
		DirectoryInfo directory;
		
		if(Directory.Exists(directoryPath)) directory = new DirectoryInfo(directoryPath);
		else
		{
			try{ directory = new FileInfo(directoryPath).Directory;	}
			catch{ return new string[0]; }
		}
		
		List<string> files = new List<string>();
		
		try{
            // ReSharper disable once PossibleNullReferenceException
            FileInfo[] fileInfos = directory.GetFiles("*"+extension);
			
			foreach(FileInfo fileInfo in fileInfos)
            {
                files.Add(!includePaths ? fileInfo.Name : fileInfo.FullName);
            }
		}
		
		catch{
			files = new List<string>();
		}
		
		return files.ToArray();
	}
	
	#endregion
}

/// <summary>
/// 	- A small class used to clean up the updater object when we change scenes or the application quits.
/// </summary>
public class FsCleanup : MonoBehaviour
{
	[UsedImplicitly]
    void OnApplicationQuit()
	{
		Destroy(gameObject);
	}
	
	[UsedImplicitly]
    void OnLevelLoaded()
	{
		Destroy(gameObject);
	}
}