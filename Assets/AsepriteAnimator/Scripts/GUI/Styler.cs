using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Styler
{
    private static GUISkin skin;

    public static GUISkin Skin
    {
        get
        {
            if (skin == null)
            {
                skin = GetUiStyle();
            }

            return skin;
        }
    }

    public static void DrawProObjectField<T>(
        GUIContent label,
        SerializedProperty value,
        Type objType,
        GUIStyle style,
        bool allowSceneObjects,
        Texture objIcon = null,
        params GUILayoutOption[] options) where T : UnityEngine.Object
    {

        T tObj = value.objectReferenceValue as T;

        if (objIcon == null)
        {
            objIcon = EditorGUIUtility.FindTexture("PrefabNormal Icon");
        }
        style.imagePosition = ImagePosition.ImageLeft;

        int pickerID = 455454425;

        if (tObj != null)
        {
            EditorGUILayout.LabelField(label,
                new GUIContent(tObj.name, objIcon), style, options);
        }

        if (GUILayout.Button("Select"))
        {
            EditorGUIUtility.ShowObjectPicker<T>(
                tObj, allowSceneObjects, "", pickerID);

        }
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            if (EditorGUIUtility.GetObjectPickerControlID() == pickerID)
            {
                tObj = EditorGUIUtility.GetObjectPickerObject() as T;
                value.objectReferenceValue = tObj;
            }
        }

    }

    private static GUISkin GetUiStyle()
    {
        var searchRootAssetFolder = Application.dataPath;
        var paths = Directory.GetFiles(searchRootAssetFolder, "AsepriteAnimator.guiskin", SearchOption.AllDirectories);
        foreach (var eachPath in paths)
        {
            var loadPath = eachPath.Substring(eachPath.LastIndexOf("Assets"));
            return (GUISkin)AssetDatabase.LoadAssetAtPath(loadPath, typeof(GUISkin));
        }
        return null;
    }
}