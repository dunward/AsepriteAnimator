using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AnimatorLoader : EditorWindow
    {
        private static AnimatorLoader windowInstance = null;
        private Texture spriteSheet;
        private TextAsset sheetJson;

        private AsepriteAlignment alignment;

        private string fileName;
        private string filePath;


        [MenuItem("Aseprite/Import Animation")]
        private static void Open()
        {
            if(windowInstance == null)
            {
                windowInstance = CreateInstance<AnimatorLoader>();
                windowInstance.titleContent.text = "Aseprite Importer";
                windowInstance.minSize = new Vector2(300, 400);
                windowInstance.maxSize = new Vector2(300, 400);
            }

            windowInstance.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            #region Logo
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("", Styler.Skin.GetStyle("aseprite logo"), GUILayout.Width(130), GUILayout.Height(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion
            GUILayout.Space(15);
            #region File Name
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("File Name", GUILayout.Width(58));
            fileName = EditorGUILayout.TextField(fileName, GUILayout.Width(122));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion
            #region Sprite Sheet
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Sprite Sheet", GUILayout.Width(72));
            // spriteSheet = Styler.DrawProObjectField<Texture>(null, (SerializedProperty)spriteSheet, typeof(Texture), Styler.Skin.GetStyle("button"), false);
            spriteSheet = (Texture)EditorGUILayout.ObjectField(spriteSheet, typeof(Texture), false, GUILayout.Width(108));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion
            #region Json File
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Json File", GUILayout.Width(72));
            sheetJson = (TextAsset)EditorGUILayout.ObjectField(sheetJson, typeof(TextAsset), false, GUILayout.Width(108));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion
            #region Alignment
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Alignment", GUILayout.Width(72));
            alignment = (AsepriteAlignment)EditorGUILayout.EnumPopup(alignment, GUILayout.Width(108));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion
            GUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Generator", Styler.Skin.GetStyle("button"), GUILayout.Width(200), GUILayout.Height(30)))
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
                        Debug.Log(clip.From + "," + spriteKeyFrames.Length);
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
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
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