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
                var request = new Request();
                var response = new Response();
                string errorString = "";
                //local function that will break path into usable paramterers
                string[] getPathValues(string path) {
                    path = path.Substring(1, path.Length - 1);
                    string[] values = path.Split('/');
                    if (values.Length > 0)
                    {
                        return values;
                    }
                    else {
                        return null;
                    }

                };
                //Handle corrupted request
                try
                {
                    request = JsonConvert.DeserializeObject<Request>(payload);
                }
                catch
                {
                    response.Status = "6 Error";
                    response.Body = "wrong request format provided"; 
                    var errres= Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                    strm.Write(errres, 0 ,errres.Length);
                }

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
                        if (i == 0)
                        {
                            errorString = errorList[i];
                        }else 
                        if (i < l)
                        {
                            errorString = errorString + ", " + errorList[i];
                        }else 
                        {
                            errorString = errorString + " " + errorList[i];
                            string.Concat(errorString, errorList[i]);
                        }
                    }
                    Console.WriteLine(errorString);
                    response.Status = "4 Bad Request";
                    response.Body = errorString;
                }
                else
                {
                    //Evaluate type of request, and execute appropriate code
                    switch (request.Method)
                    {
                        case "read":
                            //Get values from path
                            var values = getPathValues(request.Path);
                            //If there's only one value we assume that the user wasnt to select whole table
                            if (values.Length == 1)
                            {
                                var result = JsonConvert.SerializeObject(database);
                                response.Status = "1 Ok";
                                response.Body = result;
                            }
                            //With two values the user provides us id, so we can select correct value
                            else if (values.Length == 2) {
                                //First we check if second param is a valid int
                                int val;
                                bool eval = Int32.TryParse(values[1], out val);
                                string result=null;
                                //Loop through all objects in db and find one with appropriate id
                                if (eval)
                                {
                                    for (var i = 0; i < database.Count; i++) {
                                        if (database[i].Cid == val) {
                                            result = JsonConvert.SerializeObject(database[i]);
                                        }
                                    }
                                    if (result != null)
                                    {
                                        response.Status = "1 Ok";
                                        response.Body = result;
                                    }
                                    else {
                                        response.Status = "5 Not Found";
                                        response.Body = "cid not found";
                                    }
                                }
                                else {
                                    response.Status = "4 Bad Request";
                                    response.Body = "illegal parameter: id";
                                }
                            }

                            else {
                                response.Status = "4 Bad Request";
                                response.Body = "illegal path";
                            }
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
                                response.Body = "missing body";
                            }
                            break;
                        default:
                            response.Status = "4 Bad Request";
                            response.Body = "illegal method";
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

