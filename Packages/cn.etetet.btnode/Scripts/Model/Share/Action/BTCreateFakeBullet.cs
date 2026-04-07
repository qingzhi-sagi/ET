using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    public class BTCreateFakeBullet : BTAction
    {
        [BTInput(typeof(Unit))]
        public string Caster;

        [BTInput(typeof(Unit))]
        public string Target;
        
        public BindPoint CasterBindPoint;
        public BindPoint TargetBindPoint;
        public int Speed;

#if UNITY
        [LabelText("特效资源名")]
#endif
        public string EffectName;

        public int Duration = 50000;
    }
}
