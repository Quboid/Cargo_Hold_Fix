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
    //[HarmonyPatch(typeof(ShipAI))]
    //[HarmonyPatch("TrySpawn")]
    //public class SAI_TrySpawn
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
    //        //    Debug.Log($"SHIPS: {spawned} spawned ({dummy} dummies)\n#{vehicleID}: {vehicleData.m_transferSize} - {result}\n{vehicleData.m_flags}");
    //        //}

    //        return result;
    //    }

    //    public static bool HasFlag(Vehicle data, Vehicle.Flags flag)
    //    {
    //        return (data.m_flags & flag) == flag;
    //    }
    //}


    [HarmonyPatch(typeof(CargoShipAI))]
    [HarmonyPatch("SimulationStep")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public class CSAI_SimulationStep
    {
        public static void Postfix(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos, int ___m_cargoCapacity)
        {
            //if ((vehicleID >= 0 && vehicleID <= ushort.MaxValue) && HasFlag(data, Vehicle.Flags.Created) && (HasFlag(data, Vehicle.Flags.WaitingCargo) || HasFlag(data, Vehicle.Flags.Spawned)))
            //{
            //    Debug.Log($"#{vehicleID} {data.m_waitCounter}, {data.m_transferSize}/{___m_cargoCapacity} - {HasFlag(data, Vehicle.Flags.WaitingCargo)}\n{data.m_flags}");
            //}
        }

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

            string msg = $"ILCODE SHIPS\nLines: {codes.Count}\n";
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
