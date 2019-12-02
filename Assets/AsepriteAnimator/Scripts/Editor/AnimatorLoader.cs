using System;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.VersionControl;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AnimatorLoader : EditorWindow
    {
        private static AnimatorLoader windowInstance = null;
        private Texture spriteSheet;
        private TextAsset sheetJson;

        private string fileName;
        private string filePath;

        [MenuItem("Aseprite/Import Animation")]
        private static void Open()
        {
            if(windowInstance == null)
            {
                windowInstance = CreateInstance<AnimatorLoader>();
                windowInstance.titleContent.text = "Aseprite Importer";
                windowInstance.minSize = new Vector2(500, 300);
                GUI.skin = Resources.Load<GUISkin>("AsepriteAnimator");
            }

            windowInstance.Show();
        }

        private void OnGUI()
        {
            DisplayGUI();

            if (GUILayout.Button("Generator"))
            {
                JToken token = JObject.Parse(sheetJson.text);

                var aseprites = GetAsepriteData(token["frames"]);
                var clips = GetAsepriteClipData(token["meta"]["frameTags"]);

                #region Sprite Split
                TextureImporter ti = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteSheet)) as TextureImporter;
                ti.spriteImportMode = SpriteImportMode.Multiple;

                List<SpriteMetaData> meta = new List<SpriteMetaData>();



                foreach (var t in aseprites)
                {
                    meta.Add(new SpriteMetaData()
                    {
                        border = Vector4.zero,
                        rect = new Rect(t.GetPosition(), t.GetSize()),
                        name = t.Name,
                        alignment = 0
                    });
                }

                ti.isReadable = true;
                ti.spritesheet = meta.ToArray();
                ti.SaveAndReimport();
                #endregion

                Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(ti.assetPath).Select(x => x as Sprite).Where(x => x != null).ToArray();

                foreach (var sprite in sprites)
                {
                    Debug.Log(sprite.name);
                }

                AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPath($"Assets/{fileName}.controller");

                foreach (var clip in clips)
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
                        totalDuration += aseprites[i].Duration / 1000f; // millie seconds change
                        spriteKeyFrames[i - clip.From].value = sprites[i];
                        Debug.Log($"{clip.Name} : {sprites[i].name}");
                    }

                    AnimationUtility.SetObjectReferenceCurve(c, spriteBinding, spriteKeyFrames);
                    AssetDatabase.CreateAsset(c, $"Assets/{clip.Name}.anim");
                    animator.AddMotion(c);
                }
            }
        }

        private void DisplayGUI()
        {
            EditorGUILayout.BeginHorizontal();
            fileName = EditorGUILayout.TextField("File Name", fileName);
            if (GUILayout.Button("path", GUILayout.Width(40)))
            {
                filePath = EditorUtility.OpenFolderPanel("Animation Save Folder", "", ""); 
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Sprite Sheet");
            spriteSheet = (Texture)EditorGUILayout.ObjectField(spriteSheet, typeof(Texture), false);
            EditorGUILayout.EndHorizontal();
            sheetJson = (TextAsset)EditorGUILayout.ObjectField("Animation Json", sheetJson, typeof(TextAsset), false);
        }

        private List<AsepriteData> GetAsepriteData(JToken token)
        {
            return token.ToObject<JObject>().Properties().Select(property => new AsepriteData(property.Name,
                int.Parse(property.Value["frame"]["x"].ToString()),
                int.Parse(property.Value["frame"]["y"].ToString()),
                int.Parse(property.Value["frame"]["w"].ToString()),
                int.Parse(property.Value["frame"]["h"].ToString()),
                int.Parse(property.Value["duration"].ToString()))).ToList();
        }

        private List<AsepriteClipData> GetAsepriteClipData(JToken token)
        {
            return token.Select(data => new AsepriteClipData(data["name"].ToString(),
                int.Parse(data["from"].ToString()),
                int.Parse(data["to"].ToString()))).ToList();
        }
    }
}