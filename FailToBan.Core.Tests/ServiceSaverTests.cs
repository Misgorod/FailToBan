using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class ServiceSaverTests
    {
        [Test]
        public void Save_BakNotExists_ServiceSaved()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory("/test");
            mockFileSystem.File.Create(@"/test/service.conf");
            mockFileSystem.File.Create(@"/test/service.local");
            var mockService = new Mock<IService>();
            mockService.Setup(x => x.Name).Returns("service");
            mockService.Setup(x => x.ConfSetting.ToString()).Returns("service");
            mockService.Setup(x => x.LocalSetting.ToString()).Returns("service");
            var sut = new ServiceSaver(@"/test", mockFileSystem);
            // Act
            sut.Save(mockService.Object);
            // Assert
            var confText = mockFileSystem.File.ReadAllText(@"/test/service.conf");
            var localText = mockFileSystem.File.ReadAllText(@"/test/service.local");
            Assert.That(confText, Is.EqualTo("service"));
            Assert.That(localText, Is.EqualTo("service"));
        }

        [Test]
        public void Save_BakExists_ServiceSaved()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory("/test");
            mockFileSystem.File.Create(@"/test/service.conf");
            mockFileSystem.File.Create(@"/test/service.local");
            mockFileSystem.File.Create(@"/test/service.conf.bak");
            mockFileSystem.File.Create(@"/test/service.local.bak");
            var mockService = new Mock<IService>();
            mockService.Setup(x => x.Name).Returns("service");
            mockService.Setup(x => x.ConfSetting.ToString()).Returns("service");
            mockService.Setup(x => x.LocalSetting.ToString()).Returns("service");
            var sut = new ServiceSaver(@"/test", mockFileSystem);
            // Act
            sut.Save(mockService.Object);
            // Assert
            var confText = mockFileSystem.File.ReadAllText(@"/test/service.conf");
            var localText = mockFileSystem.File.ReadAllText(@"/test/service.local");
            Assert.That(confText, Is.EqualTo("service"));
            Assert.That(localText, Is.EqualTo("service"));
        }

        [Test]
        public void Delete_FileExists()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(@"/test");
            mockFileSystem.File.Create(@"/test/service.conf");
            mockFileSystem.File.Create(@"/test/service.local");
            var mockService = new Mock<IService>();
            mockService.Setup(x => x.Name).Returns("service");
            var sut = new ServiceSaver(@"/test", mockFileSystem);
            // Act
            sut.Delete(mockService.Object);
            // Assert
            Assert.That(mockFileSystem.File.Exists(@"/test/service.conf"), Is.False);
            Assert.That(mockFileSystem.File.Exists(@"/test/service.local"), Is.False);
        }

        [Test]
        public void Delete_FileNotExists()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(@"/test");
            var mockService = new Mock<IService>();
            mockService.Setup(x => x.Name).Returns("service");
            var sut = new ServiceSaver(@"/test", mockFileSystem);
            // Act
            sut.Delete(mockService.Object);
            // Assert
            Assert.That(mockFileSystem.File.Exists(@"/test/service.conf"), Is.False);
            Assert.That(mockFileSystem.File.Exists(@"/test/service.local"), Is.False);
        }
    }
}