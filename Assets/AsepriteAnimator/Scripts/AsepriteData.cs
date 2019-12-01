using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AsepriteData
    {
        private int x;
        private int y;
        private int w;
        private int h;

        private string name;

        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }

        public Vector2 GetSize()
        {
            return new Vector2(w, h);
        }

        public AsepriteData(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        public override string ToString()
        {
            return $"x : {x}, y : {y}, w : {w}, h : {h}";
        }
    }
}