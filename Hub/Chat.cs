// Copyright (c) Microsoft. All rights reserved.
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
            var baseAddress = "https://testqnaassistant.azurewebsites.net/qnamaker/knowledgebases/06b1420e-5412-4163-99cc-b82946a3464f/generateAnswer";
            
            string accessKey = _configuration["KnowledgeBaseAccessKey"];

            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
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

            
            
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message);
            Clients.All.SendAsync("broadcastMessage", "Assistant", content.answers[0].answer);
        }


        public void Echo(string name, string message)
        {
            //             POST /knowledgebases/06b1420e-5412-4163-99cc-b82946a3464f/generateAnswer
            // Host: https://testqnaassistant.azurewebsites.net/qnamaker
            // Authorization: EndpointKey d7eb4be0-ab98-41bb-81e7-799cf284aa44
            // Content-Type: application/json
            // {"question":"<Your question>"}
            //curl -X POST https://testqnaassistant.azurewebsites.net/qnamaker/knowledgebases/06b1420e-5412-4163-99cc-b82946a3464f/generateAnswer -H "Authorization: EndpointKey d7eb4be0-ab98-41bb-81e7-799cf284aa44" -H "Content-type: application/json" -d "{'question':'<Your question>'}"


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
