Product : File Selector Package
Studio : Arkham Interactive
Date : September 17th, 2013
Version : 1.0
Email : support@arkhaminteractive.com

How To Use:
	0) Add FileSelectorStyles.cs to any object if you want to edit the File Selector's styles outside of code
	1) Call 'GetFile(SelectFileFunction callback, string extension)' from code to open the window
	2) Use callback function to use resulting path

 - Callback functions also have a 'status' parameter that indicate if the resulting path is valid