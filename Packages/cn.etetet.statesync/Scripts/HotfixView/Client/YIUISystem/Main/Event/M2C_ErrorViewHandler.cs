namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class M2C_ErrorViewHandler : MessageHandler<Scene, M2C_Error>
    {
        protected override async ETTask Run(Scene root, M2C_Error message)
        {
            TipsHelper.OpenSync<TipsTextViewComponent>(root, $"<color=red>{OutputText(root.Fiber(), message.Error, message.Values.ToArray())}</color>");
            await ETTask.CompletedTask;
        }

        private static string OutputText(Fiber fiber, int key, params string[] args)
        {
            return string.Format(GetText(fiber, key), args);
        }

        private static string GetText(Fiber fiber, int key)
        {
            return fiber.GetSingleton<TextConfigCategory>().Get(key).CN;
        }
    }
}
