using System;
using UnityEngine;


namespace VRM
{
    #if UNITY_5_5_OR_NEWER
    [DefaultExecutionOrder(11001)]
    #endif
    public class VRMSpringBoneColliderGroup : MonoBehaviour
    {
        [Serializable]
        public class SphereCollider
        {
            public Vector3 Offset;

            [Range(0, 1.0f)]
            public float Radius;
        }

        [SerializeField]
        public SphereCollider[] Colliders = new SphereCollider[]{
            new SphereCollider
            {
                Radius=0.1f
            }
        };

        [Serializable]
        public class CapsuleCollider
        {
            public Vector3 OffsetStart;
            public Vector3 OffsetEnd;

            [Range(0, 1.0f)]
            public float Radius;
        }

        [SerializeField]
        public CapsuleCollider[] CapsuleColliders = new CapsuleCollider[]{
            new CapsuleCollider
            {
                Radius=0.1f
            }
        };

        [SerializeField]
        Color m_gizmoColor = Color.magenta;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = m_gizmoColor;
            Matrix4x4 mat = transform.localToWorldMatrix;
            Gizmos.matrix = mat * Matrix4x4.Scale(new Vector3(
                1.0f / transform.lossyScale.x,
                1.0f / transform.lossyScale.y,
                1.0f / transform.lossyScale.z
                ));
            foreach (var y in Colliders)
            {
                Gizmos.DrawWireSphere(y.Offset, y.Radius);
            }

            foreach (var y in CapsuleColliders)
            {
                Gizmos.DrawWireSphere(y.OffsetStart, y.Radius);
                Gizmos.DrawWireSphere(y.OffsetEnd, y.Radius);

                var offsets = new Vector3[]
                {
                    new Vector3(1.0f, 0.0f, .0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector3(0.0f, -1.0f, 0.0f)
                };
                for (int i = 0; i < offsets.Length; i++) {
                    Gizmos.DrawLine(y.OffsetStart + offsets[i] * y.Radius, y.OffsetEnd + offsets[i] * y.Radius);
                }
            }
        }
    }
}
