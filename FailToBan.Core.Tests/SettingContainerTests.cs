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
            var mockConf = new Mock<ISetting>(MockBehavior.Strict).Object;
            var mockLocal = new Mock<ISetting>(MockBehavior.Strict).Object;
            settingContainer.AddConf(service, type, mockConf);
            settingContainer.AddLocal(service, type, mockLocal);
            // Act
            var result = settingContainer.GetSettings(service, type);
            // Assert
            Assert.That(result.conf, Is.SameAs(mockConf));
            Assert.That(result.local, Is.SameAs(mockLocal));
        }

        [TestCase("sshd", SettingType.Jail)]
        [TestCase("service", SettingType.Action)]
        public void GetSetting_GetNotExistingSetting_NullGot(string service, SettingType type)
        {
            // Arrange
            var settingContainer = new SettingContainer();
            // Act
            var setting = settingContainer.GetSettings(service, type);
            // Assert
            Assert.That(setting.conf, Is.Null);
            Assert.That(setting.local, Is.Null);
        }

        [TestCase("sshd", SettingType.Jail)]
        [TestCase("service", SettingType.Action)]
        public void AddSetting_AddExisting_False(string service, SettingType type)
        {
            // Arrange
            var settingContainer = new SettingContainer();
            var mockConf = new Mock<ISetting>(MockBehavior.Strict).Object;
            var mockLocal = new Mock<ISetting>(MockBehavior.Strict).Object;
            settingContainer.AddConf(service, type, mockConf);
            settingContainer.AddLocal(service, type, mockLocal);
            // Act
            var confResult = settingContainer.AddConf(service, type, mockConf);
            var localResult = settingContainer.AddLocal(service, type, mockLocal);
            // Assert
            Assert.That(confResult, Is.False);
            Assert.That(localResult, Is.False);
        }
    }
}