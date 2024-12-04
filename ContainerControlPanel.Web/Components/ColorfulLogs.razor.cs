using Microsoft.AspNetCore.Components;

namespace ContainerControlPanel.Web.Components;

public partial class ColorfulLogs
{
    [Parameter]
    public string Logs { get; set; } = string.Empty;

    private bool CheckForErrorMessage(string word)
    {
        return word.Contains("error")
            || word.Contains("exception")
            || word.Contains("fail")
            || word.Contains("fatal")
            || word.Contains("err");
    }

    private bool CheckForWarningMessage(string word)
    {
        return word.Contains("warn")
            || word.Contains("warning");
    }

    private bool CheckForSuccessMessage(string word)
    {
        return word.Contains("success");
    }
}