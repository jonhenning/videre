using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Services;

namespace Videre.Core.Providers
{
    public interface IJsonValueProviderFactory
    {
        string Name { get; }
        void Swap();
    }
}
