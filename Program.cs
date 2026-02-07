using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GCI
{
    class Program
    {
        static void Main(string[] args)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var config = deserializer.Deserialize<GciConfig>(File.ReadAllText("config.yml"));

            if (GciLogin(config))
            {
                Console.WriteLine("Login successful!");
            }
            else
            {
                Console.WriteLine("Login failed.");
            }
        }

        static bool GciLogin(GciConfig config)
        {
            string stoneName = config.StoneName;
            string hostUserId = config.HostUserId;
            string hostPassword = config.HostPassword;
            string gemService = config.GemService;
            string gsUserName = config.GsUserName;
            string gsPassword = config.GsPassword;

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GEMSTONE")))
            {
                Console.Error.WriteLine("ERROR: GEMSTONE environment variable is not set.");
                return false;
            }

            Console.WriteLine("Initializing Gci...");
            if (!GciWrapper.GciInit())
            {
                Console.WriteLine("GciInit failed");
                return false;
            }

            GciWrapper.GciSetNet(stoneName, hostUserId, hostPassword, gemService);

            if (!GciWrapper.GciLogin(gsUserName, gsPassword))
            {
                Console.WriteLine("GciLogin failed");
                return false;
            }
            
            return true;
        }

    }
}
