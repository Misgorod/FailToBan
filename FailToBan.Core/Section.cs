using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FailToBan.Core
{
    /// <inheritdoc />
    public class Section : ISection
    {
        public Dictionary<RuleType, string> Rules { get; }

        private Section(Dictionary<RuleType, string> rules)
        {
            this.Rules = rules;
        }

        public Section() : this(new Dictionary<RuleType, string>())
        { }

        /// <inheritdoc />
        /// <summary>
        /// Устанавливает значение правилу, создавая его, если значение не было установлено
        /// </summary>
        /// <param name="rule">Правило, значение которого задаётся</param>
        /// <param name="value">Значение, устанавливаемое правилу</param>
        /// <returns>Текущая секция</returns>
        public ISection SetRule(RuleType rule, string value)
        {
            Rules[rule] = value;
            return this;
        }

        /// <summary>
        /// Добавляет значение к правилу, создавая его, если значение не было установлено
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISection AddToRule(RuleType rule, string value)
        {
            Rules[rule] += $"{Environment.NewLine} {value}";
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Возвращает значение правила
        /// </summary>
        /// <param name="rule">Правило</param>
        /// <returns>Значение правила</returns>
        public string GetRule(RuleType rule)
        {
            return Rules.ContainsKey(rule) ? Rules[rule] : null;
        }

        public ISection Clone()
        {
            var cloneRules = new Dictionary<RuleType, string>(Rules);
            var clone = new Section(cloneRules);
            return clone;
        }

        /// <inheritdoc />
        /// <summary>
        /// Возвращает текстовое представление секции для записи в конфигурационный файл
        /// </summary>
        /// <param name="name">Имя секции</param>
        /// <returns></returns>
        public string ToString(string name)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[{name}]");
            foreach (var (rule, value) in Rules)
            {
                builder.AppendLine($"{RuleTypeExtension.ToString(rule)} = {value}");
            }

            return builder.ToString();
        }

    }
}