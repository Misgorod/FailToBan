using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class RuleTypeTests
    {
        [Test]
        public void TryParse_ParseExisting_ReturnsTrue()
        {
            // Act
            var result = RuleTypeExtension.TryParse("enabled", out var ruleType);
            // Assert
            Assert.That(result, Is.EqualTo(true));
            Assert.That(ruleType, Is.EqualTo(RuleType.Enabled));
        }

        [Test]
        public void TryParse_ParseNotExisting_ReturnsFalse()
        {
            // Act
            var result = RuleTypeExtension.TryParse("not_existing", out var ruleType);
            // Assert
            Assert.That(result, Is.EqualTo(false));
            Assert.That(ruleType, Is.EqualTo(RuleType.Null));
        }
    }
}