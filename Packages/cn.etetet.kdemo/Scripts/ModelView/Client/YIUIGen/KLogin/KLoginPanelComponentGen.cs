using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Panel)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class KLoginPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "KLogin";
        public const string ResName = "KLoginPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public TMPro.TMP_InputField u_ComAccountTMP_InputField;
        public TMPro.TMP_InputField u_ComPasswordTMP_InputField;
        public UITaskEventP0 u_EventLogin;
        public UITaskEventHandleP0 u_EventLoginHandle;
        public const string OnEventLoginInvoke = "KLoginPanelComponent.OnEventLoginInvoke";

    }
}