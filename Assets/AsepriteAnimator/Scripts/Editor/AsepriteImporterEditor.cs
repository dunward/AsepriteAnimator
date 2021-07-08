using System.Collections;
using System.Collections.Generic;
using UnityEditor;
 
using UnityEngine;

[CustomEditor(typeof(AsepriteImporter))]
public class AsepriteImporterEditor : UnityEditor.AssetImporters.ScriptedImporterEditor
{
    public override bool showImportedObject => false;
     
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("TEST");
        base.OnInspectorGUI();
    }
}