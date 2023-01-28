using TMPro;

namespace RCG2_L10n
{
    public struct FontReplacement
    {
        public TMP_FontAsset FailbackFont { get; set; }
        public bool Replaced { get; set; }

        public FontReplacement(TMP_FontAsset font, bool replaced) {
            FailbackFont = font;
            Replaced = replaced;
        }
    }
}
