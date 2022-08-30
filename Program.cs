namespace GitTimeTravel {

    public class Program {

        Program(string cwd, int monthsToAdd) {
            _cwd = cwd;
            _monthsToAdd = monthsToAdd;
        }

        string _cwd;
        int _monthsToAdd;
        Execute _execute = new Execute();
        List<Commit> _commits = new List<Commit>();

        class Commit {
            public string hash;
            public string author;
            public string dateStr;
            public DateTimeOffset date;
            public string message;
        }

        (int next, Commit commit) _ParseCommit(int first, List<string> stdout) {

            var commit = new Commit();
            commit.hash = stdout[first].Substring(7, 40);
            commit.author = stdout[first + 1].Substring(8);
            commit.dateStr = stdout[first + 2].Substring(8);
            commit.date = DateTimeOffset.ParseExact(commit.dateStr, "ddd MMM dd HH:mm:ss yyyy zzz", System.Globalization.CultureInfo.InvariantCulture);
            commit.message = stdout[first + 4];
            var next = first + 6;
            return (next, commit);
        }

        static string _ToRFC1233PlusTimeZone(DateTimeOffset date) {
            var rv = date.ToString("ddd MMM dd HH':'mm':'ss yyyy zz00");
            return rv;
        }

        void _WriteLineCommits() {
            foreach (var commit in _commits) {
                Console.WriteLine(commit.hash);
                //Console.WriteLine(commit.author);
                Console.WriteLine(commit.dateStr);
                //Console.WriteLine(commit.date);
                //Console.WriteLine(commit.date.ToString("r"));
                //Console.WriteLine(commit.date.ToString("r", System.Globalization.CultureInfo.InvariantCulture));
                //Console.WriteLine(commit.date.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
                //Console.WriteLine(commit.date.AddMonths(6).ToLocalTime().ToString("r"));

                Console.WriteLine( _ToRFC1233PlusTimeZone(commit.date));
                var newDate = commit.date.AddMonths(6);
                Console.WriteLine( _ToRFC1233PlusTimeZone(newDate));


                //Console.WriteLine(commit.message);
            }
        }

        string _CreateGitRebaseTodo(List<Commit> commits) {
            var fileName = System.IO.Path.GetTempFileName();
            using(var stream = new StreamWriter(fileName))
            {
                for (var i = commits.Count() - 1; i >= 0; --i) {
                    var commit = commits[i];
                    var s = String.Format("e {0}", commit.hash.Substring(0, 8));
                    stream.WriteLine(s);
                }
            }
            return fileName;             
        }

        string _CreateBatchFile(string todoFileName) {
            var fileName = System.IO.Path.GetTempFileName() + ".bat";
            var line = String.Format("copy {0} .git\\rebase-merge\\git-rebase-todo", todoFileName);
            File.WriteAllText(fileName, line);
            return fileName;
        }

        void _ConsoleWriteLine(string line) {
            Console.WriteLine(line);
        }

        void _GetCommits() {
            var result = _execute.Run("git.exe", "log");
            var i = 0;
            while (i < result.stdout.Count) {
                var parseResult = _ParseCommit(i, result.stdout);
                _commits.Add(parseResult.commit);
                i = parseResult.next;                
            }
        }

        void _StartRebase() {
            var todoFileName = _CreateGitRebaseTodo(_commits);
            var batFileName = _CreateBatchFile(todoFileName);            
            //git -c sequence.editor=c:\\temp\\doit.bat rebase -i --root main
            var gitargs = String.Format("-c sequence.editor={0} rebase -i --root main", batFileName.Replace(@"\", @"\\"));
            var result = _execute.Run("git.exe", gitargs);
        }

        void _AmendCommits() {
            for (var i = _commits.Count() - 1; i >= 0; --i) {
                var commit = _commits[i];
                var newDate = commit.date.AddMonths(_monthsToAdd);
                var newDateStr = _ToRFC1233PlusTimeZone(newDate);
                var result1 = _execute.Run("git.exe", String.Format("commit --amend --reset-author --no-edit --date=\"{0}\"", newDateStr));
                var result2 = _execute.Run("git.exe", "rebase --continue");
            }
        }        

        void _Main() {
            _execute.flags = Execute.Flags.ThrowOnErrorCode | Execute.Flags.LogCommand;
            _execute.statusUpdate = _ConsoleWriteLine;
            _execute.workingDirectory = _cwd;
            _GetCommits();
            _StartRebase();
            _AmendCommits();
        }

        static void Main(string[] args) {
            var program = new Program(args[0], Int32.Parse(args[1]));
            program._Main();
        }
    }
}
