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
            GUILayout.FlexibleSpace();
            #region Logo
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
            #region Generate
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", Styler.Skin.GetStyle("button"), GUILayout.Width(200), GUILayout.Height(30)))
            {
                if(fileName == null || fileName.Equals(string.Empty) || spriteSheet == null || sheetJson == null)
                {
                    Debug.Log("Need to fill the fields");
                    return;
                }

                JToken token = JObject.Parse(sheetJson.text);

                var aseprites = GetAsepriteData(token["frames"]);
                var clips = GetAsepriteClipData(token["meta"]["frameTags"]);

                Sprite[] sprites = GetSplitSprites(aseprites);
                SaveAnimation(aseprites, clips, sprites);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion
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

        private Sprite[] GetSplitSprites(List<AsepriteData> aseprites)
        {
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteSheet)) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            var meta = aseprites.Select(aseprite => new SpriteMetaData()
            {
                border = Vector4.zero, rect = new Rect(aseprite.GetPosition(), aseprite.GetSize()),
                name = aseprite.Name, alignment = (int) alignment
            }).ToList();

            importer.isReadable = true;
            importer.spritesheet = meta.ToArray();
            importer.SaveAndReimport();

            return AssetDatabase.LoadAllAssetRepresentationsAtPath(importer.assetPath).Select(x => x as Sprite).Where(x => x != null).ToArray();
        }

        private (AnimationClip, EditorCurveBinding) CreateSpriteAnimationClip()
        {
            AnimationClip clip = new AnimationClip();
            EditorCurveBinding curve = new EditorCurveBinding();
            curve.type = typeof(SpriteRenderer);
            curve.path = "";
            curve.propertyName = "m_Sprite";

            return (clip, curve);
        }

        private void SaveAnimation(List<AsepriteData> aseprites, List<AsepriteClipData> clipsDatas, Sprite[] sprites)
        {
            AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPath($"Assets/{fileName}.controller");

            foreach (var clips in clipsDatas)
            {
                var (clip, curve) = CreateSpriteAnimationClip();

                int length = clips.To - clips.From - 1;

                ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[length];

                float totalDuration = 0;

                for (int i = clips.From; i < spriteKeyFrames.Length + clips.From; i++)
                {
                    spriteKeyFrames[i - clips.From] = new ObjectReferenceKeyframe();
                    spriteKeyFrames[i - clips.From].time = totalDuration;
                    spriteKeyFrames[i - clips.From].value = sprites[i];
                    totalDuration += aseprites[i].Duration / 1000f;
                }

                AnimationUtility.SetObjectReferenceCurve(clip, curve, spriteKeyFrames);
                AssetDatabase.CreateAsset(clip, $"Assets/{clips.Name}.anim");
                animator.AddMotion(clip);
            }
        }
    }
}