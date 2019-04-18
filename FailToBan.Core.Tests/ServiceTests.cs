using Moq;
using Newtonsoft.Json.Bson;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        [TestCase("ruleValue")]
        public void GetRule_GetExistingRule_RuleGot(string ruleValue)
        {
            // Arrange
            var mockSection = new Mock<ISection>(); 
            mockSection.Setup(x => x.GetRule(It.IsAny<RuleType>())).Returns(ruleValue);
            var mockConfSetting = new Mock<ISetting>();
            mockConfSetting.Setup(x => x.GetSection(It.IsAny<string>()))
                       .Returns(mockSection.Object);
            var mockLocalSetting = new Mock<ISetting>();
            mockLocalSetting.Setup(x => x.GetSection(It.IsAny<string>())).Returns((ISection)null);
            var sut = new Service("Test", mockLocalSetting.Object)
            {
                ConfSetting = mockConfSetting.Object,
            };
            // Act
            var result = sut.GetRule("section", RuleType.Action);
            // Assert
            Assert.That(ruleValue, Is.EqualTo(result));
        }

        [Test]
        public void SetRule_RuleSet()
        {
            // Arrange
            var mockSection = new Mock<ISection>();
            mockSection.Setup(x => x.SetRule(It.IsAny<RuleType>(), It.IsAny<string>()))
                                    .Returns(mockSection.Object);
            var mockSetting = new Mock<ISetting>();
            mockSetting.Setup(x => x.GetOrCreateSection(It.IsAny<string>())).Returns(mockSection.Object);
            mockSetting.Setup(x => x.AddSection(It.IsAny<string>(), It.IsAny<ISection>())).Returns(true);
            var sut = new Service("Service", mockSetting.Object);
            // Act
            sut.SetRule("section", RuleType.Null, "value");
        }

        [Test]
        public void Clone_ServiceCloned()
        {
            // Arrange
            var clonedMockSetting = new Mock<ISetting>();
            var mockSetting = new Mock<ISetting>();
            mockSetting.Setup(x => x.Clone()).Returns(clonedMockSetting.Object);
            var sut = new Service("service", mockSetting.Object);
            // Act
            var result = sut.Clone();
            // Assert
            Assert.That(result, Is.Not.SameAs(sut));
        }
    }
}