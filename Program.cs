using System;

namespace GCI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (LoginExample())
            {
                Console.WriteLine("Login successful!");
            }
            else
            {
                Console.WriteLine("Login failed.");
            }
        }

        static bool LoginExample()
        {
            string stoneName = "guava_ops";
            string hostUserId = "";
            string hostPassword = "";
            string gemService = "!tcp@localhost#netldi:8080#task!gemnetobject";
            string gsUserName = "SystemUser";
            string gsPassword = "swordfish";

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
