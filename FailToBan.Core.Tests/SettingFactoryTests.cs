using System.Text.RegularExpressions;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class SettingFactoryTests
    {
        [TestCase(@"^\[([\w\d?=]+)\]\s*$",
            @"^([^#\s][_.\w]*)\s*=\s*([^\r]+)\s*", 
            @"^\s+(.+)\s*", 
            "[TestSection]\r\n" +
                "enabled = value\r\n", 
            new string[] {"TestSection"}, 
            new RuleType[] {RuleType.Enabled},
            "value")
        ]
        [TestCase(@"^\[([\w\d?=]+)\]\s*$",
            @"^([^#\s][_.\w]*)\s*=\s*([^\r]+)\s*",
            @"^\s+(.+)\s*",
            "[TestSection]\r\n" +
            "enabled = value\r\n" +
            " value\r\n",
            new string[] { "TestSection" },
            new RuleType[] { RuleType.Enabled },
            "value\r\n value\r")
        ]
        public void Build_CreateSettingFromValidConfig_SettingCreated(string sectionPattern, string rulePattern, string continuationPattern, string config, string[] sections, RuleType[] rules, string value)
        {
            // Arrange
            var sectionRegex = new Regex(sectionPattern);
            var ruleRegex = new Regex(rulePattern);
            var continuationRegex = new Regex(continuationPattern);
            var factory = new SettingFactory(sectionRegex, ruleRegex, continuationRegex);
            // Act
            var setting = factory.Build(config);
            // Assert
            foreach (var sectionName in sections)
            {
                var section = setting.GetSection(sectionName);
                Assert.That(section, Is.Not.Null);
                foreach (var ruleType in rules)
                {
                    var rule = section.GetRule(ruleType);
                    Assert.That(rule, Is.EqualTo(value));
                }
            }
        }

        [Test]
        public void Build_CreateSettingFromNull_ReturnsNull()
        {
            // Arrange
            var sut = new SettingFactory(new Regex(""), new Regex(""), new Regex(""));
            // Act
            var setting = sut.Build(null);
            // Assert
            Assert.That(setting, Is.Null);
        }
    }
}