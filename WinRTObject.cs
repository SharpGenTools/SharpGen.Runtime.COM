using System;

namespace SharpGen.Runtime
{
    public partial class WinRTObject
    {
        public Guid[] Iids
        {
            get
            {
                GetIids(out var count, out var iids);
                var iid = new Guid[count];
                MemoryHelpers.Read<Guid>(iids, iid, (int) count);
                return iid;
            }
        }
    }
}