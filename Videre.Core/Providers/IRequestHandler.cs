using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Services;

namespace Videre.Core.Providers
{
    public interface IRequestHandler
    {
        string Name { get; }
        int Priority { get; }   //a general priority to allow handlers to be invoked before others...  
        void Execute(string url, Models.PageTemplate template);
    }
}
