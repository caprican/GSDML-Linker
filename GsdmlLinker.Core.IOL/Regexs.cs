using System.Text.RegularExpressions;

namespace GsdmlLinker.Core.IOL;

public static partial class Regexs
{
    [GeneratedRegex(@"(\d)\.\.(\d)")]
    public static partial Regex RegexSubslots();

    [GeneratedRegex(@"(\d{4})(\d{2})(\d{2})")]
    public static partial Regex DateRegex();

    [GeneratedRegex(@"(\d{2})(\d{2})(\d{2})")]
    public static partial Regex TimeRegex();

    //[GeneratedRegex(@"(\w+)-(.+?)-IODD([1-9]\.\d+)(?(-(\w+)))", RegexOptions.IgnoreCase, "en-US")]
    [GeneratedRegex(@"(\w+)-(.+?)-(\d{8})-IODD([1-9]\.\d(\.\d)?)(-\w+)?(-.+)?", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex FileNameRegex();
}
