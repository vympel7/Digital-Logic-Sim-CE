using System;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Assets.Scripts.Graphics
{
    using Scripts.Chip;

    public class DisplayScreen : BuiltinChip
    {
        public Renderer TextureRender;
        public const int SIZE = 8;
        private string _editCoords;
        private Texture2D _texture;
        private int[] _texCoords;

        public static Texture2D CreateSolidTexture2D(Color color, int width, int height = -1)
        {
            if (height == -1)
            {
                height = width;
            }
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        public int[] map2d(int index, int size)
        {
            int[] coords = new int[2];
            coords[0] = index % size;
            coords[1] = index / size;
            return coords;
        }

        protected override void Awake()
        {
            _texture = CreateSolidTexture2D(new Color(0, 0, 0), SIZE);
            _texture.filterMode = FilterMode.Point;
            _texture.wrapMode = TextureWrapMode.Clamp;
            TextureRender.sharedMaterial.mainTexture = _texture;
            base.Awake();
        }

        //update display here
        protected override void ProcessOutput()
        {
            _editCoords = "";
            for (int i = 6; i < 12; i++)
            {
                _editCoords += InputPins[i].State.ToString();
            }
            _texCoords = map2d(Convert.ToInt32(_editCoords, 2), SIZE);
            _texture.SetPixel(_texCoords[0], _texCoords[1], new Color(Convert.ToInt32(InputPins[0].State.ToString() + InputPins[1].State.ToString(), 2) / 2f, Convert.ToInt32(InputPins[2].State.ToString() + InputPins[3].State.ToString(), 2) / 2f, Convert.ToInt32(InputPins[4].State.ToString() + InputPins[5].State.ToString(), 2)) / 2f);
            _texture.Apply();
        }
    }
}