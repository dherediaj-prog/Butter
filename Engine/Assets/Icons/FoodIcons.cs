namespace Engine.Assets.Icons;

public static class FoodIcons
{
    // Croissant precisely replicated from the reference image, cleaned as a unifilar asset.
    // Base 32x32 viewbox. Unifilar center-line (1px stroke), no double borders, precise curves.
    // Composed of 5 clean, distinct line segments for maximum precision.
    public const string Croissant = 
        // 1. Primary Unified Central Shape (Closed path with smoothly rounded vertices)
        "M 14,7 L 18,7 Q 19.5,7 20,8 L 22,20 Q 22.5,22 21,23.5 L 17.5,26.5 Q 16,28 14.5,26.5 L 11,23.5 Q 9.5,22 10,20 L 12,8 Q 12.5,7 14,7 Z " +
        
        // 2. Left Internal Curve (Defining segments, smooth Q curves)
        "M 11.5,10 L 7.5,11 Q 6,11.5 5.5,13 L 4.5,19 Q 4,21 6,22 L 10.5,22 " +
        
        // 3. Left Outer Tip Curve (Detailed smooth C curve for precision)
        "M 6.5,12 C 1.5,10.5 0.5,19 5.5,20.5 " +
        
        // 4. Right Internal Curve (Symmetric segment definition)
        "M 20.5,10 L 24.5,11 Q 26,11.5 26.5,13 L 27.5,19 Q 28,21 26,22 L 21.5,22 " +
        
        // 5. Right Outer Tip Curve (Symmetric symmetric tip definition)
        "M 25.5,12 C 30.5,10.5 31.5,19 26.5,20.5";
}