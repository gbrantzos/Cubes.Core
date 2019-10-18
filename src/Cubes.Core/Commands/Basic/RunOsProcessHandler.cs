using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Cubes.Core.Commands.Basic
{
    public class RunOsProcessHandler : IRequestHandler<RunOsProcessCommand, RunOsProcessResult>
    {
        public Task<RunOsProcessResult> Handle(RunOsProcessCommand command, CancellationToken cancellationToken)
        {
            // Helpers
            var output = new StringBuilder();

            // Create process
            var process = new Process();
            process.StartInfo.FileName = command.Command;
            process.StartInfo.Arguments = command.Arguments;
            process.StartInfo.WorkingDirectory = command.StartIn;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Capture output
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (s, e) => { output.Append($"{System.Environment.NewLine}{e.Data}"); };
            process.ErrorDataReceived += (s, e) => { output.Append($"{System.Environment.NewLine}ERROR: { e.Data}"); };
            output.Append($"==> {command.ToString()}");

            // Start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for exit
            process.WaitForExit(command.TimeoutSecs * 1000);
            process.CancelOutputRead();
            process.CancelErrorRead();

            var msg = output.ToString();

            var result = new RunOsProcessResult
            {
                Message  = $"Process finished, exit code {process.ExitCode}",
                ExitCode = process.ExitCode,
                Output   = output.ToString()
            };
            return Task.FromResult(result);
        }
    }
}