using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DebugSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    public class Session 
    {
        public bool Started { get; set; }
        public Dictionary<string, int> ClientSteps { get; set; }
        public Dictionary<string, string> ClientMessages { get; set; }
        public int HoldUntil { get; set; }

        public Session()
        {
            Started = false;
            ClientSteps = new Dictionary<string, int>();
            HoldUntil = 2;
        }

        public ProtocolResponse Process(ProtocolRequest req)
        {
            if (!ClientSteps.ContainsKey(req.ClientId))
            {
                if (Started)
                {
                    ClientSteps.Add(req.ClientId, 0);
                }
            }

            if (ClientSteps.Count < HoldUntil)
            {
                return ProtocolResponse.Hold();
            }

            if (!Started)
            {
                return ProtocolResponse.Hold();
            }

            var me = ClientSteps[req.ClientId];
            var furthestAhead = ClientSteps.Values.Max();
            var allInSync = ClientSteps.Values.All(x => x == ClientSteps.Values.First());

            if (me < furthestAhead || allInSync)
            {
                ClientSteps[req.ClientId]++;

                return ProtocolResponse.Continue();
            }

            return ProtocolResponse.Hold();
        }
    }

    public enum Action
    {
        Hold,
        Continue,
        Break
    }

    public class ProtocolRequest
    {
        public string ClientId { get; set; }
        public string Message { get; set; }
    }

    public class ProtocolResponse
    {
        public Action Action;

        public static ProtocolResponse Hold()
        {
            return new ProtocolResponse
            {
                Action = Action.Hold
            };
        }
        public static ProtocolResponse Continue()
        {
            return new ProtocolResponse
            {
                Action = Action.Continue
            };
        }
    }
}
