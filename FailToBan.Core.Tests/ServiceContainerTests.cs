using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class ServiceContainerTests
    {

        [Test]
        public void Default_ServiceSetAndGot()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            // Act
            sut.SetDefault(serviceMock.Object);
            var service = sut.GetDefault();
            // Assert
            Assert.That(serviceMock.Object, Is.SameAs(service));
        }

        [Test]
        public void Action_ServiceSetAndGot()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.SetupGet(x => x.Name).Returns("test");
            // Act
            sut.SetAction(serviceMock.Object);
            var service = sut.GetAction("test");
            // Assert
            Assert.That(serviceMock.Object, Is.SameAs(service));
        }

        [Test]
        public void DeleteAction_ActionDeleted()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.SetupGet(x => x.Name).Returns("action");
            sut.SetAction(serviceMock.Object);
            // Act
            var result = sut.DeleteAction("action");
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Filter_ServiceSetAndGot()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.SetupGet(x => x.Name).Returns("test");
            // Act
            sut.SetFilter(serviceMock.Object);
            var service = sut.GetFilter("test");
            // Assert
            Assert.That(serviceMock.Object, Is.SameAs(service));
        }

        [Test]
        public void DeleteFilter_FilterDeleted()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.SetupGet(x => x.Name).Returns("filter");
            sut.SetFilter(serviceMock.Object);
            // Act
            var result = sut.DeleteFilter("filter");
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Jail_ServiceSetAndGot()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.SetupGet(x => x.Name).Returns("test");
            // Act
            sut.SetJail(serviceMock.Object);
            var service = sut.GetJail("test");
            // Assert
            Assert.That(serviceMock.Object, Is.SameAs(service));
        }

        [Test]
        public void DeleteJail_JailDeleted()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.SetupGet(x => x.Name).Returns("jail");
            sut.SetJail(serviceMock.Object);
            // Act
            var result = sut.DeleteJail("jail");
            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Actions_GetCopy()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.Clone()).Returns(new Mock<IService>().Object);
            serviceMock.Setup(x => x.Name).Returns("test");
            sut.SetAction(serviceMock.Object);
            // Act
            var actions = sut.Actions;
            // Assert
            Assert.That(actions.Count, Is.EqualTo(1));
            Assert.That(actions["test"], Is.Not.SameAs(serviceMock.Object));
        }

        [Test]
        public void Filters_GetCopy()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.Clone()).Returns(new Mock<IService>().Object);
            serviceMock.Setup(x => x.Name).Returns("test");
            sut.SetFilter(serviceMock.Object);
            // Act
            var filters = sut.Filters;
            // Assert
            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters["test"], Is.Not.SameAs(serviceMock.Object));
        }

        [Test]
        public void Jails_GetCopy()
        {
            // Arrange
            var sut = new ServiceContainer();
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.Clone()).Returns(new Mock<IService>().Object);
            serviceMock.Setup(x => x.Name).Returns("test");
            sut.SetJail(serviceMock.Object);
            // Act
            var jails = sut.Jails;
            // Assert
            Assert.That(jails.Count, Is.EqualTo(1));
            Assert.That(jails["test"], Is.Not.SameAs(serviceMock.Object));
        }
    }
}