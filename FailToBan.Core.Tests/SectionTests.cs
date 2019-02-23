using System.Data;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class SectionTests
    {
        [Test]
        public void Get_GetNotExistingRule_NullGot()
        {
            // Arrange
            var section = new Section();
            // Act
            var ruleValue = section.Get(RuleType.Null);
            // Assert
            Assert.That(ruleValue, Is.Null);
        }

        [Test]
        public void Set_SetExistingRule_RuleSet()
        {
            // Arrange
            var section = new Section();
            // Act
            var newSection = section.Set(RuleType.Enabled, "true");
            var ruleValue = section.Get(RuleType.Enabled);
            // Assert
            Assert.That(section, Is.SameAs(newSection));
            Assert.That(ruleValue, Is.EqualTo("true"));
        }

        [Test]
        public void ToString_ConvertToString_Converted()
        {
            // Arrange
            var section = new Section();
            const string expected = "[TestSection]\r\n" +
                                    "enabled = true\r" +
                                    "\n";
            // Act
            section.Set(RuleType.Enabled, "true");
            var result = section.ToString("TestSection");
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}