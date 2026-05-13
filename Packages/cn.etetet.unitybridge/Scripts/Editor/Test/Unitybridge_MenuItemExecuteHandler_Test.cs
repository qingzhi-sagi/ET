namespace ET.Test
{
    public class Unitybridge_MenuItemExecuteHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            MenuItemExecuteRequest request = MenuItemExecuteRequest.Create();
            request.MenuPath = "UnityBridge/Tests/MissingMenuItem";

            IResponse rawResponse = await new UnityBridgeMenuItemExecuteHandler().Handle(request);
            if (rawResponse is not MenuItemExecuteResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "MenuItemExecuteHandler should return MenuItemExecuteResponse");
            }

            if (response.Error != UnityBridgeErrorCode.HandlerFail || response.Executed || response.MenuPath != request.MenuPath)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "MenuItemExecuteHandler should report missing menu item failure");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
