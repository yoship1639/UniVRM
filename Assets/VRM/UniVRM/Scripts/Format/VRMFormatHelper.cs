using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    public static class VRMFormatHelper
    {
        public static void Apply(this VRM.glTF_VRM_Humanoid humanoid,
            UniHumanoid.AvatarDescription desc, List<Transform> nodes)
        {
            humanoid.armStretch = desc.armStretch;
            humanoid.legStretch = desc.legStretch;
            humanoid.upperArmTwist = desc.upperArmTwist;
            humanoid.lowerArmTwist = desc.lowerArmTwist;
            humanoid.upperLegTwist = desc.upperLegTwist;
            humanoid.lowerLegTwist = desc.lowerArmTwist;
            humanoid.feetSpacing = desc.feetSpacing;
            humanoid.hasTranslationDoF = desc.hasTranslationDoF;

            foreach (var x in desc.human)
            {
                var key = x.humanBone.FromHumanBodyBone();
                var found = humanoid.humanBones.FirstOrDefault(y => y.vrmBone == key);
                if (found == null)
                {
                    found = new glTF_VRM_HumanoidBone
                    {
                        vrmBone = key
                    };
                    humanoid.humanBones.Add(found);
                }
                found.node = nodes.FindIndex(y => y.name == x.boneName);

                found.useDefaultValues = x.useDefaultValues;
                found.axisLength = x.axisLength;
                found.center = x.center;
                found.max = x.max;
                found.min = x.min;
            }
        }

        public static UniHumanoid.AvatarDescription ToDescription(this VRM.glTF_VRM_Humanoid humanoid, 
            List<Transform> nodes)
        {
            var description = ScriptableObject.CreateInstance<UniHumanoid.AvatarDescription>();
            description.upperLegTwist = humanoid.upperLegTwist;
            description.lowerLegTwist = humanoid.lowerLegTwist;
            description.upperArmTwist = humanoid.upperArmTwist;
            description.lowerArmTwist = humanoid.lowerArmTwist;
            description.armStretch = humanoid.armStretch;
            description.legStretch = humanoid.legStretch;
            description.hasTranslationDoF = humanoid.hasTranslationDoF;
            description.human = humanoid.humanBones
                .Where(x => x.node >= 0 && x.node < nodes.Count)
                .Select(x => new UniHumanoid.BoneLimit
                {
                    boneName = nodes[x.node].name,
                    useDefaultValues = x.useDefaultValues,
                    axisLength = x.axisLength,
                    center = x.center,
                    min = x.min,
                    max = x.max,
                    humanBone = x.vrmBone.ToHumanBodyBone(),
                })
            .Where(x => x.humanBone != HumanBodyBones.LastBone)
            .ToArray();
            return description;
        }

        public static glTF_VRM_BlendShapeBind Cerate(Transform root, List<Mesh> meshes, BlendShapeBinding binding)
        {
            var transform = UniGLTF.UnityExtensions.GetFromPath(root.transform, binding.RelativePath);
            var renderer = transform.GetComponent<SkinnedMeshRenderer>();
            var mesh = renderer.sharedMesh;
            var meshIndex = meshes.IndexOf(mesh);

            return new glTF_VRM_BlendShapeBind
            {
                mesh = meshIndex,
                index = binding.Index,
                weight = binding.Weight,
            };
        }

        public static void Add(this glTF_VRM_BlendShapeMaster master,
            BlendShapeClip clip, Transform transform, List<Mesh> meshes)
        {
            var list = new List<glTF_VRM_BlendShapeBind>();
            if (clip.Values != null)
            {
                list.AddRange(clip.Values.Select(y => Cerate(transform, meshes.ToList(), y)));
            }

            var materialList = new List<glTF_VRM_MaterialValueBind>();
            if (clip.MaterialValues != null)
            {
                materialList.AddRange(clip.MaterialValues.Select(y => new glTF_VRM_MaterialValueBind
                {
                    materialName = y.MaterialName,
                    propertyName = y.ValueName,
                    targetValue = y.TargetValue.ToArray(),
                }));
            }

            var group = new glTF_VRM_BlendShapeGroup
            {
                name = clip.BlendShapeName,
                presetName = clip.Preset.ToString().ToLower(),
                isBinary = clip.IsBinary,
                binds = list,
                materialValues = materialList,
            };
            master.blendShapeGroups.Add(group);
        }

        public static void Apply(this glTF_VRM_DegreeMap map, CurveMapper mapper)
        {
            map.curve = mapper.Curve.keys.SelectMany(x => new float[] { x.time, x.value, x.inTangent, x.outTangent }).ToArray();
            map.xRange = mapper.CurveXRangeDegree;
            map.yRange = mapper.CurveYRangeDegree;
        }
    }
}
