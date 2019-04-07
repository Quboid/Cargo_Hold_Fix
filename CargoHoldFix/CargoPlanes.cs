using Harmony;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;

namespace CargoHoldFix
{
    [HarmonyPatch(typeof(CargoPlaneAI))]
    [HarmonyPatch("SimulationStep")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public class CPAI_SimulationStep
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //CodeInstruction ci;

            var codes = new List<CodeInstruction>(instructions);

            int lineNo = 1;
            while (!(codes[lineNo].opcode == OpCodes.Ldc_I4_2))
            {
                lineNo++;
            }

            codes[lineNo].opcode = OpCodes.Ldc_I4;
            codes[lineNo].operand = CargoHoldFix.delayTrain.value * 2;
            codes[lineNo + 3].opcode = OpCodes.Cgt;

            //string msg = $"ILCODE PLANES\nLines: {codes.Count}\n";
            //for (int i = 0; i < codes.Count; i++)
            //{
            //    ci = codes[i];
            //    msg += $"{ci.opcode}, {ci.operand ?? "null"} <{(ci.operand == null ? "null" : ci.operand.GetType().ToString())}>\n";
            //}
            //Debug.Log($"{msg}");

            return codes;
        }
    }
}
