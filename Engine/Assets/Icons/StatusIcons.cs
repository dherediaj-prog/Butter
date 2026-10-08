namespace Engine.Assets.Icons;

public static class StatusIcons
{
    // Cerrar en Círculo / Close Circle / Error (Estructura base 32x32, Centerline 1px)
    public const string CloseCircle = 
        "M 16,4 A 12,12 0 1 0 16,28 A 12,12 0 1 0 16,4 Z " +
        "M 11.5,11.5 L 20.5,20.5 " +
        "M 20.5,11.5 L 11.5,20.5";

    // Advertencia / Warning Triangle (Estructura base 32x32, Centerline 1px)
    public const string Warning = 
        "M 16,4 A 2.5,2.5 0 0 1 18.2,5.2 L 28.5,23 A 2.5,2.5 0 0 1 26.3,26.8 H 5.7 A 2.5,2.5 0 0 1 3.5,23 L 13.8,5.2 A 2.5,2.5 0 0 1 16,4 Z " +
        "M 16,11 V 17 " +
        "M 16,21 V 21.5";

    // Información / Info Circle (Círculo con '!') (Estructura base 32x32, Centerline 1px)
    public const string InfoCircle = 
        "M 16,4 A 12,12 0 1 0 16,28 A 12,12 0 1 0 16,4 Z " +
        "M 16,10 V 17 " +
        "M 16,21 V 21.5";

    // Carga / Indicador de Progreso / Spinner (Estructura base 32x32, Centerline 1px)
    public const string Spinner = 
        "M 16,4 V 8 " +
        "M 16,24 V 28 " +
        "M 4,16 H 8 " +
        "M 24,16 H 28 " +
        "M 7.5,7.5 L 10.3,10.3 " +
        "M 24.5,7.5 L 21.7,10.3 " +
        "M 7.5,24.5 L 10.3,21.7 " +
        "M 24.5,24.5 L 21.7,21.7";
}