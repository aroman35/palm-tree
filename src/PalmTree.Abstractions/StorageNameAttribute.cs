namespace PalmTree.Abstractions;

public class StorageNameAttribute : Attribute
{
    public StorageNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}