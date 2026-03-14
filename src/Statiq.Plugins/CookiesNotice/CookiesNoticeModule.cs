using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Statiq.Plugins;

// https://github.com/madmurphy/cookies.js/
public class CookiesNoticeModule : ParallelModule
{
    private static readonly string _noticeTemplate = LoadNoticeTemplate();

    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        if (input.Destination.Extension == ".html")
        {
            var content = await input.GetContentStringAsync();
            var updatedContent = InjectCookiesNotice(content, context);
            return input.Clone(context.GetContentProvider(updatedContent, MediaTypes.Html)).Yield();
        }
        return input.Yield();
    }

    private static string LoadNoticeTemplate()
    {
        var assembly = typeof(CookiesNoticeModule).Assembly;
        var resourceName = $"{assembly.GetName().Name}.CookiesNotice.cookies-notice.html";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new System.IO.StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string InjectCookiesNotice(string html, IExecutionContext context)
    {
        var cookieName = SanitizeCookieName(context.GetString("CookiesNoticeName", "cookiesAccepted"));
        var cookieMessage = WebUtility.HtmlEncode(context.GetString("CookiesNoticeMessage", "This website uses Google Analytics, and its cookies."));
        var cookieButtonText = WebUtility.HtmlEncode(context.GetString("CookiesNoticeButtonText", "Understood"));

        var noticeHtml = _noticeTemplate
            .Replace("{{COOKIE_NAME}}", cookieName)
            .Replace("{{COOKIE_MESSAGE}}", cookieMessage)
            .Replace("{{COOKIE_BUTTON_TEXT}}", cookieButtonText);

        var bodyCloseIndex = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyCloseIndex >= 0)
        {
            return html.Insert(bodyCloseIndex, noticeHtml);
        }
        return html;
    }

    private static string SanitizeCookieName(string name) =>
        Regex.Replace(name, @"[^A-Za-z0-9_\-]", "_");
}

