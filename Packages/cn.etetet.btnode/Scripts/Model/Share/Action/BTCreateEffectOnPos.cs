using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;
using Sirenix.OdinInspector;

namespace ET
{
    public class BTCreateEffectOnPos : BTNode
    {
        [BTInput(typeof(float3))]
        public string Pos;
        
#if UNITY
        [LabelText("特效资源名")]
#endif
        public string EffectName;

        public int Duration = 5000;
    }
}
