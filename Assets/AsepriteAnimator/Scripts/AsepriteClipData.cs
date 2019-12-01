using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsepriteAnimator
{
    public class AsepriteClipData
    {
        private string name;
        private int from;
        private int to;

        public string Name
        {
            get => name;
        }

        public int From
        {
            get => from;
        }

        public int To
        {
            get => to;
        }

        public AsepriteClipData(string name, int from, int to)
        {
            this.name = name;
            this.from = from;
            this.to = to;
        }

        public override string ToString()
        {
            return $"name : {Name}, From : {From}, To : {To}";
        }
    }
}