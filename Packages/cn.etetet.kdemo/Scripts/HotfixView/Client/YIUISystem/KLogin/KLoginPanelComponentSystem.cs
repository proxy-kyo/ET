using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2026.9.8
    /// Desc
    /// </summary>
    [FriendOf(typeof(KLoginPanelComponent))]
    public static partial class KLoginPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this KLoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this KLoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this KLoginPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(KLoginPanelComponent.OnEventLoginInvoke)]
        private static async ETTask OnEventLoginInvoke(this KLoginPanelComponent self)
        {
            var acct = self.u_ComAccountTMP_InputField.text;
            var pass = self.u_ComPasswordTMP_InputField.text;
            Log.Info($"收到 [YIUIInvoke(KLoginPanelComponent.OnEventLoginInvoke)]: {acct}, {pass}");
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            await LoginHelper.Login(self.Root(), globalComponent.GlobalConfig.Address, acct, pass);
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
