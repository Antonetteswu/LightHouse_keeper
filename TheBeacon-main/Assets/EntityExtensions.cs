// PSEUDOCODE / PLAN:
// 1. Provide an extension method `EnableMovementAndJump(this Entity entity, bool enabled)`
//    so existing calls from Animator events compile even if the original `Entity` class
//    does not implement that method.
// 2. In the extension method:
//    - If `entity` is null, log a warning and return.
//    - Try to invoke obvious instance methods on `Entity` that accept a bool:
//        "EnableMovement", "EnableJump" (invoke both if present).
//    - If the above methods are not present, attempt to set common boolean properties/fields:
//        movement candidates: "CanMove", "canMove", "MovementEnabled", "movementEnabled", "isMovable"
//        jump candidates: "CanJump", "canJump", "JumpEnabled", "jumpEnabled", "isJumpable"
//    - If neither methods nor fields/properties are found for either movement or jump, log a warning.
//    - This approach uses reflection so we don't require the original `Entity` type to be modified
//      or marked `partial`. It preserves runtime behavior where possible.
// 3. Keep logging minimal but helpful using `Debug.LogWarning` for missing members.
// 4. Implement helper functions to reduce duplication and keep code readable.
//
// This file implements the extension method to fix CS1061 errors where `EnableMovementAndJump`
// is missing on `Entity`. The Animator event call sites (in `Entity_AnimationsEvents`) can
// keep calling `entity.EnableMovementAndJump(...)` and the extension will attempt to perform
// the appropriate action.

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class EntityExtensions
{
    /// <summary>
    /// Extension method to enable/disable movement and jump on an Entity.
    /// Attempts to call known methods or set common boolean fields/properties using reflection.
    /// </summary>
    public static void EnableMovementAndJump(this Entity entity, bool enabled)
    {
        if (entity == null)
        {
            Debug.LogWarning("[EntityExtensions] Entity is null in EnableMovementAndJump.");
            return;
        }

        var type = entity.GetType();

        // Try to invoke method(s) that accept a single bool parameter.
        bool movementInvoked = TryInvokeBoolMethod(type, entity, new[] { "EnableMovement", "SetMovementEnabled", "EnableMove", "SetCanMove" }, enabled);
        bool jumpInvoked = TryInvokeBoolMethod(type, entity, new[] { "EnableJump", "SetJumpEnabled", "EnableJumping", "SetCanJump" }, enabled);

        // If methods weren't found, try to set common fields/properties.
        if (!movementInvoked)
        {
            movementInvoked = TrySetBoolMember(type, entity, new[] { "CanMove", "canMove", "MovementEnabled", "movementEnabled", "isMovable" }, enabled);
        }

        if (!jumpInvoked)
        {
            jumpInvoked = TrySetBoolMember(type, entity, new[] { "CanJump", "canJump", "JumpEnabled", "jumpEnabled", "isJumpable" }, enabled);
        }

        if (!movementInvoked && !jumpInvoked)
        {
            Debug.LogWarning($"[EntityExtensions] No known movement/jump members found on '{type.FullName}'. Unable to {(enabled ? "enable" : "disable")} movement/jump.");
        }
    }

    private static bool TryInvokeBoolMethod(Type type, object instance, string[] methodNames, bool arg)
    {
        foreach (var name in methodNames)
        {
            var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
            if (method != null)
            {
                try
                {
                    method.Invoke(instance, new object[] { arg });
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[EntityExtensions] Exception invoking method '{name}' on '{type.FullName}': {ex.Message}");
                    return false;
                }
            }
        }

        return false;
    }

    private static bool TrySetBoolMember(Type type, object instance, string[] memberNames, bool value)
    {
        foreach (var name in memberNames)
        {
            // Try property first
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
            {
                try
                {
                    prop.SetValue(instance, value);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[EntityExtensions] Exception setting property '{name}' on '{type.FullName}': {ex.Message}");
                    return false;
                }
            }

            // Try field
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                try
                {
                    field.SetValue(instance, value);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[EntityExtensions] Exception setting field '{name}' on '{type.FullName}': {ex.Message}");
                    return false;
                }
            }
        }

        return false;
    }
}           