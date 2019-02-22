using System;
using System.Collections.Generic;
using System.Linq;

namespace FailToBan.Core
{
    public static class SectionTypeExtension
    {
        public static SectionType Parse(string name)
        {
            return mapping.ContainsKey(name) ? mapping[name] : SectionType.Name;
        }

        public static string ToString(this SectionType type)
        {
            if (type == SectionType.Name)
            {
                throw new Exception("Wrong string converting from section type");
            }
            return mapping
                .First(x => x.Value == type)
                .Key;
        }

        private static readonly Dictionary<string, SectionType> mapping = new Dictionary<string, SectionType>()
        {
            {"DEFAULT", SectionType.Default},
            {"Definition", SectionType.Definiton},
            {"INCLUDES", SectionType.Includes},
            {"Init", SectionType.Init},
            {"Init?family=inet6", SectionType.InitFamily}
        };
    }
}