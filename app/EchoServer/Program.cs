using DomainModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var addr = IPAddress.Parse("127.0.0.1");
            var server = new TcpListener(addr, 5000);
            server.Start();
            List<Category> database = new List<Category>()
            {
                new Category { Cid = 1, Name = "Beverages"},
                new Category { Cid = 2, Name = "Condiments" },
                new Category { Cid = 3, Name = "Confections" }
            };
            LoopClients();
            Console.WriteLine("Server started ...");
             void LoopClients()
            {
                while (true)
                {
                    // wait for client connection
                    TcpClient newClient = server.AcceptTcpClient();

                    // client found.
                    // create a thread to handle communication
                    Thread t = new Thread(new ParameterizedThreadStart(mainClass));
                    t.Start(newClient);
                }
            }
            void mainClass(object obj) {
                while (true)
                {
                    //connection setup
                    var client = (TcpClient)obj;
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
                    List<string> getPathValues(string path)
                    {
                        path = path.Substring(1, path.Length - 1);
                        List<string> values = new List<string>(path.Split('/'));
                        if (values.Count > 0)
                        {
                            values.RemoveAt(0);

                            return values;
                        }
                        else
                        {
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
                        var errres = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        strm.Write(errres, 0, errres.Length);
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
                    if (errorList.Count > 0)
                    {
                        var l = errorList.Count;
                        for (var i = 0; i < l; i++)
                        {
                            if (i == 0)
                            {
                                errorString = errorList[i];
                            }
                            else
                            if (i < l)
                            {
                                errorString = errorString + ", " + errorList[i];
                            }
                            else
                            {
                                errorString = errorString + " " + errorList[i];
                                string.Concat(errorString, errorList[i]);
                            }
                        }
                        Console.WriteLine(errorString);
                        response.Status = "missing resource";
                        response.Body = errorString;
                    }
                    else
                    {
                        //Get values from path
                        var values = getPathValues(request.Path);
                        //Evaluate type of request, and execute appropriate code
                        switch (request.Method)
                        {
                            case "read":

                                //If there's only one value we assume that the user wasnt to select whole table
                                if (values.Count == 1)
                                {
                                    var result = JsonConvert.SerializeObject(database);
                                    response.Status = "1 Ok";
                                    response.Body = result;
                                }
                                //With two values the user provides us id, so we can select correct value
                                else if (values.Count == 2)
                                {
                                    //First we check if second param is a valid int
                                    int val;
                                    bool eval = Int32.TryParse(values[1], out val);
                                    string result = null;
                                    //Loop through all objects in db and find one with appropriate id
                                    if (eval)
                                    {
                                        for (var i = 0; i < database.Count; i++)
                                        {
                                            if (database[i].Cid == val)
                                            {
                                                result = JsonConvert.SerializeObject(database[i]);
                                            }
                                        }
                                        if (result != null)
                                        {
                                            response.Status = "1 Ok";
                                            response.Body = result;
                                        }
                                        else
                                        {
                                            response.Status = "5 Not Found";
                                            response.Body = "cid not found";
                                        }
                                    }
                                    else
                                    {
                                        response.Status = "4 Bad Request";
                                        response.Body = null;
                                    }
                                }

                                else
                                {
                                    response.Status = "5 not found";
                                    response.Body = null;
                                }
                                break;
                            case "update":
                                //If there's only one value we assume that the user wasnt to select whole table
                                if (values.Count == 1)
                                {
                                    response.Status = "4 Bad Request";
                                    response.Body = "missing attribute cid";
                                }
                                //With two values the user provides us id, so we can select correct value
                                else if (values.Count == 2)
                                {
                                    //First we check if second param is a valid int
                                    int val;
                                    bool eval = Int32.TryParse(values[1], out val);
                                    string result = null;
                                    //Loop through all objects in db and find one with appropriate id
                                    if (eval)
                                    {
                                        try
                                        {
                                            var test = JsonConvert.DeserializeObject<Category>(request.Body);
                                        }
                                        catch
                                        {
                                            response.Status = "6 Error";
                                            response.Body = "wrong body format provided";
                                            var errres = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                            strm.Write(errres, 0, errres.Length);
                                        }
                                        for (var i = 0; i < database.Count; i++)
                                        {
                                            if (database[i].Cid == val)
                                            {
                                                var newVal = JsonConvert.DeserializeObject<Category>(request.Body);
                                                var newName = newVal.Name;

                                                if (newVal.Cid == database[i].Cid)
                                                {
                                                    database[i].Name = newName;
                                                    result = "Ok";
                                                    break;
                                                }
                                            }
                                            //If provided cid and edit request do not match, passs error message
                                            else if (i == database.Count)
                                            {
                                                result = null;
                                            }
                                        }
                                        if (result != null)
                                        {
                                            response.Status = "3 Updated";
                                            response.Body = "3 Updated";
                                        }
                                        else
                                        {
                                            response.Status = "5 Not Found";
                                            response.Body = "5 cid not found";
                                        }
                                    }
                                    else
                                    {
                                        response.Status = "4 Bad Request";
                                        response.Body = "4 illegal parameter: cid";
                                    }
                                }

                                else
                                {
                                    response.Status = "4 Bad Request";
                                    response.Body = "illegal path";
                                }
                                break;
                            case "create":
                                //The user only needs to select the whole table
                                if (request.Path.Contains("categories"))
                                {
                                    if (values.Count == 1)
                                    {
                                        var newId = database[database.Count - 1].Cid + 1;
                                        database.Add(new Category { Cid = newId, Name = request.Body });
                                        var result = JsonConvert.SerializeObject(database[database.Count - 1]);
                                        Console.WriteLine(result);
                                        response.Status = "1 Ok";
                                        response.Body = result;
                                    }
                                    else
                                    {
                                        response.Status = "4 Bad Request";
                                        response.Body = null;
                                    }

                                }
                                else
                                {
                                    response.Status = "4 missing body";
                                    response.Body = "illegal path choose right table";
                                }

                                break;
                            case "delete":

                                if (values.Count != 2 && values.Count != 3)
                                {
                                    //First we check if second param is a valid int

                                    bool eval = Int32.TryParse(values[1], out int val);

                                    //Loop through all objects in db and find one with appropriate id
                                    if (eval)
                                    {
                                        for (var i = 0; i < database.Count; i++)
                                        {
                                            if (database[i].Cid == val)
                                            {
                                                database.Remove(database[i]);

                                            }
                                            else
                                            {
                                                response.Status = "4 Not Found";
                                                response.Body = "Bad path";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    response.Status = "5 not found";

                                }
                                break;
                            case "echo":

                                if (request.Body != null)
                                {
                                    response.Status = "1 Ok";
                                    response.Body = request.Body;
                                }
                                else
                                {
                                    response.Status = "4 missing body";
                                    response.Body = "missing body";
                                }
                                break;
                            default:
                                response.Status = "4 illegal method";
                                response.Body = "4 Bad Request, illegal method";
                                break;
                        }
                    }
                    var res = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                    strm.Write(res, 0, res.Length);
                    break;
                }
            }
            
        }
            //server.Stop();
    }
}

