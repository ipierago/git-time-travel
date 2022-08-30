using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace GitTimeTravel {

    class Execute  {

        public enum Flags {
            ThrowOnErrorCode = 1 << 0,
            LogCommand = 1 << 1,
        }

        public class Result {
            public int exitCode;
            public List<string> stdout;
            public List<string> stderr;
        }

        string _JoinWithNewline(List<string> list) {
            var rv = "";
            for (int i = 0; i < list.Count; ++i) {
                if (i > 0) rv += Environment.NewLine;
                rv += list[i];
            }
            return rv;
        }

        Action<string> _statusUpdate = line => {};
        public Action<string> statusUpdate { get { return _statusUpdate; } set { _statusUpdate = value; } }

        Flags _flags = 0;
        public Flags flags { get {return _flags;} set {_flags = value;}}

        string _workingDirectory = "";
        public string workingDirectory {get {return _workingDirectory;} set {_workingDirectory = value;}}

        public Result Run(string exeFileName, string args) {
            if ((_flags & Flags.LogCommand) != 0) {
                var line = String.Format("> {0} {1}", exeFileName, args);
                _statusUpdate(line);
            }

            using (Process process = new Process() {
                StartInfo = new System.Diagnostics.ProcessStartInfo() { 
                    FileName = exeFileName,  Arguments = args, 
                    UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
                    WorkingDirectory = _workingDirectory
                }
            }) {
                var stdout = new List<string>();                
                process.OutputDataReceived += (object sender, DataReceivedEventArgs eventArgs) => { 
                    if (eventArgs.Data != null) {
                        _statusUpdate(eventArgs.Data);
                        stdout.Add(eventArgs.Data);
                    }
                };
                var stderr = new List<string>();
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs eventArgs) => {
                    if (eventArgs.Data != null) {
                        _statusUpdate(eventArgs.Data);
                        stderr.Add(eventArgs.Data);
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                if (process.ExitCode != 0) {
                    if ((_flags & Flags.ThrowOnErrorCode) != 0) {
                        throw new Exception(String.Format("\"git {0}\" returned error code {1}", args, process.ExitCode));
                    }
                }             
                return new Result() {exitCode = process.ExitCode, stdout = stdout, stderr = stderr};
            }
        }



    }
}
