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

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void SetupSprinting(RigidbodyControllerC __instance)
    {
        _originalSpeed = __instance.speed;
        _originalMaxVelocityChange = __instance.maxVelocityChange;
        __instance.canJump = true;
        Stamina.ResetStamina();
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
        var didSprint = false;

        if ((Stamina.CanSprint() || !Mobility.UseStaminaSystem!.Value) && Input.GetKey(KeyCode.LeftShift))
        {
            didSprint = true;
            instance.maxVelocityChange = _originalMaxVelocityChange * SprintSpeedMultiplier;
            instance.speed = _originalSpeed * SprintSpeedMultiplier;
        }
        else
        {
            instance.maxVelocityChange = _originalMaxVelocityChange;
            instance.speed = _originalSpeed;
        }

        Stamina.HandleTick(didSprint);
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