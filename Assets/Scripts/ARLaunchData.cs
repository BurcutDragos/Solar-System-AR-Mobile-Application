using UnityEngine;

/// <summary>
/// Carries the selected celestial body's presentation data from a body display
/// scene (e.g. Mercury.unity) into the shared ARViewScreen scene. All fields are
/// project assets (Mesh, Material, AudioClip) so they survive the scene load.
/// </summary>
public static class ARLaunchData
{
    public static bool HasData;
    public static string BodyName = "Planet";
    public static Mesh Mesh;
    public static Material Material;
    public static AudioClip Sound;
    public static string ReturnScene = "PlanetsScreen";
    public static float SourceScale = 1f;

    /// <summary>The planet's axial tilt, taken from its transform rotation in the display scene.</summary>
    public static Quaternion Rotation = Quaternion.identity;

    /// <summary>PlanetAutoRotation.speedMultiplier from the display scene (drives spin speed).</summary>
    public static float SpeedMultiplier = 15000f;

    /// <summary>Whether the celestial body has a ring system (e.g. Saturn, Chiron).</summary>
    public static bool HasRings;

    /// <summary>Number of segments for procedurally generating the ring mesh.</summary>
    public static int RingSegments = 3;

    /// <summary>Inner radius of the ring.</summary>
    public static float RingInnerRadius = 0.7f;

    /// <summary>Thickness of the ring.</summary>
    public static float RingThickness = 0.5f;

    /// <summary>Material applied to the ring mesh.</summary>
    public static Material RingMaterial;
}
