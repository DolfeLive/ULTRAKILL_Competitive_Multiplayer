using HarmonyLib;
using plog.Models;
using Steamworks;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Reflection.Emit;

namespace UltraIDK
{
    [HarmonyPatch(typeof(PostProcessV2_Handler), "SetupOutlines", new Type[] { typeof(bool) })]
    class PostProcessPatches
    {
        [HarmonyPrefix]
        public static bool OutlinePatch(
            bool forceOnePixelOutline,
            PostProcessV2_Handler __instance,
            ref CommandBuffer ___outlineCB,
            ref Material ___outlineMat,
            ref RenderTexture ___reusableBufferA,
            ref RenderTexture ___reusableBufferB,
            ref RenderTexture ___mainTex,
            ref Camera ___mainCam,
            ref int ___width,
            ref int ___height,
            ref bool ___isGLCore,
            ref OptionsManager ___oman,
            ref Shader ___outlinePx,
            ref int ___distance
        )
        {
            Debug.Log("Simplify: " + MonoSingleton<PrefsManager>.Instance.GetInt("simplifyEnemies", 0).ToString());
            Debug.Log("Distance: " + MonoSingleton<PrefsManager>.Instance.GetInt("simplifyEnemiesDistance", 0).ToString());
            Debug.Log("Thickness: " + MonoSingleton<PrefsManager>.Instance.GetInt("outlineThickness", 0).ToString());

            ___distance = MonoSingleton<PrefsManager>.Instance.GetInt("outlineThickness", 0);

            if (___mainCam == null)
            {
                __instance.SetupRTs();
                return false;
            }

            ___mainCam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);

            if (___mainTex == null)
            {
                __instance.SetupRTs();
                return false;
            }

            if (___isGLCore)
            {
                return false;
            }

            if (___outlineCB == null)
            {
                ___outlineCB = new CommandBuffer();
                ___outlineCB.name = "Outlines";
            }

            Vector2 vector = new Vector2((float)___mainTex.width, (float)___mainTex.height);
            Vector2 vector2 = vector / new Vector2((float)Screen.width, (float)Screen.height);
            float num = (float)___distance;
            if (___distance > 1)
            {
                num = (float)___distance * Mathf.Max(vector2.x, vector2.y);
            }

            ___outlineCB.Clear();

            if (___outlineMat == null)
            {
                ___outlineMat = new Material(___outlinePx);
            }

            ___outlineCB.SetGlobalVector("_Resolution", vector);
            ___outlineCB.SetGlobalVector("_ResolutionDiff", vector2);
            Mathf.CeilToInt((float)___width / 8f);
            Mathf.CeilToInt((float)___height / 8f);

            if (!forceOnePixelOutline && ___distance > 1 && num > 1f && ___oman.simplifyEnemies)
            {
                ___distance = Mathf.Min(___distance, 16);
                ___outlineCB.Blit(___reusableBufferA, ___reusableBufferB, ___outlineMat, 0);

                float num2 = 8f;
                int num3 = 0;
                while (num2 >= 0.5f || ___reusableBufferA.name == "Reusable B")
                {
                    ___outlineCB.SetGlobalFloat("_TestDistance", num2);
                    ___outlineCB.Blit(___reusableBufferB, ___reusableBufferA, ___outlineMat, 1);

                    RenderTexture temp = ___reusableBufferB;
                    ___reusableBufferB = ___reusableBufferA;
                    ___reusableBufferA = temp;
                    num2 *= 0.5f;
                    num3++;
                }

                ___outlineCB.SetGlobalFloat("_OutlineDistance", (float)___distance);
                ___outlineCB.SetGlobalFloat("_TestDistance", num2);
                ___outlineCB.Blit(___reusableBufferB, ___mainTex, ___outlineMat, 2);
                ___outlineCB.SetRenderTarget(___reusableBufferA);
                ___outlineCB.ClearRenderTarget(false, true, Color.black);
                ___outlineCB.SetRenderTarget(___reusableBufferB);
                ___outlineCB.ClearRenderTarget(false, true, Color.black);
            }
            else
            {
                int passToUse = Mathf.Min(2, ___outlineMat.passCount - 1);
                ___outlineCB.Blit(___reusableBufferA, ___mainTex, ___outlineMat, passToUse);
                ___outlineCB.SetRenderTarget(___reusableBufferA);
                ___outlineCB.ClearRenderTarget(false, true, Color.black);
            }

