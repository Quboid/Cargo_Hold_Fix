using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Harmony;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using System.Reflection.Emit;

namespace CargoHoldFix
{
    //[HarmonyPatch(typeof(AircraftAI))]
    //[HarmonyPatch("TrySpawn")]
    //public class AAI_TrySpawn
    //{
    //    static int spawned = 0;
    //    static int dummy = 0;

    //    public static bool Postfix(bool result, ushort vehicleID, ref Vehicle vehicleData)
    //    {
    //        if (result)
    //        {
    //            if (HasFlag(vehicleData, Vehicle.Flags.DummyTraffic))
    //            {
    //                dummy++;
    //            }
    //            else
    //            {
    //                spawned++;
    //            }
    //        }
    //        //if (!HasFlag(vehicleData, Vehicle.Flags.DummyTraffic))
    //        //{
    //        //    Debug.Log($"PLANES: {spawned} spawned ({dummy} dummies)\n#{vehicleID}: {vehicleData.m_transferSize} - {result}\n{vehicleData.m_flags}");
    //        //}

    //        return result;
    //    }

    //    public static bool HasFlag(Vehicle data, Vehicle.Flags flag)
    //    {
    //        return (data.m_flags & flag) == flag;
    //    }
    //}


    [HarmonyPatch(typeof(CargoPlaneAI))]
    [HarmonyPatch("SimulationStep")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public class CPAI_SimulationStep
    {
        //public static bool HasFlag(Vehicle data, Vehicle.Flags flag)
        //{
        //    return (data.m_flags & flag) == flag;
        //}

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction ci;

            var codes = new List<CodeInstruction>(instructions);

            int lineNo = 1;
            while (!(codes[lineNo].opcode == OpCodes.Ldc_I4_2 && codes[lineNo].operand == null))
            {
                lineNo++;
            }

            codes[lineNo].opcode = OpCodes.Ldc_I4;
            codes[lineNo].operand = CargoHoldFix.delayTrain.value * 2;
            codes[lineNo + 3].opcode = OpCodes.Cgt;

            string msg = $"ILCODE PLANES\nLines: {codes.Count}\n";
            for (int i = 0; i < codes.Count; i++)
            {
                ci = codes[i];
                msg += $"{ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            }
            Debug.Log($"{msg}");

            return codes;
        }
    }
}
