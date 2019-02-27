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
            var sut = new Service(mockConfSetting.Object, mockLocalSetting.Object, "Test");
            // Act
            var result = sut.GetRule("section", RuleType.Action);
            // Assert
            Assert.That(ruleValue, Is.EqualTo(result));
        }

        //[TestCase("")]
        //public void SetRule_SetNotExistingSection_RuleSet(string ruleValue)
        //{
        //    // Arrange
        //    var sut = new Service()
        //}
    }
}