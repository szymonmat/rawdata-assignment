using DomainModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var addr = IPAddress.Parse("127.0.0.1");
            var server = new TcpListener(addr, 5000);
            List<Category> database = new List<Category>()
            {
                new Category { Cid = 1, Name = "Beverages"},
                new Category { Cid = 2, Name = "Condiments" },
                new Category { Cid = 3, Name = "Confections" }
            };
            server.Start();
            Console.WriteLine("Server started ...");


            while (true)
            {
                //connection setup
                var client = server.AcceptTcpClient();
                var strm = client.GetStream();
                var buffer = new byte[client.ReceiveBufferSize];
                var readCnt = strm.Read(buffer, 0, buffer.Length);
                //Data processing setup
                List<string> errorList = new List<string>();
                var payload = Encoding.UTF8.GetString(buffer, 0, readCnt);
                var request = JsonConvert.DeserializeObject<Request>(payload);
                var response = new Response();
                String errorString = "";

                //Validate for mandatory elements (mehtod, date, path)
                if (request.Method == null)
                {
                    errorList.Add("missing body");
                }
                if (request.Date == 0)
                {
                    errorList.Add("missing date");
                }
                if (request.Path == null)
                {
                    errorList.Add("missing path");
                }
                //If any of critical elements are missing generate error message, otherwise start evaluationg methods
                if (errorList.Count > 0) {
                    var l = errorList.Count;
                    for (var i = 0; i < l; i++)
                    {
                        if (i != l)
                        {
                            string.Concat(errorString, errorList[i], ',');
                        }
                        else {
                            string.Concat(errorString, errorList[i]);
                        }
                    }
                    response.Status = "4 Bad Request";
                    response.Body = errorString;
                }
                else
                {
                    //Evaluate type of request, and execute appropriate code
                    switch (request.Method)
                    {
                        case "read":

                            break;
                        case "update":

                            break;
                        case "create":

                            break;
                        case "delete":

                            break;
                        case "echo":
                            if (request.Body != null)
                            {
                                response.Status = "1 Ok";
                                response.Body = request.Body;
                            }
                            else
                            {
                                response.Status = "4 Bad Request";
                                response.Body = "4 missing body";
                            }
                            Console.WriteLine(JsonConvert.SerializeObject(response));

                            break;
                    }
                }
                var res = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                strm.Write(res, 0, res.Length);
             }
        }
            //server.Stop();
    }
}

