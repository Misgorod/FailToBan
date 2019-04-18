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

        [TestCase("Test")]
        public void Clone_CloneSetting_ReturnsCloned(string sectionName)
        {
            // Arrange
            var sut = new Setting();
            var mockSection = new Mock<ISection>(MockBehavior.Strict);
            var clonedMockSection = new Mock<ISection>(MockBehavior.Strict).Object;
            mockSection.Setup(x => x.Clone()).Returns(clonedMockSection);
            var sutSection = mockSection.Object;
            sut.AddSection(sectionName, sutSection);
            // Act
            var clone = sut.Clone();
            var clonedSection = clone.GetSection(sectionName);
            // Assert
            Assert.That(clonedSection, Is.SameAs(clonedMockSection));
            Assert.That(clone, Is.Not.SameAs(sut));
            Assert.That(clonedSection, Is.Not.SameAs(sutSection));
        }

        [TestCase("section")]
        public void Sections_GetSection_GetCloned(string sectionName)
        {
            // Arrange
            var sut = new Setting();
            var mockSection = new Mock<ISection>();
            mockSection
                .Setup(x => x.Clone())
                .Returns(new Mock<ISection>().Object);
            sut.AddSection(sectionName, mockSection.Object);
            // Act 
            var sections = sut.Sections;
            // Assert
            Assert.That(mockSection.Object, Is.Not.SameAs(sections[sectionName]));
        }

        [TestCase("section")]
        public void GetOrCreateSections_ExistingSection_SectionGot(string sectionName)
        {
            // Arrange
            var sut = new Setting();
            var mockSection = new Mock<ISection>();
            sut.AddSection(sectionName, mockSection.Object);
            // Act
            var result = sut.GetOrCreateSection(sectionName);
            // Assert
            Assert.That(result, Is.SameAs(mockSection.Object));
        }

        [TestCase("section")]
        public void GetOrCreateSections_NotExistingSection_SectionGot(string sectionName)
        {
            // Arrange
            var sut = new Setting();
            var mockSection = new Mock<ISection>();
            // Act
            var result = sut.GetOrCreateSection(sectionName);
            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}