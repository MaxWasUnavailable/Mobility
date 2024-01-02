using UnityEngine;

namespace Mobility.Features;

internal class StaminaBar : MonoBehaviour
{
    private const float TimeBeforeHide = 5f;
    private const int StaminaBarHeight = 15;
    private const int StaminaBarWidth = 180;
    private const int StaminaBarPadding = 5;
    private const int ScreenPadding = 50;
    private static float _timeSinceStaminaFull;

    private void OnGUI()
    {
        if (!ShouldDraw()) return;

        if (StaminaFull())
            _timeSinceStaminaFull += Time.deltaTime;
        else
            _timeSinceStaminaFull = 0f;

        var staminaPercentage = Mathf.Clamp01(Stamina.StaminaValue / Stamina.MaxStamina);

        var staminaBar = new Rect(ScreenPadding, Screen.height - ScreenPadding - StaminaBarHeight,
            StaminaBarWidth * staminaPercentage,
            StaminaBarHeight);
        var staminaBarBackground = new Rect(staminaBar.x - StaminaBarPadding, staminaBar.y - StaminaBarPadding,
            StaminaBarWidth + StaminaBarPadding * 2, staminaBar.height + StaminaBarPadding * 2);

        GUI.DrawTexture(staminaBarBackground, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f,
            new Color(1f, 1f, 0.78f, 1f), 1f, 1f);
        
        if (!(staminaPercentage > 0f)) return;
        GUI.color = new Color(0.71f, 0.53f, 0.57f, 1f);
        GUI.DrawTexture(staminaBar, Texture2D.whiteTexture);
    }

    private static bool StaminaFull()
    {
        return Stamina.StaminaValue >= Stamina.MaxStamina;
    }

    private static bool ShouldDraw()
    {
        if (!Mobility.EnableStaminaSystem!.Value || !Mobility.EnableStaminaBar!.Value) return false;
        return !StaminaFull() || !(_timeSinceStaminaFull >= TimeBeforeHide);
    }
}