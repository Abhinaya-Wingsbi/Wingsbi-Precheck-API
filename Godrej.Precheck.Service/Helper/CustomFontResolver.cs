using System;
using System.IO;
using System.Reflection;
using PdfSharp.Fonts;

namespace Godrej.Precheck.Service.Helper
{
    public class CustomFontResolver : IFontResolver
    {
        private const string FONT_RESOURCE_PATH = "Godrej.Precheck.Service.Resources.Fonts.arial.ttf";
        private byte[] _fontData;

        public CustomFontResolver()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(FONT_RESOURCE_PATH))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Font resource not found: {FONT_RESOURCE_PATH}");
                }
                
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    _fontData = ms.ToArray();
                }
            }
        }

        public byte[] GetFont(string faceName)
        {
            return _fontData;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Always return our Arial font regardless of the requested family
            return new FontResolverInfo("arial.ttf");
        }
    }
} 