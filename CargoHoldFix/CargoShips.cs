using Harmony;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;

namespace CargoHoldFix
{
    [HarmonyPatch(typeof(CargoShipAI))]
    [HarmonyPatch("SimulationStep")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public class CSAI_SimulationStep
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int lineNo = 1;
            while (!(codes[lineNo].opcode == OpCodes.Ldc_I4_2))
            {
                lineNo++;
                if (lineNo >= (codes.Count - 3))
                {
                    Debug.Log($"CSAI Code not found - already patched?");
                    return codes;
                }
            }

            //Debug.Log($"Line number: {lineNo}");

            codes[lineNo].opcode = OpCodes.Ldc_I4;
            codes[lineNo].operand = OptimisedOutsideConnections.delayShip.value * 4;
            codes[lineNo + 3].opcode = OpCodes.Cgt;

            return codes;
        }
    }


    //[HarmonyPatch(typeof(PassengerShipAI))]
    //[HarmonyPatch("CanLeave")]
    //public class PSAI_CanLeave
    //{
    //    public static bool Postfix(bool __result, ushort vehicleID, ref Vehicle vehicleData)
    //    {
    //        if (__result)
    //        {
    //            Random.InitState(System.DateTime.Now.Second);
    //            if (Random.Range(0, 80) == 0)
    //            {
    //                return true;
    //            }
    //            //Debug.Log($"Ship #{vehicleID}:{__result} - wait:{vehicleData.m_waitCounter}, tType:{vehicleData.m_transferType}, tSize:{vehicleData.m_transferSize}, tLine:{vehicleData.m_transportLine}");
    //        }
    //        return false;
    //    }
    //}
}