            ___mainCam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, ___outlineCB);

            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.name = "Clear Before Draw";
            commandBuffer.SetRenderTarget(___reusableBufferA);
            commandBuffer.ClearRenderTarget(false, true, Color.black);

            ___mainCam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);

            return false;
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Graphics))]
    class GraphicsBlitWildcard
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(UnityEngine.Graphics)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == "Blit");
        }

        static void Prefix(params object[] __args)
        {
            Debug.Log($"Graphics.Blit called. Args: {string.Join(", ", __args.Select(a => a?.ToString() ?? "null"))}");
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Material), "SetPass")]
    class Patch_Material_SetPass
    {
        static void Prefix(Material __instance, int pass)
        {
            if (__instance == null) return;

            if (pass >= __instance.passCount)
            {
                Debug.LogError($"[Material.SetPass] Invalid pass {pass} on material '{__instance.name ?? "null"}' with passCount={__instance.passCount}");
                Debug.LogError(UnityEngine.StackTraceUtility.ExtractStackTrace());
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Rendering.CommandBuffer), "Blit", new Type[] { typeof(Texture), typeof(RenderTargetIdentifier), typeof(Material), typeof(int) })]
    class Patch_CommandBuffer_Blit
    {
        static void Prefix(Texture source, UnityEngine.Rendering.RenderTargetIdentifier dest, Material mat, int pass)
        {
            if (mat != null && pass >= mat.passCount)
            {
                Debug.LogError($"[CommandBuffer.Blit] Invalid pass {pass} on material {mat.name}");
            }
        }
    }


    [HarmonyPatch(typeof(EndlessGrid), "LoadPattern")]
    public static class EndlessGrid_LoadPattern_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var getInstanceMethod = AccessTools.PropertyGetter(typeof(MonoSingleton<HudMessageReceiver>), "Instance");
            var sendHudMessageMethod = AccessTools.Method(typeof(HudMessageReceiver), "SendHudMessage",
                new[] { typeof(string), typeof(string), typeof(string), typeof(int), typeof(bool), typeof(bool), typeof(bool) });

            var debugLogMethod = AccessTools.Method(typeof(Debug), "Log", new[] { typeof(object) });

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand as MethodInfo == getInstanceMethod)
                {
                    int startRemove = i;
                    int endRemove = i;

                    while (endRemove < codes.Count)
                    {
                        if (codes[endRemove].opcode == OpCodes.Callvirt && codes[endRemove].operand as MethodInfo == sendHudMessageMethod)
                            break;

                        endRemove++;
                    }

                    if (endRemove < codes.Count)
                        endRemove++;

                    codes.RemoveRange(startRemove, endRemove - startRemove);

                    codes.InsertRange(startRemove, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_1), // Load local variable 'text'
                    new CodeInstruction(OpCodes.Call, debugLogMethod), // Debug.Log(object)
                });

                    break;
                }
            }
            return codes;
        }
    }

    [HarmonyPatch(typeof(EndlessGrid), "Update")]
    class EndlessUpdatePatches
    {
        [HarmonyPrefix]
        public static void UpdatePatch()
        {
            if (CompMultiplayerMain.instance.inMultiplayerScene)
            {
                return;
                Debug.Log("prevented org Endless");
            }
        }
    }

    [HarmonyPatch(typeof(ActivateNextWave), "FixedUpdate")]
    class NextWavePatches
    {
        [HarmonyPrefix]
        public static void UpdatePatch()
        {
            if (CompMultiplayerMain.instance.inMultiplayerScene)
            {
                return;
                Debug.Log("prevented org Endless");
            }
        }
    }

    [HarmonyPatch(typeof(EndlessGrid), "NextWave")]
    class nwPatch
    {
        [HarmonyPrefix]
        public static void nwPatch22()
        {
            return;
            if (CompMultiplayerMain.instance.inMultiplayerScene)
            {

            }
        }
    }




    /*
    [HarmonyPatch]
    class InstantiatePatches
    {
        // Basic Instantiate(Object)
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object) })]
        class InstantiateBasicPatch
        {
            static void Postfix(UnityEngine.Object __result)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"GameObject instantiated: {resultObj.name}");
                }
            }
        }

        // Instantiate(Object, Vector3, Quaternion)
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
            new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
        class InstantiateWithPositionRotationPatch
        {
            static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"GameObject instantiated with position {position} and rotation {rotation}: {resultObj.name}");
                }
            }
        }

        // Instantiate(Object, Vector3, Quaternion, Transform)
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
            new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
        class InstantiateWithPositionRotationParentPatch
        {
            static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation, Transform parent)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"GameObject instantiated with position {position}, rotation {rotation}, and parent {parent?.name ?? "null"}: {resultObj.name}");
                }
            }
        }

        // Instantiate(Object, Transform)
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
            new Type[] { typeof(UnityEngine.Object), typeof(Transform) })]
        class InstantiateWithParentPatch
        {
            static void Postfix(UnityEngine.Object __result, Transform parent)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"GameObject instantiated with parent {parent?.name ?? "null"}: {resultObj.name}");
                }
            }
        }

        // Instantiate(Object, Transform, bool)
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
            new Type[] { typeof(UnityEngine.Object), typeof(Transform), typeof(bool) })]
        class InstantiateWithParentAndWorldSpacePatch
        {
            static void Postfix(UnityEngine.Object __result, Transform parent, bool instantiateInWorldSpace)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"GameObject instantiated with parent {parent?.name ?? "null"} and instantiateInWorldSpace={instantiateInWorldSpace}: {resultObj.name}");
                }
            }
        }

        // Basic generic Instantiate<T>
        [HarmonyPatch(typeof(UnityEngine.Object))]
        [HarmonyPatch("Instantiate", typeof(UnityEngine.Object))]
        class InstantiateGenericBasicPatch
        {
            static void Postfix(UnityEngine.Object __result)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"Generic GameObject instantiated: {resultObj.name}");
                }
            }
        }

        // Generic Instantiate<T> with position and rotation
        [HarmonyPatch(typeof(UnityEngine.Object))]
        [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion))]
        class InstantiateGenericWithPositionRotationPatch
        {
            static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"Generic GameObject instantiated with position {position} and rotation {rotation}: {resultObj.name}");
                }
            }
        }

        // Generic Instantiate<T> with position, rotation and parent
        [HarmonyPatch(typeof(UnityEngine.Object))]
        [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform))]
        class InstantiateGenericWithPositionRotationParentPatch
        {
            static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation, Transform parent)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"Generic GameObject instantiated with position {position}, rotation {rotation}, and parent {parent?.name ?? "null"}: {resultObj.name}");
                }
            }
        }

        // Generic Instantiate<T> with parent
        [HarmonyPatch(typeof(UnityEngine.Object))]
        [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Transform))]
        class InstantiateGenericWithParentPatch
        {
            static void Postfix(UnityEngine.Object __result, Transform parent)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"Generic GameObject instantiated with parent {parent?.name ?? "null"}: {resultObj.name}");
                }
            }
        }

        // Generic Instantiate<T> with parent and world space
        [HarmonyPatch(typeof(UnityEngine.Object))]
        [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Transform), typeof(bool))]
        class InstantiateGenericWithParentAndWorldSpacePatch
        {
            static void Postfix(UnityEngine.Object __result, Transform parent, bool instantiateInWorldSpace)
            {
                if (__result is GameObject resultObj)
                {
                    UltraIDK.Logger.Log($"Generic GameObject instantiated with parent {parent?.name ?? "null"} and instantiateInWorldSpace={instantiateInWorldSpace}: {resultObj.name}");
                }
            }
        }
    }
    */
    // Add patches for: player speed, rocket riding, bullets, damage reciving, shotgun bullet location rng
    //
    //
}
