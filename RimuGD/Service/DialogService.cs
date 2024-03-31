using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RimuGD {
    internal abstract class DialogService<T> where T : class, new()
    {
        public abstract string Title { get; set; }
        public abstract string InitialDirectory { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool? Show();

        protected readonly T dialog;

        protected DialogService(T dialog)
        {
            this.dialog = dialog;
        }
    } 
}
