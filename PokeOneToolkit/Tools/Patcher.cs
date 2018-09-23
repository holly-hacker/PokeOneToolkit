using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using PokeOneToolkit.Helpers;
using PokeOneToolkit.Runtime;

namespace PokeOneToolkit.Tools
{
    internal class Patcher
    {
        public void Run()
        {
            string launcherPath = NotQuiteIOC.LauncherPath;
            string psxFolder = Path.Combine(launcherPath, "files", "PokeOne_Data", "Managed");
            string psxBackupPath = Path.Combine(psxFolder, "PSXAPI.dll.bak");
            string psxOrigPath = Path.Combine(psxFolder, "PSXAPI.dll");
            string psxReadPath = File.Exists(psxBackupPath) ? psxBackupPath : psxOrigPath;
            if (!File.Exists(psxReadPath)) throw new FileNotFoundException($"File {psxReadPath} does not exist");

            var rtMod = AssemblyDef.Load(Constants.RuntimeDllPath).Modules[0];
            var rtType = rtMod.Types.Single(x => x.FullName == typeof(Handler).FullName);
            var methodRecv = rtType.FindMethod(nameof(Handler.HandleRecv));
            var methodSend = rtType.FindMethod(nameof(Handler.HandleSend));

            var psxMod = AssemblyDef.Load(psxReadPath).Modules[0]; // load the original DLL
            var connection = psxMod.Types.Single(x => x.FullName == "PSXAPI.Connection");
            var connRecv = connection.FindMethod("doRead");
            var connSend = connection.FindMethod("Send");

            rtMod.Types.Remove(rtType);
            psxMod.AddAsNonNestedType(rtType);

            Instruction jumpTarget;
            connSend.Body.Instructions.Insert(6, jumpTarget = Instruction.Create(OpCodes.Ldarga_S, connSend.Parameters[1]));    // param 0 is `this`
            connSend.Body.Instructions.Insert(7, Instruction.Create(OpCodes.Call, methodSend));
            connSend.Body.Instructions[4].Operand = jumpTarget;

            connRecv.Body.Instructions.Insert(124, Instruction.Create(OpCodes.Ldloca_S, (Local)connRecv.Body.Instructions[118].Operand));
            connRecv.Body.Instructions.Insert(125, Instruction.Create(OpCodes.Call, methodRecv));
            connRecv.Body.ExceptionHandlers[0].TryStart = connRecv.Body.Instructions[124];

            if (!File.Exists(psxBackupPath))
                File.Move(psxReadPath, psxBackupPath);

            psxMod.Assembly.Write(psxOrigPath);
        }
    }
}
