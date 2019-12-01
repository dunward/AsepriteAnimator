using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AsepriteData
    {
        private string name;

        private int x;
        private int y;
        private int w;
        private int h;

        private int duration;

        public string Name
        {
            get => name;
        }

        public int Duration
        {
            get => duration;
        }

        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }

        public Vector2 GetSize()
        {
            return new Vector2(w, h);
        }

        public AsepriteData(string name, int x, int y, int w, int h, int duration)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.duration = duration;
        }

        public override string ToString()
        {
            return $"name : {name}, x : {x}, y : {y}, w : {w}, h : {h}, duration(ms) {duration}";
        }
    }
}