using ColossalFramework;
using ColossalFramework.UI;
using Harmony;
using ICities;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace CargoHoldFix
{
    [HarmonyPatch(typeof(CargoHoldFix_CitizenHandler.CitizenHandlerClass))]
    [HarmonyPatch("CitizenHandler")]
    public class CitizenHandlerILCode
    {
        public static List<CodeInstruction> ILCode = null;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ILCode = (List<CodeInstruction>)instructions;
            //return instructions;

            var codes = new List<CodeInstruction>(instructions);
            CodeInstruction ci;

            string msg = $"TESTILCODE\nLines: {codes.Count}\n";
            for (int i = 0; i < codes.Count; i++)
            {
                ci = codes[i];
                msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            }
            Debug.Log($"{msg}");

            return codes;
        }
    }

    [HarmonyPatch(typeof(HumanAI))]
    [HarmonyPatch("SimulationStep")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(CitizenInstance), typeof(CitizenInstance.Frame), typeof(bool) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal })]
    public class HAI_SimulationStep
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            CodeInstruction ci;

            string msg = $"ILCODE HumanAI (before)\nLines: {codes.Count}\n";
            for (int i = 320; i < 360; i++)
            //for (int i = 0; i < codes.Count; i++)
            {
                ci = codes[i];
                msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            }
            Debug.Log($"{msg}");

            newCodes.Add(codes[0]);
            newCodes.Add(codes[1]);
            newCodes.Add(codes[2]);
            int lineNo = 3;
            while (!(codes[lineNo - 2].opcode == OpCodes.Call && codes[lineNo - 1].opcode == OpCodes.Ldflda && codes[lineNo].opcode == OpCodes.Ldc_I4_2))
            {
                newCodes.Add(codes[lineNo++]);
                if (lineNo >= (codes.Count - 5))
                {
                    Debug.Log($"HAI Code not found - already patched?");
                    return codes;
                }
            }
            Debug.Log($"LINE FOUND - {lineNo}: {codes[lineNo].opcode} {codes[lineNo].operand}");
            //Debug.Log($"{typeof(HAI_SimulationStep).Assembly.GetType("CargoHoldFix.HAI_SimulationStep")}");
            //Debug.Log($"{typeof(HAI_SimulationStep).Assembly.GetType("CargoHoldFix.HAI_SimulationStep").GetMethod("IsWaitingAtOutsideConnection")}");

            //newCodes.Add(new CodeInstruction(OpCodes.Stloc_0));
            //newCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_2));
            //newCodes.Add(new CodeInstruction(OpCodes.Stloc_1));
            //newCodes.Add(new CodeInstruction(OpCodes.Ldloca_S, typeof(CitizenInstance)));
            //newCodes.Add(new CodeInstruction(OpCodes.Call, typeof(HAI_SimulationStep).Assembly.GetType("CargoHoldFix.HAI_SimulationStep").GetMethod("IsWaitingAtOutsideConnection")));
            //newCodes.Add(new CodeInstruction(OpCodes.Brfalse));

            //newCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_S, 10));
            //newCodes.Add(new CodeInstruction(OpCodes.Stloc_1));
            //newCodes.Add(codes[lineNo]);
            //newCodes.Add(codes[lineNo + 1]);
            //newCodes.Add(new CodeInstruction(OpCodes.Ldloc_1));
            //newCodes.Add(codes[lineNo + 3]);

            if (CitizenHandlerILCode.ILCode == null)
            {
                throw new NullReferenceException($"CitizenHandlerILCode.ILCode is Null");
            }

            for (int i = 0; i <= 1; i++)
            {
                newCodes.Add(CitizenHandlerILCode.ILCode[i]);
            }

            msg = $"ILCODE HumanAI (during)\nLines: {newCodes.Count}\n";
            for (int i = 320; i < newCodes.Count; i++)
            {
                ci = newCodes[i];
                msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            }
            Debug.Log($"{msg}");

            for (int i = lineNo + 1; i < codes.Count; i++)
            {
                newCodes.Add(codes[i]);
            }

            msg = $"ILCODE HumanAI (after)\nLines: {newCodes.Count}\n";
            for (int i = 320; i < 360; i++)
            //for (int i = 0; i < codes.Count; i++)
            {
                ci = newCodes[i];
                msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            }
            Debug.Log($"{msg}");

            return newCodes;
        }

        public static uint GetWaitFactor(ref CitizenInstance citizenData)
        {
            List<int> tagged = new List<int>
            {
                102800,
                102798,
                102796,
                104186
            };
            if (IsWaitingAtOutsideConnection(ref citizenData))
            {
                if (tagged.Contains(citizenData.Info.GetInstanceID()))
                    Debug.Log($"{citizenData.Info.GetInstanceID()} {citizenData.m_waitCounter}");
                return 5u;// 10u;
            }
            return 2u;
        }

        public static bool IsWaitingAtOutsideConnection(ref CitizenInstance citizenData)
        {
            // Waiting for sightseeing bus
            if ((citizenData.m_flags & (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour)) != CitizenInstance.Flags.None)
            {
                return false;
            }

            var pathManager = PathManager.instance;
            var netManager = NetManager.instance;

            if (!pathManager.m_pathUnits.m_buffer[citizenData.m_path].GetPosition(citizenData.m_pathPositionIndex >> 1, out PathUnit.Position position))
            {
                return false;
            }

            int nodeId = netManager.m_segments.m_buffer[position.m_segment].m_startNode;
            return netManager.m_nodes.m_buffer[nodeId].m_transportLine == 0;
        }
    }
}
