using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class AllowInstanceAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class)]
	[EnableClass]
	public class BaseAttribute: EnableClassAttribute
	{
	}
}
