﻿using DomainModel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 5000);

            var strm = client.GetStream();

            var request = new Request
            {
                Method = "read",
                Path = "/api/categories",
                Date = 1507318869,
                Body = "{ cid: 3, name:'Test'}"
            };
            //Validate recieved string first
            //Check if recieved string is a json 
            
            var payload = JsonConvert.SerializeObject(request);
            var buffer = Encoding.UTF8.GetBytes(payload);

            strm.Write(buffer, 0, buffer.Length);

            var readCnt = strm.Read(buffer, 0, buffer.Length);

            var res = Encoding.UTF8.GetString(buffer, 0, readCnt);

            Console.WriteLine(res);

            strm.Close();

            client.Dispose();
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
