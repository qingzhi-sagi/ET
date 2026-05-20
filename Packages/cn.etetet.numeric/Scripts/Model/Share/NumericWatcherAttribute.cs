using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NumericWatcherAttribute : BaseAttribute
	{
		public int SceneType { get; }
		
		public NumericType NumericType { get; }

		public NumericWatcherAttribute(int sceneType, int type)
            : this(sceneType, (NumericType)type)
		{
		}

		public NumericWatcherAttribute(int sceneType, NumericType type)
		{
			this.SceneType = sceneType;
			this.NumericType = type;
		}
	}
}
