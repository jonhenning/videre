using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Videre.Core.Extensions.Bootstrap
{
    public interface IFluentInterface   //http://blogs.clariusconsulting.net/kzu/how-to-hide-system-object-members-from-your-interfaces/
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}
