﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Extensions.Configuration;
    using Newtonsoft.Json;
    using Microsoft.Extensions.Configuration;

    public class Chat : Hub
    {
        private readonly IConfiguration _configuration;

        public Chat(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void BroadcastMessage(string name, string message)
        {
            var answer = GetAnswerToUserEnteredQuestion(message);

            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message);
            Clients.Client(Context.ConnectionId).SendAsync("broadcastMessage", "Assistant", answer.answers[0].answer);
        }

        private QuestionAndAnswer GetAnswerToUserEnteredQuestion(string message)
        {
            var baseAddress =
                "https://testqnaassistant.azurewebsites.net/qnamaker/knowledgebases/06b1420e-5412-4163-99cc-b82946a3464f/generateAnswer";

            string accessKey = _configuration["KnowledgeBaseAccessKey"];

            var http = (HttpWebRequest) WebRequest.Create(new Uri(baseAddress));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Headers.Add("Authorization", $"EndpointKey {accessKey}");
            http.Method = "POST";

            string parsedContent = $"{{\"question\":\"{message}\"}}";
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = JsonConvert.DeserializeObject<QuestionAndAnswer>(sr.ReadToEnd());
            return content;
        }


        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message);
        }

    }


    public class QuestionAndAnswer
    {
        public Answer[] answers { get; set; }
    }

    public class Answer
    {
        public string[] questions { get; set; }
        public string answer { get; set; }
        public float score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public object[] metadata { get; set; }
    }

}
