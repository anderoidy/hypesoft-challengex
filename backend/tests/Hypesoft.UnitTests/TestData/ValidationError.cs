using System;

namespace Hypesoft.UnitTests.TestData
{
    public class ValidationError : IEquatable<ValidationError>
    {
        public string Identifier { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public Severity Severity { get; set; } = Severity.Error;

        public bool Equals(ValidationError other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return string.Equals(Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ErrorMessage, other.ErrorMessage) &&
                   string.Equals(ErrorCode, other.ErrorCode) &&
                   Severity == other.Severity;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ValidationError);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(Identifier ?? string.Empty),
                ErrorMessage?.GetHashCode() ?? 0,
                ErrorCode?.GetHashCode() ?? 0,
                Severity);
        }
    }

    public enum Severity
    {
        Error = 0,
        Warning = 1,
        Info = 2
    }
}
