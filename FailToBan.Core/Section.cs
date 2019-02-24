using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FailToBan.Core
{
    /// <inheritdoc />
    public class Section : ISection
    {
        private readonly Dictionary<RuleType, string> rules = new Dictionary<RuleType, string>();

        /// <inheritdoc />
        /// <summary>
        /// Устанавливает значение правилу, создавая его, если значение не было установлено
        /// </summary>
        /// <param name="rule">Правило, значение которого задаётся</param>
        /// <param name="value">Значение, устанавливаемое правилу</param>
        /// <returns>Текущая секция</returns>
        public ISection Set(RuleType rule, string value)
        {
            rules[rule] = value;
            return this;
        }

        /// <summary>
        /// Добавляет значение к правилу, создавая его, если значение не было установлено
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISection Add(RuleType rule, string value)
        {
            rules[rule] += $"{Environment.NewLine} {value}";
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Возвращает значение правила
        /// </summary>
        /// <param name="rule">Правило</param>
        /// <returns>Значение правила</returns>
        public string Get(RuleType rule)
        {
            return rules.ContainsKey(rule) ? rules[rule] : null;
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
            foreach (var (rule, value) in rules)
            {
                builder.AppendLine($"{RuleTypeExtension.ToString(rule)} = {value}");
            }

            return builder.ToString();
        }

    }
}