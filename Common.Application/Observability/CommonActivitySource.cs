using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Application.Observability
{
    public static class CommonActivitySource
    {
        public static readonly ActivitySource Instance = new("Common"); // لمراقبة هذه المكتبه في حال نريد مراقبة غيرها نضيف الاسم 
    }
}
