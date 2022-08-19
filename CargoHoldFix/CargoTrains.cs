using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace CargoHoldFix
{
    [HarmonyPatch(typeof(CargoTrainAI))]
    [HarmonyPatch("SimulationStep")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public class CTAI_SimulationStep
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //CodeInstruction ci;

            var codes = new List<CodeInstruction>(instructions);

            //string msg = $"ILCODE TRAINS (before)\nLines: {codes.Count}\n";
            //for (int i = 0; i < codes.Count; i++)
            //{
            //    ci = codes[i];
            //    msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            //}
            //Debug.Log($"{msg}");

            int lineNo = 1;
            while (!(codes[lineNo].opcode == OpCodes.Ldc_I4_2 && codes[lineNo + 1].opcode == OpCodes.Call && codes[lineNo + 2].opcode == OpCodes.Ldc_I4_0))
            {
                lineNo++;
                if (lineNo >= (codes.Count - 3))
                {
                    Debug.Log($"CTAI Code not found - already patched?");
                    return codes;
                }
            }
            Debug.Log($"TRAINS ILCode FOUND - {lineNo}: {codes[lineNo].opcode} {codes[lineNo].operand}");

            codes[lineNo].opcode = OpCodes.Ldc_I4;
            codes[lineNo].operand = CargoHoldFix.delayTrain.value * 2;
            codes[lineNo + 3].opcode = OpCodes.Cgt;

            //msg = $"ILCODE TRAINS (after)\nLines: {codes.Count}\n";
            //for (int i = 0; i < codes.Count; i++)
            //{
            //    ci = codes[i];
            //    msg += $"{i}: {ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            //}
            //Debug.Log($"{msg}");

            return codes;
        }
    }
}
