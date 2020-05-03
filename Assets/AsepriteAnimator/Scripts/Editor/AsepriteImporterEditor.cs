using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(AsepriteImporter))]
public class AsepriteImporterEditor : ScriptedImporterEditor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("TEST");
        base.OnInspectorGUI();
    }
}