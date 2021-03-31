using System;
using System.Linq;
using System.Reflection;

namespace Netch.Models
{
    public abstract class ParameterBase
    {
        //     null value par

        private readonly bool _full;
        protected readonly string FullPrefix = "--";

        protected readonly string ParametersSeparate = " ";
        protected readonly string Separate = " ";
        protected readonly string VerbPrefix = "-";

        protected ParameterBase()
        {
            _full = !GetType().IsDefined(typeof(VerbAttribute));
        }

        public override string ToString()
        {
            var parameters = GetType().GetProperties().Select(PropToParameter).Where(s => s != null);
            return string.Join(ParametersSeparate, parameters).Trim();
        }

        private string? PropToParameter(PropertyInfo p)
        {
            // prefix
            bool full;
            if (p.IsDefined(typeof(VerbAttribute)))
            {
                full = false;
            }
            else if (p.IsDefined(typeof(FullAttribute)))
            {
                full = true;
            }
            else
            {
                full = _full;
            }

            var prefix = full ? FullPrefix : VerbPrefix;
            // key
            var key = p.GetCustomAttribute<RealNameAttribute>()?.Name ?? p.Name;

            // build
            var value = p.GetValue(this);
            switch (value)
            {
                case bool b:
                    return b ? $"{prefix}{key}" : null;
                default:
                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                    {
                        return p.IsDefined(typeof(OptionalAttribute)) ? null : throw new RequiredArgumentValueInvalidException(p.Name, this, null);
                    }

                    if (p.IsDefined(typeof(QuoteAttribute)))
                    {
                        value = $"\"{value}\"";
                    }

                    return $"{prefix}{key}{Separate}{value}";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class VerbAttribute : Attribute
    {
        // Don't use verb and full both on one class or property
        // if you did, will take verb
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class FullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class QuoteAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RealNameAttribute : Attribute
    {
        public RealNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [Serializable]
    public class RequiredArgumentValueInvalidException : Exception
    {
        private const string DefaultMessage = "{0}'s Argument \"{1}\" value invalid. A required argument's value can't be null or empty.";

        private readonly string? _message;

        public RequiredArgumentValueInvalidException()
        {
            _message = "Some Argument value invalid. A required argument value's can't be null or empty.";
        }

        public RequiredArgumentValueInvalidException(string argumentName, object argumentObject, string? message)
        {
            ArgumentName = argumentName;
            ArgumentObject = argumentObject;
            _message = message;
        }

        public string? ArgumentName { get; }

        public object? ArgumentObject { get; }

        public override string Message => _message ?? string.Format(DefaultMessage, ArgumentObject!.GetType(), ArgumentName);
    }
}