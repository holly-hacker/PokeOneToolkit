using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PSXAPI;
using Req = PSXAPI.Request;
using Resp = PSXAPI.Response;

namespace PokeOneToolkit.Runtime
{
    public static class HandlerPsx
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        static HandlerPsx()
        {
            AllocConsole();
            Log("Allocated console");
        }

        public static void HandleSend(ref Proto proto)
        {
            switch (proto) {
                case Req.Ping _:
                case Req.Move _:
                case Req.GuildEmblem _:
                case Req.Login _:
                    return;

                case Req.ChatMessage msg: {
                    if (msg.Message.StartsWith(".talk" )) {
                        var g = new Guid(msg.Message.Substring(".talk ".Length));
                        proto = new Req.Talk { NpcID = g };
                    } else if (msg.Message.StartsWith(".run ")) {
                        proto = new Req.BattleRun() {RequestID = Convert.ToInt32(msg.Message.Substring(".run ".Length))};
                    } else if (msg.Message.StartsWith(".script ")) {
                        Guid g = new Guid(msg.Message.Substring(".script ".Length, 36));
                        string resp = msg.Message.Substring(".script ".Length + 36 + 1);
                        proto = new Req.Script {
                            ScriptID = g,
                            Response = resp,
                        };
                    }
                    break;
                }
                    
            }
            
            Log($"-> {proto._Name} {ToJsonString(proto)}");
        }
        
        public static void HandleRecv(ref Proto proto)
        {
            try {
                switch (proto) {
                    case Resp.Ping _:
                    case Resp.MapUsers _:
                    case Resp.GuildEmblem _:
                    case Resp.Battle _:
                    case Resp.InventoryPokemon _:
                        return;

                    case Resp.Login login:
                        // login.Battle = null;
                        // login.Username = "✈️";
                        login.SkinsUser.EquipedClothe = login.Equip.Clothe = 1;
                        login.SkinsUser.EquipedHat = login.Equip.Hat = 1;
                        login.Style.Hair = 0;
                        return;

                }

                Log($"<- {proto._Name} {ToJsonString(proto)}");
            } catch (Exception e) {
                Log(e.ToString());
                throw;
            }
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
