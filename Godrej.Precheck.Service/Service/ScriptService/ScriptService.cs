using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Godrej.Precheck.Repository.Repository.ScriptRepository;

namespace Godrej.Precheck.Service.Service.ScriptService
{
    public class ScriptService : IScriptService
    {
        private readonly ILogger<ScriptService> _logger;
        private readonly IScriptRepository _scriptRepository;

        public ScriptService(ILogger<ScriptService> logger, IScriptRepository scriptRepository)
        {
            _logger = logger;
            _scriptRepository = scriptRepository;
        }
    }
}
