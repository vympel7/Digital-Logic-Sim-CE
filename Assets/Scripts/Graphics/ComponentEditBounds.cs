using UnityEngine;

namespace Assets.Scripts.Graphics
{
    [ExecuteInEditMode]
    public class ComponentEditBounds : MonoBehaviour
    {
        public float Thickness = 0.1f;
        public Material Material;
        public Transform InputSignalArea;
        public Transform OutputSignalArea;

        private Mesh _quadMesh;
        private Matrix4x4[] _trs;

        private void Start()
        {
            if (Application.isPlaying)
            {
                MeshShapeCreator.CreateQuadMesh(ref _quadMesh);
                CreateMatrices();
            }
        }

        private void UpdateSignalAreaSizeAndPos(Transform signalArea)
        {
            signalArea.position = new Vector3(signalArea.position.x, transform.position.y, signalArea.position.z);
            signalArea.localScale = new Vector3(signalArea.localScale.x, transform.localScale.y, 1);
        }

        private void CreateMatrices()
        {
            Vector3 centre = transform.position;
            float width = Mathf.Abs(transform.localScale.x);
            float height = Mathf.Abs(transform.localScale.y);

            Vector3[] edgeCentres = {
                centre + Vector3.left * width / 2,
                centre + Vector3.right * width / 2,
                centre + Vector3.up * height / 2,
                centre + Vector3.down * height / 2
            };

            Vector3[] edgeScales = {
                new Vector3 (Thickness, height + Thickness, 1),
                new Vector3 (Thickness, height + Thickness, 1),
                new Vector3 (width + Thickness, Thickness, 1),
                new Vector3 (width + Thickness, Thickness, 1)
            };

            _trs = new Matrix4x4[4];
            for (int i = 0; i < 4; i++)
            {
                _trs[i] = Matrix4x4.TRS(edgeCentres[i], Quaternion.identity, edgeScales[i]);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                MeshShapeCreator.CreateQuadMesh(ref _quadMesh);
                CreateMatrices();
                UpdateSignalAreaSizeAndPos(InputSignalArea);
                UpdateSignalAreaSizeAndPos(OutputSignalArea);
            }

            for (int i = 0; i < 4; i++)
            {
                UnityEngine.Graphics.DrawMesh(_quadMesh, _trs[i], Material, 0);
            }
        }
    }
}