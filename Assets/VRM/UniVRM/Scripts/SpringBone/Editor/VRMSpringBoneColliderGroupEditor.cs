using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMSpringBoneColliderGroup))]
    public class VRMSpringBoneColliderGroupEditor : Editor
    {
        VRMSpringBoneColliderGroup m_target;

        private void OnEnable()
        {
            m_target = (VRMSpringBoneColliderGroup)target;
        }

        private void OnSceneGUI()
        {
            Undo.RecordObject(m_target, "VRMSpringBoneColliderGroupEditor");

            Handles.matrix = m_target.transform.localToWorldMatrix;
            Gizmos.color = Color.green;

            bool changed = false;

            foreach (var x in m_target.Colliders)
            {
                var offset = Handles.PositionHandle(x.Offset, Quaternion.identity);
                if (offset != x.Offset)
                {
                    changed = true;
                    x.Offset = offset;
                }
            }

            foreach (var x in m_target.CapsuleColliders)
            {
                var offset = Handles.PositionHandle(x.OffsetStart, Quaternion.identity);
                if (offset != x.OffsetStart)
                {
                    changed = true;
                    x.OffsetStart = offset;
                }

                offset = Handles.PositionHandle(x.OffsetEnd, Quaternion.identity);
                if (offset != x.OffsetEnd)
                {
                    changed = true;
                    x.OffsetEnd = offset;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(m_target);
            }
        }
    }
}
