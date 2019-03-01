using System.Collections.Generic;

namespace FailToBan.Core
{
    public interface ISetting
    {
        Dictionary<string, ISection> Sections { get; }
        ISection GetSection(string name);
        ISection GetOrCreateSection(string name);
        bool AddSection(string name, ISection section);
        string ToString();
        ISetting Clone();
    }
}