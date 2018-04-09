﻿using UnityEditor;
using UnityEngine;
using UnityEditorInternal;


namespace VRM
{
    /// <summary>
    /// Prefabをインスタンス化してPreviewに表示する
    /// 
    /// * https://github.com/Unity-Technologies/UnityCsReference/blob/11bcfd801fccd2a52b09bb6fd636c1ddcc9f1705/Editor/Mono/Inspector/ModelInspector.cs
    /// 
    /// </summary>
    public class PreviewEditor : Editor
    {
        /// <summary>
        /// PreviewRenderUtilityを管理する。
        /// 
        /// * PreviewRenderUtility.m_cameraのUnityVersionによる切り分け
        /// 
        /// </summary>
        PreviewFaceRenderer m_renderer;

        /// <summary>
        /// Prefabをインスタンス化したシーンを管理する。
        /// 
        /// * BlendShapeのBake
        /// * MaterialMorphの適用
        /// * Previewカメラのコントロール
        /// * Previewライティングのコントロール
        /// 
        /// </summary>
        PreviewSceneManager m_scene;
        protected PreviewSceneManager PreviewSceneManager
        {
            get { return m_scene; }
        }

        /// <summary>
        /// Previewシーンに表示するPrefab
        /// </summary>
        GameObject m_prefab;
        GameObject Prefab
        {
            get { return m_prefab; }
            set
            {
                if (m_prefab == value) return;
                m_prefab = value;

                if (m_scene != null)
                {
                    //Debug.LogFormat("OnDestroy");
                    GameObject.DestroyImmediate(m_scene.gameObject);
                    m_scene = null;
                }

                if (m_prefab != null)
                {
                    m_scene = VRM.PreviewSceneManager.GetOrCreate(m_prefab);
                    if (m_scene != null)
                    {
                        m_scene.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// シーンにBlendShapeとMaterialMorphを適用する
        /// </summary>
        /// <param name="values"></param>
        /// <param name="materialValues"></param>
        /// <param name="weight"></param>
        protected void Bake(BlendShapeBinding[] values, MaterialValueBinding[] materialValues, float weight)
        {
            if (m_scene != null)
            {
                m_scene.Bake(values, materialValues, weight);
            }
        }

        protected virtual void OnEnable()
        {
            m_renderer = new PreviewFaceRenderer();
            var assetPath = AssetDatabase.GetAssetPath(target);
            //Debug.LogFormat("assetPath: {0}", assetPath);
            if (!string.IsNullOrEmpty(assetPath))
            {
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }
        }

        protected virtual void OnDisable()
        {
            if (m_renderer != null)
            {
                m_renderer.Dispose();
                m_renderer = null;
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_scene != null)
            {
                //Debug.LogFormat("OnDestroy");
                m_scene.Clean();
                GameObject.DestroyImmediate(m_scene.gameObject);
                m_scene = null;
            }
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            Prefab = (GameObject)EditorGUILayout.ObjectField("prefab", Prefab, typeof(GameObject), false);
        }

        private static int sliderHash = "Slider".GetHashCode();
        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlId = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && (double)position.width > 50.0)
                    {
                        GUIUtility.hotControl = controlId;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        break;
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                        GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        scrollPosition -= current.delta * (!current.shift ? 1f : 3f) / Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                        break;
                    }
                    break;
            }
            return scrollPosition;
        }

        Vector2 previewDir;

        // very important to override this, it tells Unity to render an ObjectPreview at the bottom of the inspector
        public override bool HasPreviewGUI() { return true; }

        // the main ObjectPreview function... it's called constantly, like other IMGUI On*GUI() functions
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            // if this is happening, you have bigger problems
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), 
                        "Mesh preview requires\nrender texture support");
                }
                return;
            }

            previewDir = Drag2D(previewDir, r);
            //Debug.LogFormat("{0}", previewDir);

            if (Event.current.type != EventType.Repaint)
            {
                // if we don't need to update yet, then don't
                return;
            }

            if (m_renderer != null && m_scene != null)
            {
                var texture = m_renderer.Render(r, background, m_scene, previewDir);
                if (texture != null)
                {
                    // draw the RenderTexture in the ObjectPreview pane
                    GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, false);
                }
            }
        }
    }
}
