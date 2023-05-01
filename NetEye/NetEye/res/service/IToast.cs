using System;
using System.Collections.Generic;
using System.Text;

namespace NetEye.res.service
{
    public interface IToast
    {
        void ShortToast(string message);
        void LongToast(string message);
        // DependencyService.Get<IToast>().ShortToast("Lorem ipsum dolor sit amet");
    }
}
