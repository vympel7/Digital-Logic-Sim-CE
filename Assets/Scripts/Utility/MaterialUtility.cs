using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class MaterialUtility
    {
        public static Material CreateUnlitMaterial(Color col)
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = col;
            return mat;
        }

    }
}