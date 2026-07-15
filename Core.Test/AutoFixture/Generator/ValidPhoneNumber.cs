using System.Text.RegularExpressions;

namespace TSpec.Test.AutoFixture.Primitives;

public static partial class ValidPhoneNumber
{
    private static readonly Regex _phoneNumberE164Regex = PhoneNumberE164Regex();
    public static bool Verify(string email) => _phoneNumberE164Regex.IsMatch(email);

    [GeneratedRegex(@"^\+[1-9]\d{1,14}$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PhoneNumberE164Regex();
}