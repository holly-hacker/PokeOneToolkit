using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using MAPAPI;
using Req = MAPAPI.Request;
using Resp = MAPAPI.Response;

namespace PokeOneToolkit.Runtime
{
    public static class HandlerMap
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        static HandlerMap()
        {
            AllocConsole();
            Log("Allocated console");
        }

        public static void HandleSend(ref Proto proto)
        {
            switch (proto) {
               case Req.Ping _:
                   return;
            }
            
            Log($">> {proto._Name} {ToJsonString(proto)}");
        }

        private static Dictionary<string, Resp.MapDump> _editedMapDumps = new Dictionary<string, Resp.MapDump>();

        public static void HandleRecv(ref Proto proto)
        {
            switch (proto) {
                case Resp.MapServerMap msm: {
                    if (_editedMapDumps.ContainsKey(msm.MapName))
                    {
                        Log("This map is already edited.");
                        return;
                    }
                    var md = Resp.MapDump.Deserialize(CompressionHelper.DecompressBytes(msm.MapData));
                    
                    foreach (var npc in md.NPCs.OrderBy(x => x.Settings.NPCName)) {
                        if (npc.Settings.LOS > 0) {
                            npc.Settings.NPCName += $" (LOS={npc.Settings.LOS})";
                            npc.Settings.LOS = 0;
                        }

                        string x;
                        switch (npc.Settings.Sprite) {
                                case 0: x = "invisible"; break;
                                case 10: x = "pokeball"; break;
                                default: x = $"<unknown({npc.Settings.Sprite})>"; break;
                        }
                        Log($"{npc.Settings.NPCName} ({npc.ID}): {x} at {{{npc.x}x{npc.y}x{npc.z}}} Enabled={npc.Settings.Enabled}");

                        // npc.Settings.Enabled = false;
                    }

                    int removed = md.NPCs.RemoveAll(x => x.Settings.Sprite == 9);
                    Log($"Removed {removed} cut trees");

                    msm.MapData = CompressionHelper.CompressBytes(Proto.Serialize(md));
                    _editedMapDumps.Add(msm.MapName, md);
                    Log("Edited mapdata!");
                    return;
                }
            }

            Log($"<< {proto._Name} {ToJsonString(proto)}");
        }

        private static void Log(string s)
        {
            // recreating sw is probably bad, but it only prints if it gets closed (not flushed)
            using (var sw = new StreamWriter(Console.OpenStandardOutput()))
                sw.WriteLine(s);
        }

        private static string ToJsonString(Proto p) => JsonConvert.SerializeObject(p, new JsonSerializerSettings {
            Converters = {new Newtonsoft.Json.Converters.StringEnumConverter()}
        });
    }
}
