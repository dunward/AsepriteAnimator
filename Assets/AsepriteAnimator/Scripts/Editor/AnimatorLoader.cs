using System;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using UnityEditor;
using UnityEditor.VersionControl;
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

                #region AsepriteData Generator
                List<AsepriteData> test = new List<AsepriteData>();
                List<AsepriteClipData> clipList = new List<AsepriteClipData>();

                foreach (var t in token["frames"])
                {
                    string name = t.Path.Split('\'')[1];

                    test.Add(new AsepriteData(name,
                                              int.Parse(t.Last["frame"]["x"].ToString()),
                                              int.Parse(t.Last["frame"]["y"].ToString()),
                                              int.Parse(t.Last["frame"]["w"].ToString()),
                                              int.Parse(t.Last["frame"]["h"].ToString()),
                                              int.Parse(t.Last["duration"].ToString())));
                }

                foreach (var t in token["meta"]["frameTags"])
                {
                    clipList.Add(new AsepriteClipData(
                        t["name"].ToString(),
                        int.Parse(t["from"].ToString()),
                        int.Parse(t["to"].ToString())
                        ));
                }

                foreach (var t in test)
                {
                    Debug.Log(t);
                }

                foreach (var t in clipList)
                {
                    Debug.Log(t);
                }

                #endregion
                #region Sprite Split
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
                        name = t.Name,
                        alignment = 0
                    });

                    oko++;
                }

                ti.isReadable = true;
                ti.spritesheet = meta.ToArray();
                ti.SaveAndReimport();
                #endregion

                // Sprite[] sprites = ;
                Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(ti.assetPath).Select(x => x as Sprite).Where(x => x != null).ToArray();

                foreach (var sprite in sprites)
                {
                    Debug.Log(sprite.name);
                }

                // AsepriteData <- Need more
                // [meta/frameTags] <- frame data

                //////////////////////////////////////////
                // 1. load sprite[]                     // 88 line
                // 2. insert sprite, duration to clip   //
                // 3. insert clip to animator           //
                // 4. save clip / animator binary files //
                //////////////////////////////////////////
                
                // change -> first. parse json about frame info

                Animator animator = new Animator();

                foreach (var clip in clipList)
                {
                    AnimationClip c = new AnimationClip();
                    EditorCurveBinding spriteBinding = new EditorCurveBinding();
                    spriteBinding.type = typeof(SpriteRenderer);
                    spriteBinding.path = "";
                    spriteBinding.propertyName = "m_Sprite";

                    int length = clip.To - clip.From - 1;

                    ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[length];

                    float totalDuration = 0;

                    for (int i = clip.From; i < spriteKeyFrames.Length + clip.From; i++)
                    {
                        Debug.Log(clip.From + ","+ spriteKeyFrames.Length);
                        spriteKeyFrames[i - clip.From] = new ObjectReferenceKeyframe();
                        spriteKeyFrames[i - clip.From].time = totalDuration;
                        totalDuration += test[i].Duration / 1000f; // millie seconds change
                        spriteKeyFrames[i - clip.From].value = sprites[i];
                        Debug.Log($"{clip.Name} : {sprites[i].name}");
                    }

                    AnimationUtility.SetObjectReferenceCurve(c, spriteBinding, spriteKeyFrames);
                    AssetDatabase.CreateAsset(c, $"Assets/{clip.Name}.anim");
                    Console.WriteLine("!");
                }



                // clip.SetCurve(AssetDatabase.GetAssetPath(sprites[0]), typeof(SpriteRenderer), sprites[0].name, null);

            }
        }
    }
}