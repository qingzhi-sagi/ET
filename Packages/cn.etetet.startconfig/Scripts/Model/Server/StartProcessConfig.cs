using System.Net;

namespace ET.Server
{
    [EnableClass]
    public partial class StartProcessConfigCategory
    {
    }

    [EnableClass]
    public partial class StartMachineConfigCategory
    {
    }

    public partial class StartProcessConfig
    {
        public override void EndInit()
        {
        }
    }

    public static class StartProcessConfigExtensions
    {
        public static StartMachineConfig GetStartMachineConfig(this StartProcessConfig self, Fiber fiber)
        {
            return fiber.GetSingleton<StartMachineConfigCategory>().Get(self.MachineId);
        }

        public static string GetInnerIP(this StartProcessConfig self, Fiber fiber)
        {
            return self.GetStartMachineConfig(fiber).InnerIP;
        }

        public static string GetOuterIP(this StartProcessConfig self, Fiber fiber)
        {
            return self.GetStartMachineConfig(fiber).OuterIP;
        }

        public static IPEndPoint GetInnerIPInnerPort(this StartProcessConfig self, Fiber fiber)
        {
            return NetworkHelper.ToIPEndPoint($"{self.GetInnerIP(fiber)}:{self.Port}");
        }

        public static Address GetAddress(this StartProcessConfig self, Fiber fiber)
        {
            return new Address(self.GetInnerIP(fiber), self.Port);
        }
    }
}
