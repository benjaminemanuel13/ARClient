using System;
using System.Collections.Generic;
using System.Text;

namespace ARClient.EventArguments
{
    public class MessageRecievedEventArgs : EventArgs
    {
        public string Message { get; set; }

        public MessageRecievedEventArgs(string message)
        {
            Message = message;
        }
    }
}
