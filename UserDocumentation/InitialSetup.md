Here is the workflow you can use to start your new game without breaking the sample:

Create a New Unity Scene: File > New Scene.
Initialize the UI architecture: Use the custom menu command you built: FMV > Initialize Sample Scene. This will instantly populate your new scene with the correct Canvas, text objects, and FMVManager.
Create New Game Data: In your Assets/Resources/FMV/ folder, create a new text file for your actual game (e.g., MyGameScenes.txt). You can start scripting your new nodes here!
Update the Manager Settings: Select the FMV Manager GameObject in your new scene. In the inspector, change the exposed settings to point to your new game:
Text Assets Path: Change from FMV/Scenes to FMV/MyGameScenes (or whatever you named it).
Initial Scene Id: Change this to match the starting node in your new text file (e.g., intro_01).
Video Assets Path (Optional): If you want to keep your final game videos separate from the template ones, you can create a new folder under Resources (like Resources/ActualGameVideos) and update this path to match.
If you run into architectural issues or want to add new features (like an inventory UI or a new type of condition) as you build your game, you can simply open the template Unity Scene to test those core code changes in isolation before using them in your real game.