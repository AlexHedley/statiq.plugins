using System.Net;
using System.Text.RegularExpressions;

namespace Statiq.Plugins;

// https://github.com/madmurphy/cookies.js/
public class CookiesNoticeModule : ParallelModule
{
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

    private static string InjectCookiesNotice(string html, IExecutionContext context)
    {
        var cookieName = SanitizeCookieName(context.GetString("CookiesNoticeName", "cookiesAccepted"));
        var cookieMessage = WebUtility.HtmlEncode(context.GetString("CookiesNoticeMessage", "This website uses Google Analytics, and its cookies."));
        var cookieButtonText = WebUtility.HtmlEncode(context.GetString("CookiesNoticeButtonText", "Understood"));

        var noticeHtml = $@"<style>
#cookie-notice {{
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: #fff;
    border-top: 1px solid #dee2e6;
    padding: 1rem 1.5rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
    z-index: 9999;
}}
#cookie-notice p {{
    margin: 0;
}}
#cookie-notice-accept {{
    background-color: #6c757d;
    color: #fff;
    border: none;
    border-radius: 0.25rem;
    padding: 0.375rem 0.75rem;
    cursor: pointer;
    white-space: nowrap;
    margin-left: 1rem;
}}
#cookie-notice-accept:hover {{
    background-color: #5a6268;
}}
</style>
<div id=""cookie-notice"" style=""display:none;"">
    <p>{cookieMessage}</p>
    <button id=""cookie-notice-accept"">{cookieButtonText}</button>
</div>
<script src=""https://cdn.jsdelivr.net/gh/madmurphy/cookies.js@1.0.0/cookies.min.js""></script>
<script>
window.addEventListener('DOMContentLoaded', function () {{
    var notice = document.getElementById('cookie-notice');
    var btn = document.getElementById('cookie-notice-accept');
    if (notice && btn) {{
        if (!docCookies.hasItem('{cookieName}')) {{
            notice.style.display = 'flex';
        }}
        btn.addEventListener('click', function () {{
            docCookies.setItem('{cookieName}', 'true', Infinity);
            notice.style.display = 'none';
        }});
    }}
}});
</script>
";

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

