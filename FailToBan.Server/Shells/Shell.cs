using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class Shell
    {
        private ShellState state;

        public virtual string Get(string[] values)
        {
            return state.Handle(values) + "\n" + state.Message;
        }

        public void SetState(ShellState state)
        {
            this.state = state;
        }
    }
}