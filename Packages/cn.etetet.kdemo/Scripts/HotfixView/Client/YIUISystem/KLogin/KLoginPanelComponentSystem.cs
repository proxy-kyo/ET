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
            Scene root = self.Root();
            EntityRef<KLoginPanelComponent> selfRef = self;
            string address = root.GetComponent<GlobalComponent>().GlobalConfig.Address;
            try
            {
                await LoginHelper.Login(root, address, acct, pass);
            }
            catch (Exception e)
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                string message = e is RpcException rpc && rpc.Error == ErrorCode.ERR_LoginAccountPasswordError
                    ? "用户名或密码错误"
                    : "登录失败，请稍后重试";
                await TipsHelper.Open<TipsTextViewComponent>(self.Root(), message);
            }
        }
        #endregion YIUIEvent结束
    }
}
