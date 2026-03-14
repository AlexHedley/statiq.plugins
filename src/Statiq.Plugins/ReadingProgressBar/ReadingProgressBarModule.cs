namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2023/adding-reading-progress-indicator
public class ReadingProgressBarModule : ParallelModule
{
    protected override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        if (input.Source.Extension == ".md")
        {
            var color = context.GetString("ProgressBarColor", "#0085A1");
            var html = GenerateProgressBarHtml(color);
            return Task.FromResult(input
                .Clone(new MetadataItems {
                    { "ReadingProgressBar", html }
                }).Yield());
        }
        return Task.FromResult(input.Yield());
    }

    private static string GenerateProgressBarHtml(string color)
    {
        return "<div id=\"reading-progress-bar\"></div>\n" +
               "<style>\n" +
               "#reading-progress-bar {\n" +
               "    position: fixed;\n" +
               "    top: 0;\n" +
               "    left: 0;\n" +
               "    width: 0%;\n" +
               "    height: 4px;\n" +
               $"    background-color: {color};\n" +
               "    z-index: 9999;\n" +
               "    transition: width 100ms linear;\n" +
               "}\n" +
               "</style>\n" +
               "<script>\n" +
               "window.onscroll = function() {\n" +
               "    var winScroll = document.body.scrollTop || document.documentElement.scrollTop;\n" +
               "    var height = document.documentElement.scrollHeight - document.documentElement.clientHeight;\n" +
               "    var scrolled = height > 0 ? (winScroll / height) * 100 : 0;\n" +
               "    var bar = document.getElementById('reading-progress-bar');\n" +
               "    if (bar) bar.style.width = scrolled + '%';\n" +
               "};\n" +
               "</script>";
    }
}
