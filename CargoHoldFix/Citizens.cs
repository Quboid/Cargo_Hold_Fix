//using Harmony;
//using System.Collections.Generic;
//using System.Reflection.Emit;
//using UnityEngine;

//namespace CargoHoldFix
//{
//    [HarmonyPatch(typeof(HumanAI))]
//    [HarmonyPatch("SimulationStep")]
//    [HarmonyPatch(new[] { typeof(ushort), typeof(CitizenInstance), typeof(CitizenInstance.Frame), typeof(bool) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal })]
//    public class HAI_SimulationStep
//    {
//        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//        {
//            var codes = new List<CodeInstruction>(instructions);
//            var newCodes = new List<CodeInstruction>();
//            //CodeInstruction ci;

//            string msg = $"ILCODE HumanAI (before)\nLines: {codes.Count}\n";
//            //for (int i = 320; i < 360; i++)
//            ////for (int i = 0; i < codes.Count; i++)
//            //{
//            //    ci = codes[i];
//            //    msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
//            //}
//            //Debug.Log($"{msg}");

//            newCodes.Add(codes[0]);
//            newCodes.Add(codes[1]);
//            newCodes.Add(codes[2]);
//            int lineNo = 3;

//            // Include lines until pattern is found
//            while (!(codes[lineNo - 2].opcode == OpCodes.Call && codes[lineNo - 1].opcode == OpCodes.Ldflda && codes[lineNo].opcode == OpCodes.Ldc_I4_2))
//            {
//                newCodes.Add(codes[lineNo++]);
//                if (lineNo >= (codes.Count - 5))
//                {
//                    Debug.Log($"HAI Code not found - already patched?");
//                    return codes;
//                }
//            }
//            //Debug.Log($"CITIZENS ILCode FOUND - {lineNo}: {codes[lineNo].opcode} {codes[lineNo].operand}");

//            // Add new lines
//            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_2));
//            newCodes.Add(new CodeInstruction(OpCodes.Call, typeof(HAI_SimulationStep).GetMethod("GetWaitFactor")));

//            // Copy remaining lines, skipping the line that was found
//            for (int i = lineNo + 1; i < codes.Count; i++)
//            {
//                newCodes.Add(codes[i]);
//            }

//            //msg = $"ILCODE HumanAI (after patching)\nLines: {newCodes.Count}\n";
//            //for (int i = 330; i < 350; i++)
//            //{
//            //    ci = newCodes[i];
//            //    msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
//            //}
//            //Debug.Log($"{msg}");

//            return newCodes;
//        }

//        public static uint GetWaitFactor(ref CitizenInstance citizenData)
//        {
//            if (IsWaitingAtOutsideConnection(ref citizenData))
//            {
//                return (uint)CargoHoldFix.delayPassengers.value * 2u;
//            }
//            return 2u;
//        }

//        // Code by boformer
//        public static bool IsWaitingAtOutsideConnection(ref CitizenInstance citizenData)
//        {
//            // Waiting for sightseeing bus
//            if ((citizenData.m_flags & (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour)) != CitizenInstance.Flags.None)
//            {
//                return false;
//            }

//            var pathManager = PathManager.instance;
//            var netManager = NetManager.instance;

//            if (!pathManager.m_pathUnits.m_buffer[citizenData.m_path].GetPosition(citizenData.m_pathPositionIndex >> 1, out PathUnit.Position position))
//            {
//                return false;
//            }

//            int nodeId = netManager.m_segments.m_buffer[position.m_segment].m_startNode;
//            return netManager.m_nodes.m_buffer[nodeId].m_transportLine == 0;
//        }
//    }
//}
