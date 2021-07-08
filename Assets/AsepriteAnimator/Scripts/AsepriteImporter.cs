using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

[UnityEditor.AssetImporters.ScriptedImporter(1, new string[] { "ase", "aseprite" })]
public class AsepriteImporter : UnityEditor.AssetImporters.ScriptedImporter
{
    public AnimatorController animator1;
    public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
    {
        var icon = Resources.Load<Texture2D>("aseprite-logo");

        ctx.AddObjectToAsset("Thumbnail", icon);
        ctx.SetMainObject(icon);

        var sprite = Sprite.Create(icon, new Rect(0, 0,0,0), Vector2.zero, 32, 0, SpriteMeshType.FullRect, Vector4.one, false);
        var animator = new Animator();
        ctx.AddObjectToAsset("coin", sprite);
        ctx.AddObjectToAsset("anim", animator1);
    }
}