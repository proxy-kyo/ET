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
    [FriendOf(typeof(LobbyPanelComponent))]
    public static partial class LobbyPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LobbyPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(LobbyPanelComponent.OnEventEnterMapInvoke)]
        private static async ETTask OnEventEnterMapInvoke(this LobbyPanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
