using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Mobility.Features;
using UnityEngine;

namespace Mobility.Patches;


[HarmonyPatch(typeof(RigidbodyControllerC))]
internal static class RigidbodyControllerCPatches
{
    private const float SprintSpeedMultiplier = 1.5f;
    private static float _originalSpeed;
    private static float _originalMaxVelocityChange;
    private static bool _didSprint = false;
    private static Canvas? _staminaCanvas;

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void SetupSprinting(RigidbodyControllerC __instance)
    {
        _originalSpeed = __instance.speed;
        _originalMaxVelocityChange = __instance.maxVelocityChange;
        __instance.canJump = true;
        Stamina.ResetStamina();
    }
    
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void AddStaminaUI(RigidbodyControllerC __instance)
    {
        if (!Mobility.EnableStaminaSystem!.Value || !Mobility.EnableStaminaBar!.Value) return;
        if (_staminaCanvas != null) return;
        _staminaCanvas = new GameObject("StaminaCanvas").AddComponent<Canvas>();
        _staminaCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        _staminaCanvas.worldCamera = Camera.main;
        _staminaCanvas.gameObject.AddComponent<StaminaBar>();
    }

    [HarmonyPatch("FixedUpdate")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> InjectSprinting(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchForward(instruction => instruction.opcode == OpCodes.Dup)
            .Advance(-2)
            .Insert(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(RigidbodyControllerCPatches), nameof(HandleSprinting))))
            .InstructionEnumeration();
    }

    private static void HandleSprinting(RigidbodyControllerC instance)
    {
        if ((Stamina.CanSprint() || !Mobility.EnableStaminaSystem!.Value) && Input.GetKey(KeyCode.LeftShift))
        {
            instance.maxVelocityChange = _originalMaxVelocityChange * SprintSpeedMultiplier;
            instance.speed = _originalSpeed * SprintSpeedMultiplier;
            _didSprint = true;
        }
        else
        {
            instance.maxVelocityChange = _originalMaxVelocityChange;
            instance.speed = _originalSpeed;
        }
    }
    
    [HarmonyPatch("FixedUpdate")]
    [HarmonyPostfix]
    private static void HandleStamina(RigidbodyControllerC __instance)
    {
        Stamina.HandleTick(_didSprint);
        _didSprint = false;
    }

    [HarmonyPatch("FixedUpdate")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> EnableJump(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchForward(instruction => instruction.OperandIs("Jump"))
            .RemoveInstruction()
            .Advance(-2)
            .SearchForward(instruction => instruction.opcode == OpCodes.Call)
            .SetInstruction(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(RigidbodyControllerCPatches), nameof(CanJump))))
            .InstructionEnumeration();
    }

    private static bool CanJump()
    {
        return Input.GetKey(KeyCode.Space);
    }
}