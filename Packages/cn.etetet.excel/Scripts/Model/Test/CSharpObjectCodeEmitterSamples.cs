using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Test
{
    [EnableClass]
    public sealed class CSharpObjectCodeEmitterConstructorSample : Object
    {
        [BsonElement]
        private int hidden;

        public string Name;

        public int PublicProp { get; set; }

        [BsonConstructor]
        public CSharpObjectCodeEmitterConstructorSample(int hidden)
        {
            this.hidden = hidden;
        }

        public int Hidden => this.hidden;
    }

    [EnableClass]
    public sealed class CSharpObjectCodeEmitterPropertySample : Object
    {
        public int PublicProp { get; set; }

        [BsonIgnoreIfNull]
        public string NullableText { get; set; }

        [BsonIgnoreIfDefault]
        [BsonDefaultValue(5)]
        public int Count { get; set; } = 5;

        public Dictionary<int, string> Map { get; set; } = new();
    }

    [EnableClass]
    public sealed class CSharpObjectCodeEmitterBasicSample : Object
    {
        public int Id;

        public string Name;

        public CSharpObjectCodeEmitterAbstractChild Child { get; set; }
    }

    [EnableClass]
    public sealed class CSharpObjectCodeEmitterCollectionSample : Object
    {
        public List<int> Numbers = new();

        public HashSet<string> Tags = new();

        public Dictionary<int, string> Named = new();
    }

    [EnableClass]
    public abstract class CSharpObjectCodeEmitterAbstractChild : Object
    {
        public int ChildId;
    }

    [EnableClass]
    public sealed class CSharpObjectCodeEmitterConcreteChild : CSharpObjectCodeEmitterAbstractChild
    {
        public string Label;
    }

    [EnableClass]
    public sealed class CSharpObjectCodeEmitterMissingCtorSample : Object
    {
        [BsonElement]
        private int hidden = 42;

        public int Hidden => this.hidden;
    }

    [EnableClass]
    public sealed class CSharpObjectCodeEmitterCycleSample : Object
    {
        public CSharpObjectCodeEmitterCycleSample Next { get; set; }
    }
}
