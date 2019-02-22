using System;
using System.Collections.Generic;
using System.Text;

namespace FailToBan.Core
{
    public class Section
    {
        private readonly Dictionary<RuleType, string> rules;
        public string Name { get; }

        public Section(string name)
        {
            this.Name = name;
            rules = new Dictionary<RuleType, string>();
        }

        /// <summary>
        /// Устанавливает значение правилу, создавая его, если значение не было установлено
        /// </summary>
        /// <param name="rule">Правило, значение которого задаётся</param>
        /// <param name="value">Значение, устанавливаемое правилу</param>
        /// <returns>Текущая секция</returns>
        public Section Set(RuleType rule, string value)
        {
            rules[rule] = value;
            return this;
        }

        /// <summary>
        /// Возвращает значение правила
        /// </summary>
        /// <param name="rule">Правило</param>
        /// <returns>Значение правила</returns>
        public string Get(RuleType rule)
        {
            return rules.ContainsKey(rule) ? rules[rule] : null;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[{this.Name}]");
            foreach (var (rule, value) in rules)
            {
                builder.AppendLine($"{rule} = {value}");
            }

            return builder.ToString();
        }

    }
}