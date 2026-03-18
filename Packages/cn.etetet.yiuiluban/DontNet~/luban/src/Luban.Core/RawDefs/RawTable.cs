using Luban.Defs;

namespace Luban.RawDefs;

public class RawTable
{
    public  string Package { get; set; }
    private string m_Namespace;

    public string Namespace
    {
        get => m_Namespace;
        set => (m_Namespace, Package) = ETNamespacePackage.Get(value);
    }

    public string Name { get; set; }

    public string Index { get; set; }

    private string m_ValueType;

    public string ValueType
    {
        get => m_ValueType;
        set
        {
            (m_ValueType, var package) = ETNamespacePackage.Get(value);
            if (!string.IsNullOrEmpty(package))
            {
                Package = package;
            }
        }
    }

    public bool ReadSchemaFromFile { get; set; }

    public TableMode Mode { get; set; }

    public string Comment { get; set; }

    public Dictionary<string, string> Tags { get; set; }

    public List<string> Groups { get; set; } = new();

    public List<string> InputFiles { get; set; } = new();

    public string OutputFile { get; set; }
}
