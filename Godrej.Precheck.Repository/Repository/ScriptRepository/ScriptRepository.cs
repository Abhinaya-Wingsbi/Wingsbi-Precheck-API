using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Repository.Repository.ScriptRepository
{
    public class ScriptRepository : IScriptRepository
    {
        private readonly ILogger<ScriptRepository> _logger;

        public ScriptRepository(ILogger<ScriptRepository> logger)
        {
            _logger = logger;
        }
    }
}
