using System;
using IrcDotNet;
using System.Threading;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace TwitchBot
{
    class Bot
    {
        private IrcClient ircClient;
        private String ChannelToConnect = "#yohelloyukinon";
        private static bool isRunning;

        static void Main(string[] args)
        {
            Console.Title = "TwitchBot";
            Console.SetWindowSize(120, 40);
            Console.SetBufferSize(120, 400);
            Bot Miku = new Bot();
            Miku.Connect();
            Miku.JoinChannel();
            Miku.VoiceRecognition();
            isRunning = true;
            Miku.Run();

        }

        
        

        public void Run()
        {
            // Read commands from stdin until bot terminates.
            isRunning = true;
            
            while (isRunning)
            {
                /*
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null)
                    break;
                if (line.Length == 0)
                    continue;

                var parts = line.Split(' ');
                var command = parts[0].ToLower();
                if (line.Contains(' '))
                    parameters = line.Substring(line.IndexOf(' '));
                ReadCommand(command, parameters);
                */
            }
        }

        private void ReadCommand(string command, string parameters)
        {
            switch(command)
            {
                case "!c":
                    Connect();
                    break;
                case "!j":
                    JoinChannel();
                    break;
                case "!s":
                    SendMessage(parameters);
                    break;
                case "!e":
                    Stop();
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }

        public void Connect()
        {
            ircClient = new IrcClient
            {
                FloodPreventer = new IrcStandardFloodPreventer(10,100)
            };

            // register events
            ircClient.Connected += ircClient_Connected;
            ircClient.ConnectFailed += ircClient_ConnectFailed;
            ircClient.Disconnected += ircClient_Disconnected;
            ircClient.Registered += ircClient_Registered;
            ircClient.RawMessageReceived += ircClient_RawMessageReceived;
            ircClient.RawMessageSent += ircClient_RawMessageSent;
            

            IrcRegistrationInfo iri = new IrcUserRegistrationInfo()
            {
                UserName = "yohelloyukinon",
                NickName = "yohelloyukinon",
                Password = "oauth:62gpjlwlyripeq5ssyybsz3o7g0n8x"
            };

            ircClient.Connect("irc.twitch.tv", 6667, false, iri);
            Thread.Sleep(1000);
        }

        private void JoinChannel()
        {
            ircClient.Channels.Join(ChannelToConnect);
            ircClient.LocalUser.MessageReceived += ircClientLocalUser_MessageReceived;
            ircClient.LocalUser.MessageSent += ircClientLocalUser_MessageSent;
            Console.WriteLine("Nickname: " + ircClient.LocalUser.NickName);
        }

        public void VoiceRecognition()
        {
            SpeechRecognitionEngine sr = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            sr.SetInputToDefaultAudioDevice();//микрофон
           
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = new System.Globalization.CultureInfo("en-US");
            //grammarBuilder.Append(new Choices("мику, покажи время", "мику, лав лайв", "мику, выход", "время"));//добавляем используемые фразы
            grammarBuilder.Append(new Choices("English!"));
            Grammar gr = new Grammar(grammarBuilder);

            sr.UnloadAllGrammars();
            sr.LoadGrammar(gr);
            sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);//событие речь распознана
            sr.RecognizeAsync(RecognizeMode.Multiple);//начинаем распознование
        }

        private void Stop()
        {
            var serverName = "Unknown";
            if (ircClient != null)
            {
                serverName = ircClient.ServerName;
                ircClient.Disconnect();
                ircClient.Quit();
                ircClient.Dispose();
            }
            Console.Out.WriteLine("Disconnected from '{0}'.", serverName);
            isRunning = false;
        }

        
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SpeechSynthesizer ss = new SpeechSynthesizer();
            ss.Volume = 100;// от 0 до 100
            ss.Rate = 0;//от -10 до 10
            ss.SetOutputToDefaultAudioDevice();
            Console.WriteLine("Recognized phrase: " + e.Result.Text);
            ss.SpeakAsync(e.Result.Text);
            
            if (e.Result.Text.Contains("мику"))
            {
                switch (e.Result.Text.Substring(6))
                {
                    case "лав лайв":
                        SendMessage(" !ll");
                        //ircClient_RawMessageReceived(sender, e)
                        break;
                    case "покажи время":
                        SendMessage("Сейчас " + DateTime.Now.ToShortTimeString() + ".");
                        break;
                    case "выход":
                        Stop();
                        break;
                    default:
                        break;
                }
            }
            
        }
        
        private void SendMessage(string message)
        {
            if (ircClient != null)
            {
                ircClient.LocalUser.SendMessage(ChannelToConnect, message);
                //ircClient.SendRawMessage("1234567890");
            }
        }

        //events
        private void ircClient_Connected(object sender, EventArgs e)
        {
            if (e != null)
                Console.WriteLine("Connected: " + e.ToString());
        }

        private void ircClient_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            if (e != null)
                Console.WriteLine("Connect failed: " + e.Error.Message);
        }

        private void ircClient_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected.");
        }
        
        private void ircClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            string User = e.Message.Parameters[0], Message = e.Message.Parameters[1];
            if (e != null)
            {
                
                if (e.RawContent.ToString().Contains("#"))
                {
                    var i = e.RawContent.ToString().IndexOf("#");
                    Message = e.RawContent.ToString().Substring(i);
                    Console.WriteLine(Message);
                }
                else
                    Console.WriteLine("Message received: " + e.RawContent.ToString());
                    
                if (User.IndexOf('#') == 0)
                { 
                    Console.WriteLine(User + ": " + Message);
                }
                if (Message != null)
                {
                    if (Message.Contains("!tog"))
                        SendMessage("http://www.webtoons.com/en/fantasy/tower-of-god/list?title_no=95");

                    if (Message.Contains("!music"))
                        SendMessage("https://www.youtube.com/playlist?list=PLd-myY-TJBJd3yv-CDwowV9TngjIE4ntL");

                    if (Message.Contains("!ll"))
                    {
                        SendMessage("/me L\\n");
                        SendMessage("/me O");
                        SendMessage("/me V");
                        SendMessage("/me E");
                        SendMessage("/me \u2063 \\n");
                        SendMessage("/me L");
                        SendMessage("/me I");
                        SendMessage("/me V");
                        SendMessage("/me E");
                        SendMessage("/me !");
                    }
                }
            }
        }


        private void ircClientLocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            Console.WriteLine("local user message received" + e.Text);
        }

        private void ircClientLocalUser_MessageSent(object sender, IrcMessageEventArgs e)
        {
            Console.WriteLine("local user message sent" + e.Text);
        }

        private void ircClient_RawMessageSent(object sender, IrcRawMessageEventArgs e)
        {
            int i = 0;
            string Message = "";
            if (e != null)
            {
                if (e.RawContent.ToString().Contains("#"))
                {
                    i = e.RawContent.ToString().IndexOf("#");
                    Message = e.RawContent.ToString().Substring(i);
                    Console.WriteLine(Message);
                }
                else
                    Console.WriteLine("raw message sent: " + e.RawContent.ToString());
            }
        }

        private void ircClient_Registered(object sender, EventArgs e)
        {
            Console.WriteLine("Registered.");
            //JoinChannel();
        }
    }
}
