using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[UnityEditor.AssetImporters.ScriptedImporter(1, new string[] { "ase", "aseprite" })]
public class AsepriteImporter : UnityEditor.AssetImporters.ScriptedImporter
{
    public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
    {
        Debug.LogError("X");
        
        var icon = Resources.Load<Texture2D>("aseprite-logo");

        ctx.AddObjectToAsset("Icon", icon);
        ctx.SetMainObject(icon);
    }
}