using System.ComponentModel;

namespace StashManagementHelper;

public enum SortEnum
{
    [Description("Sort by item size")]
    Size,

    [Description("Default EFT sorting")]
    Default,

    [Description("Sort using custom strategy defined...")]
    Custom
}