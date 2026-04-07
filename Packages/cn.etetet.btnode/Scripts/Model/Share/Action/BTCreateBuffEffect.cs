using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    public class BTCreateBuffEffect : BTNode
    {
        [BTInput(typeof(Unit))]
        public string Unit;
        
        [BTInput(typeof(Buff))]
        public string Buff;
        
        public BindPoint BindPoint;

#if UNITY
        [LabelText("特效资源名")]
#endif
        public string EffectName;

        public int Duration = 5000;
    }
}
