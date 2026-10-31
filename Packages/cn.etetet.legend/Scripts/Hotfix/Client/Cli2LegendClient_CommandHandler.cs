namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class Cli2LegendClient_CommandHandler: MessageHandler<Scene, Cli2LegendClient_Command, LegendClient2Cli_Command>
    {
        protected override async ETTask Run(Scene root, Cli2LegendClient_Command request, LegendClient2Cli_Command response)
        {
            await LegendCliCommandHelper.Execute(root, request, response);
        }
    }
}
