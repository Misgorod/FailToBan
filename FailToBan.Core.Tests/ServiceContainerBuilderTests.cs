using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class ServiceContainerBuilderTests
    {
        [Test]
        public void Build_ServiceContainerBuilt()
        {
            // Arrange
            var fs = new MockFileSystem();
            fs.AddDirectory(@"/test");
            fs.AddFile(@"/test/jail.conf", new MockFileData("test default"));

            fs.AddDirectory(@"/test/jail.d");
            fs.AddFile(@"/test/jail.d/jail1.conf", new MockFileData("test jail"));
            fs.AddFile(@"/test/jail.d/jail2.local", new MockFileData("test jail"));

            fs.AddDirectory(@"/test/action.d");
            fs.AddFile(@"/test/action.d/action1.conf", new MockFileData("test action"));
            fs.AddFile(@"/test/action.d/action2.local", new MockFileData("test action"));
            
            fs.AddDirectory(@"/test/filter.d");
            fs.AddFile(@"/test/filter.d/filter1.conf", new MockFileData("test filter"));
            fs.AddFile(@"/test/filter.d/filter2.local", new MockFileData("test filter"));

            var mockSettingFactory = new Mock<ISettingFactory>();
            mockSettingFactory
                .Setup(x => x.Build())
                .Returns(new Mock<ISetting>().Object);
            mockSettingFactory
                .Setup(x => x.Build(It.IsAny<string>()))
                .Returns(new Mock<ISetting>().Object);

            var mockServiceFactory = new Mock<IServiceFactory>();
            // Setup service building
            mockServiceFactory
                .Setup(x => x.BuildService(It.IsAny<string>()))
                .Returns<string>(s =>
                {
                    var mockService = new Mock<IService>();
                    mockService.Setup(x => x.Name).Returns(s);
                    return mockService.Object;
                });
            mockServiceFactory
                .Setup(x => x.BuildService(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((name, confText, localText) => mockServiceFactory.Object.BuildService(name));
            // Setup jail building
            mockServiceFactory
                .Setup(x => x.BuildJail(It.IsAny<string>(), It.IsAny<ISetting>(), It.IsAny<ISetting>(), It.IsAny<IService>()))
                .Returns<string, ISetting, ISetting, IService>((name, conf, local, service) => mockServiceFactory.Object.BuildService(name));
            mockServiceFactory
                .Setup(x => x.BuildJail(It.IsAny<string>(), It.IsAny<ISetting>(), It.IsAny<IService>()))
                .Returns<string, ISetting, IService>((name, local, service) => mockServiceFactory.Object.BuildService(name));


            var sut = new ServiceContainerBuilder(fs, mockServiceFactory.Object, mockSettingFactory.Object);

            // Act
            var serviceContainer = sut
                .BuildDefault(@"C://test")
                .BuildJails(@"C://test/jail.d")
                .BuildJails(@"C://test/jail.d")
                .BuildActions(@"C://test/action.d")
                .BuildActions(@"C://test/action.d")
                .BuildFilters(@"C://test/filter.d")
                .BuildFilters(@"C://test/filter.d")
                .Build();

            // Assert
            Assert.That(serviceContainer.GetDefault(), Is.Not.Null);
            Assert.That(serviceContainer.Jails.Count, Is.EqualTo(2));
            Assert.That(serviceContainer.Actions.Count, Is.EqualTo(2));
            Assert.That(serviceContainer.Filters.Count, Is.EqualTo(2));
            Assert.That(serviceContainer.GetJail("jail1"), Is.Not.Null);
            Assert.That(serviceContainer.GetAction("action1"), Is.Not.Null);
            Assert.That(serviceContainer.GetFilter("filter1"), Is.Not.Null);
        }

        [Test]
        public void Build_ThrowsVicException()
        {
            // Arrange
            var fs = new MockFileSystem();
            var mockSettingFactory = new Mock<ISettingFactory>();
            var mockServiceFactory = new Mock<IServiceFactory>();

            var sut = new ServiceContainerBuilder(fs, mockServiceFactory.Object, mockSettingFactory.Object);
            // Assert
            Assert.Throws<VicException>(() => sut.BuildJails(@"/test"));
        }
    }
}