namespace Mobility.Features;

/// <summary>
///     Keeps track of the stamina system.
///     Sprinting consumes stamina, and stamina regenerates when not sprinting.
/// </summary>
internal static class Stamina
{
    public const float StaminaRegenerationRate = 0.1f;
    public const float StaminaConsumptionRate = 0.1f;
    public const float MaxStamina = 100f;
    public const int TicksUntilRegeneration = 200;
    public static float StaminaValue { get; private set; } = 100f;
    private static int TicksSinceLastSprint { get; set; }

    private static void RegenerateStamina()
    {
        if (StaminaValue < MaxStamina)
            StaminaValue += StaminaRegenerationRate;
        else
            StaminaValue = MaxStamina;
    }

    private static void ConsumeStamina()
    {
        if (StaminaValue > 0f)
            StaminaValue -= StaminaConsumptionRate;
        else
            StaminaValue = 0f;
    }

    private static void HandleSprint(bool didSprint)
    {
        if (didSprint)
        {
            ConsumeStamina();
            TicksSinceLastSprint = 0;
        }
        else
        {
            if (TicksSinceLastSprint < TicksUntilRegeneration)
                TicksSinceLastSprint++;
        }
    }

    public static bool CanSprint()
    {
        return StaminaValue > 0f;
    }

    private static bool CanRegenerate()
    {
        return TicksSinceLastSprint >= TicksUntilRegeneration;
    }

    public static void ResetStamina()
    {
        StaminaValue = MaxStamina;
        TicksSinceLastSprint = 0;
    }

    internal static void HandleTick(bool didSprint)
    {
        HandleSprint(didSprint);
        if (CanRegenerate())
            RegenerateStamina();
    }
}