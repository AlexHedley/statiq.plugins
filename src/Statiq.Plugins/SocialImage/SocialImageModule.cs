using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Statiq.Plugins;

// https://wellsb.com/csharp/aspnet/generate-images-statiq-imagesharp
/// <summary>
/// A module that generates a social share image for each blog post.
/// The generated image is saved to the output directory and the document's
/// <c>Image</c> metadata key is updated to point to it.
/// Processing is skipped for any document that already has an <c>Image</c>
/// frontmatter property set.
/// </summary>
public class SocialImageModule : ParallelModule
{
    private string _brandText = string.Empty;
    private string _fontFamily = string.Empty;
    private Color _backgroundColor = Color.FromRgb(30, 30, 30);
    private Color _titleColor = Color.White;
    private Color _brandColor = Color.FromRgb(180, 180, 180);
    private int _width = 1200;
    private int _height = 630;
    private string _outputPath = "images/social";

    /// <summary>Sets the brand text drawn at the bottom of the generated image.</summary>
    public SocialImageModule WithBrandText(string brandText)
    {
        _brandText = brandText;
        return this;
    }

    /// <summary>Sets the font family used for all text in the generated image.</summary>
    public SocialImageModule WithFontFamily(string fontFamily)
    {
        _fontFamily = fontFamily;
        return this;
    }

    /// <summary>Sets the background colour of the generated image.</summary>
    public SocialImageModule WithBackgroundColor(Color color)
    {
        _backgroundColor = color;
        return this;
    }

    /// <summary>Sets the colour used to draw the post title text.</summary>
    public SocialImageModule WithTitleColor(Color color)
    {
        _titleColor = color;
        return this;
    }

    /// <summary>Sets the colour used to draw the brand text.</summary>
    public SocialImageModule WithBrandColor(Color color)
    {
        _brandColor = color;
        return this;
    }

    /// <summary>Sets the dimensions of the generated image (defaults to 1200x630).</summary>
    public SocialImageModule WithDimensions(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        _width = width;
        _height = height;
        return this;
    }

    /// <summary>
    /// Sets the output directory path (relative to the site root) where images are saved.
    /// Defaults to <c>images/social</c>.
    /// </summary>
    public SocialImageModule WithOutputPath(string outputPath)
    {
        _outputPath = outputPath.Trim('/');
        return this;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        // Skip if the document already has an image set via frontmatter
        var existingImage = input.GetString(WebKeys.Image, string.Empty);
        if (!string.IsNullOrEmpty(existingImage))
        {
            return input.Yield();
        }

        var title = input.GetString("Title", string.Empty);
        if (string.IsNullOrEmpty(title))
        {
            return input.Yield();
        }

        // Resolve brand text: module config → appsettings BrandText → appsettings SiteTitle
        var brandText = !string.IsNullOrEmpty(_brandText)
            ? _brandText
            : context.GetString("BrandText", context.GetString("SiteTitle", string.Empty));

        // Resolve font family: module config → appsettings SocialImageFont → first available system font
        var fontFamily = !string.IsNullOrEmpty(_fontFamily)
            ? _fontFamily
            : context.GetString("SocialImageFont", string.Empty);

        // Derive the image file name from the source document
        var sourceName = input.Source.IsNullOrEmpty
            ? Guid.NewGuid().ToString("N")
            : input.Source.FileNameWithoutExtension.ToString();

        var imageFileName = $"{sourceName}.png";
        var imageRelativePath = $"{_outputPath}/{imageFileName}";

        var imageBytes = GenerateSocialImage(title, brandText, fontFamily);

        // Write the image directly to the output file system
        var outputFile = context.FileSystem.GetOutputFile(imageRelativePath);
        await using (var stream = outputFile.OpenWrite())
        {
            await stream.WriteAsync(imageBytes);
        }

        // Return the document with the Image metadata updated
        return input.Clone(new MetadataItems
        {
            { WebKeys.Image, "/" + imageRelativePath }
        }).Yield();
    }

    private byte[] GenerateSocialImage(string title, string brandText, string fontFamily)
    {
        var resolvedFont = ResolveFont(fontFamily);

        using var image = new Image<Rgba32>(_width, _height);

        image.Mutate(ctx =>
        {
            ctx.Fill(_backgroundColor);

            var titleFont = resolvedFont.CreateFont(60, FontStyle.Bold);
            var brandFont = resolvedFont.CreateFont(30, FontStyle.Regular);

            var titleOptions = new RichTextOptions(titleFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Origin = new PointF(80, 160),
                WrappingLength = _width - 160
            };
            ctx.DrawText(titleOptions, title, _titleColor);

            if (!string.IsNullOrEmpty(brandText))
            {
                var brandOptions = new RichTextOptions(brandFont)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Origin = new PointF(80, _height - 80)
                };
                ctx.DrawText(brandOptions, brandText, _brandColor);
            }
        });

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    private static FontFamily ResolveFont(string fontFamily)
    {
        // Try the requested font first
        if (!string.IsNullOrEmpty(fontFamily) && SystemFonts.TryGet(fontFamily, out var requested))
        {
            return requested;
        }

        // Try common cross-platform fallbacks
        foreach (var candidate in new[] { "Arial", "Liberation Sans", "DejaVu Sans", "Verdana", "Helvetica", "Ubuntu" })
        {
            if (SystemFonts.TryGet(candidate, out var fallback))
            {
                return fallback;
            }
        }

        // Use the first available system font
        var families = SystemFonts.Families.ToList();
        if (families.Count > 0)
        {
            return families[0];
        }

        throw new InvalidOperationException(
            "No system fonts are available. Install fonts on the host or configure a font family path.");
    }
}
