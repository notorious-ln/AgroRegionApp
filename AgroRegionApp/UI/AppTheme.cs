using System.Drawing;

namespace AgroRegionApp.UI
{
    internal static class AppTheme
    {
        public static readonly Color Navy = Color.FromArgb(30, 53, 88);
        public static readonly Color Blue = Color.FromArgb(46, 117, 182);
        public static readonly Color BlueHover = Color.FromArgb(37, 63, 102);
        public static readonly Color SidebarText = Color.FromArgb(184, 205, 224);
        public static readonly Color SidebarMuted = Color.FromArgb(127, 168, 204);
        public static readonly Color SidebarBorder = Color.FromArgb(46, 82, 128);
        public static readonly Color ContentBg = Color.FromArgb(240, 242, 245);
        public static readonly Color CardBg = Color.White;
        public static readonly Color Border = Color.FromArgb(192, 199, 208);
        public static readonly Color BorderLight = Color.FromArgb(209, 213, 219);
        public static readonly Color TextPrimary = Color.FromArgb(30, 53, 88);
        public static readonly Color TextBody = Color.FromArgb(55, 65, 81);
        public static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
        public static readonly Color TextLight = Color.FromArgb(156, 163, 175);
        public static readonly Color StatusBarBg = Color.FromArgb(229, 231, 235);
        public static readonly Color GridHeader = Color.FromArgb(238, 242, 247);
        public static readonly Color GridAlt = Color.FromArgb(248, 249, 250);
        public static readonly Color GridSelect = Blue;
        public static readonly Color ConnectedGreen = Color.FromArgb(22, 163, 74);
        public static readonly Color Danger = Color.FromArgb(220, 38, 38);
        public static readonly Color TabInactive = Color.FromArgb(238, 242, 247);
        public static readonly Color HintBg = Color.FromArgb(239, 246, 255);
        public static readonly Color HintBorder = Color.FromArgb(191, 219, 254);
        public static readonly Color WarnBg = Color.FromArgb(255, 251, 235);
        public static readonly Color WarnBorder = Color.FromArgb(253, 230, 138);
        public static readonly Color SuccessBg = Color.FromArgb(240, 253, 244);
        public static readonly Color SuccessBorder = Color.FromArgb(187, 247, 208);
        public static readonly Color SuccessText = Color.FromArgb(21, 128, 61);

        public static readonly Font FontUi = new Font("Segoe UI", 8.25f);
        public static readonly Font FontUiBold = new Font("Segoe UI", 8.25f, FontStyle.Bold);
        public static readonly Font FontTitle = new Font("Segoe UI", 9f, FontStyle.Bold);
        public static readonly Font FontSection = new Font("Segoe UI", 9.75f, FontStyle.Bold);
    }
}
