using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace StudyingImprovement.Model
{
    public class Message : RequestMessage<string>
    {
        public static Message SpeechToText = new Message("speechText");

        public string Method { get; private set; }

        public Message(string method) 
        {
            this.Method = method;
        }
    }
}
