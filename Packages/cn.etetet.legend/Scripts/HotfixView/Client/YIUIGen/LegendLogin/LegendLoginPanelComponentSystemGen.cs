using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [FriendOf(typeof(YIUIWindowComponent))]
    [FriendOf(typeof(YIUIPanelComponent))]
    [EntitySystemOf(typeof(LegendLoginPanelComponent))]
    public static partial class LegendLoginPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LegendLoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this LegendLoginPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this LegendLoginPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 10; 

            self.u_ComAccountTMP_InputField = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_InputField>("u_ComAccountTMP_InputField");
            self.u_ComPasswordTMP_InputField = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_InputField>("u_ComPasswordTMP_InputField");
            self.u_EventLogin = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventLogin");
            self.u_EventLoginHandle = self.u_EventLogin.Add(self,LegendLoginPanelComponent.OnEventLoginInvoke);

        }
    }
}
