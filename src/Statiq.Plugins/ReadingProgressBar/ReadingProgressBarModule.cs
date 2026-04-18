using System.Reflection;

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
        var css = ReadEmbeddedResource("reading-progress-bar.css");
        var js = ReadEmbeddedResource("reading-progress-bar.js");

        return $"<div id=\"reading-progress-bar\"></div>\n" +
               $"<style>:root {{ --rpb-color: {color}; }}\n{css}</style>\n" +
               $"<script>\n{js}</script>";
    }

    private static string ReadEmbeddedResource(string filename)
    {
        var assembly = typeof(ReadingProgressBarModule).Assembly;
        var resourceName = $"Statiq.Plugins.ReadingProgressBar.{filename}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found in assembly '{assembly.FullName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
