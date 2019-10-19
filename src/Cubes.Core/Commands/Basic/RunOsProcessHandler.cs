using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cubes.Core.Commands.Basic
{
    public class RunOsProcessHandler : RequestHandler<RunOsProcess, RunOsProcessResult>
    {
        protected override Task<RunOsProcessResult> HandleInternal(RunOsProcess command, CancellationToken cancellationToken)
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
                ExitCode = process.ExitCode,
                Output   = output.ToString()
            };
            MessageToReturn = $"Process finished, exit code {process.ExitCode}";

            return Task.FromResult(result);
        }
    }
}