using System;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands.Basic
{
    public class RunOsProcessCommand : ICommand<RunOsProcessResult>
    {
        /// <summary>
        /// OS command to start.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Command arguments.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Working directory.
        /// </summary>
        public string StartIn { get; set; }

        /// <summary>
        /// Wait timeout in seconds, by default 30.
        /// </summary>
        public int TimeoutSecs { get; set; }

        public RunOsProcessCommand() => TimeoutSecs = 30;

        public override string ToString() => $"Run {Command} with {(String.IsNullOrEmpty(Arguments)? "no argumets" : $"args '{Arguments}'")}, in {StartIn.IfNullOrEmpty("current directory")}";

    }
}