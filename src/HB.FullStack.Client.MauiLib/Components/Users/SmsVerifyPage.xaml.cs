/*
 * Author£∫Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using HB.FullStack.Client.MauiLib.Base;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Components;

public partial class SmsVerifyPage : BasePage<SmsVerifyViewModel>
{
    public SmsVerifyPage()
    {
        InitializeComponent();
    }

    protected override void RegisterCustomerControls(IList<IBaseContentView> customerControls)
    {
        customerControls.Add(SmsEntry);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //TODO: ≤ª––æÕ ‘ ‘Loaded
        SmsEntry.Focus();
    }
}