namespace LiPi.Components;

public enum ButtonSize
{
    Small,   // 28px height, 12px h-padding, 12px font, 64px min-width
    Medium,  // 32px height, 16px h-padding, 13px font, 80px min-width
    Large    // 40px height, 20px h-padding, 14px font, 96px min-width
}

public enum ButtonVariant
{
    Primary,    // Filled brand, sh-sm default + sh-md hover
    Secondary,  // 1px border, transparent bg (blends with container)
    Danger,     // Filled red, sh-sm default + sh-md hover
    Ghost,      // Transparent, hover bg only
    Link        // Text-only, underline on hover
}
