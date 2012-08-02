using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;

namespace Videre.Core.Services
{
    public class Logging
    {
        private static ILog _log = null;

        public static ILog Logger
        {
            get
            {
                if (_log == null)
                    _log = LogManager.GetCurrentClassLogger();
                return _log;
            }
        }

    }
}
