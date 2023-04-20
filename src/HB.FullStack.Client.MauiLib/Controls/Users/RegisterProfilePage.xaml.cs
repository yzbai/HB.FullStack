/*
 * Author£ºYuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

namespace HB.FullStack.Client.MauiLib.Controls;

public partial class RegisterProfilePage : BasePage<RegisterProfileViewModel>
{
    public RegisterProfilePage()
    {
        InitializeComponent();
    }

    protected override void RegisterCustomerControls(IList<IBaseContentView> customerControls)
    {
    }
}