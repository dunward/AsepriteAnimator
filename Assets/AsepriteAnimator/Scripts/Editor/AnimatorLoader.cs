using System;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using UnityEditor;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AnimatorLoader : EditorWindow
    {
        private static AnimatorLoader windowInstance = null;
        private Texture spriteSheet;
        private TextAsset sheetJson;

        [MenuItem("Aseprite/Load Animation")]
        private static void Open()
        {
            if(windowInstance == null)
            {
                windowInstance = CreateInstance<AnimatorLoader>();
                windowInstance.minSize = new Vector2(500, 300);
            }

            windowInstance.Show();
        }

        private void OnGUI()
        {
            spriteSheet = (Texture)EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Texture), false);
            sheetJson = (TextAsset)EditorGUILayout.ObjectField("Sheet Json", sheetJson, typeof(TextAsset), false);

            if (GUILayout.Button("Generator"))
            {
                JToken token = JObject.Parse(sheetJson.text);

                int q = 0;

                List<AsepriteData> test = new List<AsepriteData>();

                foreach (var t in token["frames"])
                {
                    test.Add(new AsepriteData(int.Parse(t.Last["frame"]["x"].ToString()),
                                              int.Parse(t.Last["frame"]["y"].ToString()),
                                              int.Parse(t.Last["frame"]["w"].ToString()),
                                              int.Parse(t.Last["frame"]["h"].ToString())));
                }

                TextureImporter ti = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteSheet)) as TextureImporter;
                ti.spriteImportMode = SpriteImportMode.Multiple;

                List<SpriteMetaData> meta = new List<SpriteMetaData>();

                int oko = 0;

                foreach (var t in test)
                {
                    meta.Add(new SpriteMetaData()
                    {
                        border = Vector4.zero,
                        rect = new Rect(t.GetPosition(), t.GetSize()),
                        name = $"{oko} sprite.png",
                        alignment = 0
                    });

                    oko++;
                }

                ti.isReadable = true;
                ti.spritesheet = meta.ToArray();
                ti.SaveAndReimport();
            }
        }
    }
}