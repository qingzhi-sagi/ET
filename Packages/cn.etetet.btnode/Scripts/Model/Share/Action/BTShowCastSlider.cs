using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    public class BTShowCastSlider : BTAction
    {
        [BTInput(typeof(Buff))]
        [BoxGroup("输入参数")]
        [ReadOnly]
        public string Buff = "Buff";

        [LabelText("是否增加施法进度条")]
        [InfoBox("如: 寒冰箭读条就是增长，暴风雪引导就是减少")]
        public bool IsIncrease = true;

#if UNITY
        [BoxGroup("显示信息", CenterLabel = true)]
        [LabelText("进度条显示名称")]
#endif
        public string ShowDisplayName;

#if UNITY
        [BoxGroup("显示信息")]
        [LabelText("进度条显示图标资源名")]
#endif
        public string IconName;
    }
    
    public struct BTEvent_ShowCastSlider
    {
        public EntityRef<Unit> Unit;
        public EntityRef<Buff> Buff;
        public bool IsIncrease;
        public string IconName;
        public string ShowDisplayName;
    }
}
