using System.Drawing.Printing;
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

            PatchPsx(psxFolder);
            PatchMap(psxFolder);
        }

        private void PatchPsx(string folder)
        {
            const string target = "PSXAPI";

            string backupPath = Path.Combine(folder, target + ".dll.bak");
            string origPath = Path.Combine(folder, target + ".dll");
            string readPath = File.Exists(backupPath) ? backupPath : origPath;
            if (!File.Exists(readPath)) throw new FileNotFoundException($"File {readPath} does not exist");

            var rtMod = AssemblyDef.Load(Constants.RuntimeDllPath).Modules[0];
            var rtType = rtMod.Types.Single(x => x.FullName == typeof(HandlerPsx).FullName);
            var methodRecv = rtType.FindMethod(nameof(HandlerPsx.HandleRecv));
            var methodSend = rtType.FindMethod(nameof(HandlerPsx.HandleSend));

            var mapMod = AssemblyDef.Load(readPath).Modules[0]; // load the original DLL
            var connection = mapMod.Types.Single(x => x.FullName == target + ".Connection");
            var connRecv = connection.FindMethod("doRead");
            var connSend = connection.FindMethod("Send");

            rtMod.Types.Remove(rtType);
            mapMod.AddAsNonNestedType(rtType);

            connSend.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarga_S, connSend.Parameters[1]));    // param 0 is `this`
            connSend.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, methodSend));

            connRecv.Body.Instructions.Insert(124, Instruction.Create(OpCodes.Ldloca_S, (Local)connRecv.Body.Instructions[118].Operand));
            connRecv.Body.Instructions.Insert(125, Instruction.Create(OpCodes.Call, methodRecv));
            connRecv.Body.ExceptionHandlers[0].TryStart = connRecv.Body.Instructions[124];

            if (!File.Exists(backupPath))
                File.Move(readPath, backupPath);

            mapMod.Assembly.Write(origPath);
        }

        private void PatchMap(string folder)
        {
            const string target = "MAPAPI";

            string backupPath = Path.Combine(folder, target + ".dll.bak");
            string origPath = Path.Combine(folder, target + ".dll");
            string readPath = File.Exists(backupPath) ? backupPath : origPath;
            if (!File.Exists(readPath)) throw new FileNotFoundException($"File {readPath} does not exist");

            var rtMod = AssemblyDef.Load(Constants.RuntimeDllPath).Modules[0];
            var rtType = rtMod.Types.Single(x => x.FullName == typeof(HandlerMap).FullName);
            var methodRecv = rtType.FindMethod(nameof(HandlerMap.HandleRecv));
            var methodSend = rtType.FindMethod(nameof(HandlerMap.HandleSend));

            var mapMod = AssemblyDef.Load(readPath).Modules[0]; // load the original DLL
            var connection = mapMod.Types.Single(x => x.FullName == target + ".Connection");
            var connRecv = connection.FindMethod("doRead");
            var connSend = connection.FindMethod("Send");

            rtMod.Types.Remove(rtType);
            mapMod.AddAsNonNestedType(rtType);

            connSend.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarga_S, connSend.Parameters[1]));    // param 0 is `this`
            connSend.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, methodSend));

            connRecv.Body.Instructions.Insert(123, Instruction.Create(OpCodes.Ldloca_S, (Local)connRecv.Body.Instructions[117].Operand));
            connRecv.Body.Instructions.Insert(124, Instruction.Create(OpCodes.Call, methodRecv));
            connRecv.Body.ExceptionHandlers[0].TryStart = connRecv.Body.Instructions[123];

            if (!File.Exists(backupPath))
                File.Move(readPath, backupPath);

            mapMod.Assembly.Write(origPath);
        }
    }
}
