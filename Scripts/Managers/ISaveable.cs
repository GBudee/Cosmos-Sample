using System.IO;

public interface ISaveable
{
    public void Save(int version, BinaryWriter writer, bool changingScenes);
    public void Load(int version, BinaryReader reader);
}