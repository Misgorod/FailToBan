using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class ServiceFactoryTests
    {
        [Test]
        public void BuildService_FromName_ReturnsService()
        {
            // Arrange
            var mockSetting = new Mock<ISetting>();
            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory.Setup(x => x.Build()).Returns(mockSetting.Object);
            var sut = new ServiceFactory(mockSettingFactory.Object);
            // Act
            var result = sut.BuildService("service");
            // Assert
            Assert.That(result, Is.Not.Null);
            var serviceName = result.Name;
            Assert.That(serviceName, Is.EqualTo("service"));
        }

        [Test]
        public void BuildService_FromText_ReturnsService()
        {
            // Arrange
            var mockSetting = new Mock<ISetting>();
            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory.Setup(x => x.Build(It.IsAny<string>())).Returns(mockSetting.Object);
            var sut = new ServiceFactory(mockSettingFactory.Object);
            // Act
            var result = sut.BuildService("service", "conf", "local");
            // Assert
            Assert.That(result, Is.Not.Null);
            var serviceName = result.Name;
            Assert.That(serviceName, Is.EqualTo("service"));
        }

        [Test]
        public void BuildService_FromNameAndSetting_ReturnsService()
        {
            // Arrange
            var mockSetting = new Mock<ISetting>();
            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory.Setup(x => x.Build()).Returns(new Mock<ISetting>().Object);
            var sut = new ServiceFactory(mockSettingFactory.Object);
            // Act 
            var result = sut.BuildService("service", mockSetting.Object);
            // Assert
            Assert.That(result, Is.Not.Null);
            var serviceName = result.Name;
            Assert.That(serviceName, Is.EqualTo("service"));
        }

        [Test]
        public void BuildJail_FromName_ReturnsJail()
        {
            // Arrange
            var mockSetting = new Mock<ISetting>();
            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory.Setup(x => x.Build()).Returns(mockSetting.Object);
            var mockService = new Mock<IService>();
            var sut = new ServiceFactory(mockSettingFactory.Object);
            // Act
            var result = sut.BuildJail("service", mockService.Object);
            // Assert
            Assert.That(result, Is.Not.Null);
            var serviceName = result.Name;
            Assert.That(serviceName, Is.EqualTo("service"));
        }

        [Test]
        public void BuildJail_FromText_ReturnsService()
        {
            // Arrange
            var mockSetting = new Mock<ISetting>();
            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory.Setup(x => x.Build(It.IsAny<string>())).Returns(mockSetting.Object);
            var mockService = new Mock<IService>();
            var sut = new ServiceFactory(mockSettingFactory.Object);
            // Act
            var result = sut.BuildJail("service", "conf", "local", mockService.Object);
            // Assert
            Assert.That(result, Is.Not.Null);
            var serviceName = result.Name;
            Assert.That(serviceName, Is.EqualTo("service"));
        }

        [Test]
        public void BuildJail_FromNameAndSetting_ReturnsJail()
        {
            // Arrange
            var mockSetting = new Mock<ISetting>();
            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory.Setup(x => x.Build()).Returns(new Mock<ISetting>().Object);
            var mockService = new Mock<IService>();
            var sut = new ServiceFactory(mockSettingFactory.Object);
            // Act 
            var result = sut.BuildJail("service", mockSetting.Object, mockService.Object);
            // Assert
            Assert.That(result, Is.Not.Null);
            var serviceName = result.Name;
            Assert.That(serviceName, Is.EqualTo("service"));
        }
    }
}