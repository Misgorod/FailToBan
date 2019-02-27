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
            var ruleValue = section.GetRule(RuleType.Null);
            // Assert
            Assert.That(ruleValue, Is.Null);
        }

        [Test]
        public void Add_AddToExistingRule_RuleAdded()
        {
            // Arrange
            var section = new Section();
            // Act
            section.SetRule(RuleType.Action, "value");
            section.AddToRule(RuleType.Action, "added");
            var rule = section.GetRule(RuleType.Action);
            // Assert
            Assert.That(rule, Is.EqualTo("value\r\n added"));
        }

        [Test]
        public void Set_SetExistingRule_RuleSet()
        {
            // Arrange
            var section = new Section();
            // Act
            var newSection = section.SetRule(RuleType.Enabled, "true");
            var ruleValue = section.GetRule(RuleType.Enabled);
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
            section.SetRule(RuleType.Enabled, "true");
            var result = section.ToString("TestSection");
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(RuleType.Action, "value")]
        public void Clone_CloneSection_ReturnsCloned(RuleType type, string value)
        {
            // Arrange
            var sut = new Section();
            sut.SetRule(type, value);
            // Act
            var clone = sut.Clone();
            var clonedValue = clone.GetRule(type);
            var sutValue = sut.GetRule(type);
            // Assert
            Assert.That(clone, Is.Not.SameAs(sut));
            Assert.That(clonedValue, Is.EqualTo(sutValue));
        }
    }
}