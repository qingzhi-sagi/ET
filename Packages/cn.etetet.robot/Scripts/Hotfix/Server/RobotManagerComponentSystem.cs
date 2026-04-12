using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(RobotManagerComponent))]
    public static partial class RobotManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RobotManagerComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this RobotManagerComponent self)
        {
            async ETTask Remove(int f)
            {
                await self.Fiber().RemoveFiber(f);
            }
            
            foreach (int fiberId in self.robots.Values)
            {
                Remove(fiberId).Coroutine();
            }
        }

        public static bool GetRobotActorId(this RobotManagerComponent self, string account, out ActorId actorId)
        {
            if (!self.robots.TryGetValue(account, out int fiberId))
            {
                actorId = default;
                return false;
            }
            actorId = new ActorId(self.GetSingleton<AddressSingleton>().InnerAddress, new FiberInstanceId(fiberId, 1));
            return true;
        }

        /// <summary>
        /// 创建机器人，await之后，机器人则登录成功
        /// </summary>
        public static async ETTask<int> NewRobot(this RobotManagerComponent self, SchedulerType schedulerType, string account)
        {
            EntityRef<RobotManagerComponent> selfRef = self;
            string routerManagerAddress = GetRouterManagerAddress(self);
            int robot = await self.Fiber().CreateFiber(schedulerType, IdGenerater.Instance.GenerateId(), SceneType.Client, account);
            self = selfRef;
            ProcessInnerSender processInnerSender = self.Root().GetComponent<ProcessInnerSender>();
            Robot_LoginRequest robotLoginRequest = Robot_LoginRequest.Create();
            robotLoginRequest.Account = account;
            robotLoginRequest.Password = "";
            robotLoginRequest.Address = routerManagerAddress;
            await processInnerSender.Call(new FiberInstanceId(robot, 1), robotLoginRequest);
            self = selfRef;
            self.robots.Add(account, robot);
            return robot;
        }

        private static string GetRouterManagerAddress(this RobotManagerComponent self)
        {
            Fiber routerManagerFiber = self.Fiber().GetFiber("RouterManager");
            if (routerManagerFiber == null)
            {
                throw new Exception($"RouterManager fiber not found under fiber: {self.Fiber().Name}");
            }

            string address = routerManagerFiber.Root.GetComponent<RouterManagerAddressComponent>()?.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new Exception("RouterManagerAddressComponent is missing on RouterManager scene.");
            }

            return address;
        }
    }
}
