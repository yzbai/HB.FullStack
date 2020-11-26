using HB.FullStack.Common;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityOptions : IOptions<IdentityOptions>
    {
        public IdentityOptions Value { get { return this; } }


        //TODO: 考虑是否需要在SecurityStamp改变后，删除SignInToken？
        //public IdentityEvents Events { get; set; } = new IdentityEvents();

        /// <summary>
        /// 用来查mobile，loginname，email是否重复的布隆表 名称
        /// </summary>
        [Required]
        public string BloomFilterName { get; set; } = null!;
    }

    //public class IdentityEvents
    //{
    //    private readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();

    //    public event AsyncEventHandler<SecurityStampChangedContext, EventArgs> SecurityStampChanged
    //    {
    //        add => _asyncEventManager.Add(value);
    //        remove => _asyncEventManager.Remove(value);
    //    }

    //    internal async Task OnSecurityStampChangedAsync(SecurityStampChangedContext context)
    //    {
    //        await _asyncEventManager.RaiseEventAsync(nameof(SecurityStampChanged), context, new EventArgs()).ConfigureAwait(false);
    //    }
    //}
}
