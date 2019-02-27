using System;
using System.Collections.Generic;
using System.Text;

namespace Cubes.Core.Commands
{
    public interface ICommandResult
    {
        bool HasErrors { get; set; }
        string Message { get; set; }
    }
}
