using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class JailTests
    {
        [TestCase("value")]
        public void GetRule_ExistsInDefault_ReturnsValue(string value)
        {
            // Arrange
            var mockLocalSetting = new Mock<ISetting>();
            mockLocalSetting.Setup(x => x.GetSection(It.IsAny<string>()))
                            .Returns((ISection)null);
            var mockDefaultService = new Mock<IService>();
            mockDefaultService.Setup(x => x.GetRule(It.Is<string>(s => s == "DEFAULT"), It.IsAny<RuleType>()))
                              .Returns(value);
            var sut = new Jail("jail", mockLocalSetting.Object, mockDefaultService.Object);
            // Act
            var result = sut.GetRule("section", RuleType.Null);
            // Assert
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void Clone_JailCloned()
        {
            // Arrange
            var mockService = new Mock<IService>();
            var clonedMockSetting = new Mock<ISetting>();
            var mockSetting = new Mock<ISetting>();
            mockSetting.Setup(x => x.Clone()).Returns(clonedMockSetting.Object);
            var sut = new Jail("service", mockSetting.Object, mockService.Object);
            // Act
            var result = sut.Clone();
            // Assert
            Assert.That(result, Is.Not.SameAs(sut));
        }
    }
}