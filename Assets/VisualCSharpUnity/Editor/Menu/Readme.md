Usage:
Code (CSharp):
 ```csharp
public override void OnInspectorGUI() {
     ...
     var editorPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
     var templatePath = editorPath + "/ScriptTemplate/MyEquipmentBehaviour.cs.template";
     AddScriptWindow.Show(CreateScriptInstance, CanAddScript, templatePath);
     ...
}
 
void CreateScriptInstance(MonoScript script) {
     if (!script || script.GetClass() == null) {
         return;
     }
     //var instance = Activator.CreateInstance(script.GetClass()) as EquipmentBehaviour;
     var instance = ScriptableObject.CreateInstance(script.GetClass().Name) as EquipmentBehaviour;
     (target as Equipment).AddBehaviour(instance);
 
     AssetDatabase.AddObjectToAsset(instance, target);
     EditorUtility.SetDirty(target);
     return;
}
 
bool CanAddScript(MonoScript script) {
     var scriptClass = script.GetClass();
     if(scriptClass == null){
          return false;
     }
     return !scriptClass.IsAbstract && scriptClass.IsSubclassOf(typeof(EquipmentBehaviour));
}
```

We need the AddScriptWindowBackup class for saving the added script. So we can add them after the compiler has finished his work. The AddComponentWindow use a internal function and don't need to wait for the compiler.

And for you who don't know how to render the child objects like components:
Code (CSharp):

 ```csharp
public override void OnInspectorGUI() {
     ...
     foreach(var child in childComponents) {
          DrawEquipmentBehaviour(Editor.CreateEditor(child));      
     }
     ...
     EditorUtility.SetDirty(target);
}
 
void DrawEquipmentBehaviour(Editor editor) {
     if (!editor) {
         return;
     }
     var foldout = EditorGUILayout.InspectorTitlebar(EditorPrefs.GetBool(editor.target.GetType().Name, true), editor.target);
     EditorPrefs.SetBool(editor.target.GetType().Name, foldout);
     if (foldout) {
         editor.DrawDefaultInspector();
         editor.serializedObject.ApplyModifiedProperties();
     }
}
```
