Asset Hunter

Are you trying to go below a certain buildsize?
Are all the old placeholder graphics and models cluttering up your project?
Didn't you ever get around to delete the auto generated materials after model import?
Do you have large amount of sounds in your project, but is only using a small number of them?
Does it seem like too large a task to remove obsolete assets?

If any of the above applies to you, Asset Hunter is the tool for you

Asset Hunter is a tool that analyzes your buildlog and gives you an easily understandable overview over unused asset in your project folder. The results are grouped into folders and file types, making it easy to start cleaning up your project.

Additionally it list the uncompressed size of all the assets used in the build, enabling you to better downsize your buildsize.

How to open the window:
To open the Asset Hunter window, goto "Window->Asset Hunter" or press ctrl+h

How to use the tool:
1. First you need to make a build after you opened your project
2. Open the window
3. Press the yellow "refresh" button
4. Now select to either view "Unused Assets" or "Build Report"


Excluded from search since these are special case folders and filetypes:

The following folders are excluded
/plugins/
/streamingassets/
/resources/
/editor default resources/
/editor/

The following filetypes are excluded
.meta
.unity
thumbs.db
.orig
.ds_store
.workspace.mel
.mayaswatches

Version notes:
1.0.1 
- Changed minimum unity version

1.0.2 
-Reduced file sizes of PDFs and images

1.1.0 
-Created workaround for Unity Resource leak
-No longer serializes larger depth
-UI modifications
-Takes the new buildoutput from 4.6b20 under consideration

1.1.1 
-Added "Collapse" and "Expand" buttons
-Fixed rare issue with Out Of Memory when traversing used assets

1.2.0
-Added a way to find all the scenes a given asset exists in
-Added nullreference protection while parsing the editor log
-Removed delegate logic since they could not be serialized
-Automatically detects empty folders and removes then after having deleted assets

1.2.1
-Improved the way asset/scene dependencies are visualized

1.3.0
-Added settings to allow for manually excluding types and folders (i.e. exclude Third Party Editor Extensions)
-Removed absolute path reliance. (You can now move the Heureka Folder where you want to)

1.3.1
-Now recognizes icons and Win8 certificates (Not splash since it cannot be reached through the API)

1.3.2
-Made sure it compiles on Windows Store builds

1.3.3
-Fixed issue when user have own editor script called PlayerSettings
-Ready for Unity 5x