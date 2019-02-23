using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class SettingTests
    {
        [TestCase("sshd")]
        [TestCase("mysqld")]
        public void GetSection_GetExistingSection_SectionGot(string service)
        {
            // Arrange
            var setting = new Setting();
            var mockSection = new Mock<ISection>().Object;
            // Act
            setting.AddSection(service, mockSection);
            var section = setting.GetSection(service);
            // Assert
            Assert.That(section, Is.SameAs(mockSection));
        }

        [TestCase("sshd")]
        [TestCase("mysqld")]
        public void GetSection_GetNotExistingSection_NullGot(string service)
        {
            // Arrange
            var setting = new Setting();
            // Act
            var section = setting.GetSection(service);
            // Assert
            Assert.That(section, Is.Null);
        }

        [Test]
        public void ToString_GetStringRepresentation_StringGot()
        {
            // Arrrange
            var setting = new Setting();
            var mockSection = new Mock<ISection>(MockBehavior.Strict);
            mockSection
                .Setup(x => x.ToString("TestSection"))
                .Returns("[TestSection]\r" +
                         "\nvalue\r\n");
            setting.AddSection("TestSection", mockSection.Object);
            var expected = "[TestSection]\r" +
                           "\nvalue\r\n";
            // Act
            var result = setting.ToString();
            // Assert
            Assert.That(expected, Is.EqualTo(result));
        }
    }
}