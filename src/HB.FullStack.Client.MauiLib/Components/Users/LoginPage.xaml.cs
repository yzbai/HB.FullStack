/*
 * Author��Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using HB.FullStack.Client.MauiLib.Base;

namespace HB.FullStack.Client.MauiLib.Components;

public partial class LoginPage : BasePage<LoginViewModel>
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void RegisterCustomerControls(IList<IBaseContentView> customerControls)
    {
    }
}