using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class ServiceSaverAdapterTests
    {
        [Test]
        public void Save()
        {
            // Arrange
            var mockService = new Mock<IService>();
            var mockFirstSaver = new Mock<IServiceSaver>();
            mockFirstSaver.Setup(x => x.Save(It.IsAny<IService>()));
            var mockSecondSaver = new Mock<IServiceSaver>();
            mockSecondSaver.Setup(x => x.Save(It.IsAny<IService>()));
            var sut = new ServiceSaverAdapter(mockFirstSaver.Object, mockSecondSaver.Object);
            // Act
            sut.Save(mockService.Object);
        }

        [Test]
        public void Delete()
        {
            // Arrange
            var mockService = new Mock<IService>();
            var mockFirstSaver = new Mock<IServiceSaver>();
            mockFirstSaver.Setup(x => x.Delete(It.IsAny<IService>()));
            var mockSecondSaver = new Mock<IServiceSaver>();
            mockSecondSaver.Setup(x => x.Delete(It.IsAny<IService>()));
            var sut = new ServiceSaverAdapter(mockFirstSaver.Object, mockSecondSaver.Object);
            // Act
            sut.Delete(mockService.Object);
        }
    }
}