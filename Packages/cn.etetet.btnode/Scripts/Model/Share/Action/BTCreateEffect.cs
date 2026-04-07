using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    public class BTCreateEffect : BTNode
    {
        [BTInput(typeof(Unit))]
        public string Unit;
        
        public BindPoint BindPoint;

#if UNITY
        [LabelText("特效资源名")]
#endif
        public string EffectName;

        public int Duration = 5000;
    }
}
