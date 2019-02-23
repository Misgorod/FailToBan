using Moq;
using NUnit.Framework;

namespace FailToBan.Core.Tests
{
    [TestFixture]
    public class SettingContainerTests
    {
        [TestCase("sshd", SettingType.Jail)]
        [TestCase("service", SettingType.Action)]
        public void GetSetting_GetExistingSetting_SettingGot(string service, SettingType type)
        {
            // Arrange
            var settingContainer = new SettingContainer();
            var mockSetting = new Mock<ISetting>(MockBehavior.Strict).Object;
            settingContainer.AddSetting(service, type, mockSetting);
            // Act
            var result = settingContainer.GetSetting(service, type);
            // Assert
            Assert.That(result, Is.SameAs(mockSetting));
        }

        [TestCase("sshd", SettingType.Jail)]
        [TestCase("service", SettingType.Action)]
        public void GetSetting_GetNotExistingSetting_NullGot(string service, SettingType type)
        {
            // Arrange
            var settingContainer = new SettingContainer();
            // Act
            var setting = settingContainer.GetSetting(service, type);
            // Assert
            Assert.That(setting, Is.Null);
        }

        [TestCase("sshd", SettingType.Jail)]
        [TestCase("service", SettingType.Action)]
        public void AddSetting_AddExisting_False(string service, SettingType type)
        {
            // Arrange
            var settingContainer = new SettingContainer();
            var mockSetting = new Mock<ISetting>(MockBehavior.Strict).Object;
            settingContainer.AddSetting(service, type, mockSetting);
            // Act
            var result = settingContainer.AddSetting(service, type, mockSetting);
            // Assert
            Assert.That(result, Is.False);
        }
    }
}