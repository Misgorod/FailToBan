using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class SectionTests
    {
        [Test]
        public void Set_SetExistingRule_RuleSet()
        {
            var section = new Section("TestSection");

            var newSection = section.Set(RuleType.Enabled, "true");
            var ruleValue = section.Get(RuleType.Enabled);

            Assert.That(section, Is.SameAs(newSection));
            Assert.That(ruleValue, Is.EqualTo("true"));
        }
    }
}