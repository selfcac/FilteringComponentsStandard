using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HTTPProtocolFilter
{
    public class GitInfo
    {
        static Func<string>[] gitInfoFuncs =
        {
            GitInfo.GetInfo,
            CommonStandard.GitInfo.GetInfo
        };

        public static string GetInfo()
        {
            return string.Format("{0}+{1}{2} ({3}) {4}",
                ThisAssembly.Git.BaseTag,
                ThisAssembly.Git.Commits,
                ThisAssembly.Git.IsDirty ? "*" : "",
                ThisAssembly.Git.Branch,
                ThisAssembly.Git.Commit
                );
        }

        public static string[] AllGitInfo()
        {
            return gitInfoFuncs.Select((func) => func()).ToArray();
        }
    }
}
