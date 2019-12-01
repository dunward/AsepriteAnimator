using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AnimatorLoader : EditorWindow
    {
        private static AnimatorLoader windowInstance = null;
        private Sprite spriteSheet;
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
            spriteSheet = (Sprite)EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Sprite), false);
            sheetJson = (TextAsset)EditorGUILayout.ObjectField("Sheet Json", sheetJson, typeof(TextAsset), false);

            JToken token = JObject.Parse(sheetJson.text);

            int q = 0;

            List<string> test = new List<string>();

            foreach (var t in token["frames"])
            {
                test.Add(t);
                // test.Add(t.Value<string>().ToString());
                // Debug.Log(q.ToString() + " /// x : " + root["frame"]["x"].ToString() + ", y : " + root["frame"]["y"].ToString());
            }

            foreach(var t in test)
            {
                Debug.Log(t);
            }
        }
    }
}