using SciServerScriptExecutor.Models;
using SciserverScripting.Models;
using SciserverScripting.src.ScriptFileAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Newtonsoft.Json;

namespace SciServerScriptExecutor.Controllers
{
    public class ScriptExecutionController : ApiController
    {
        // GET api/scriptexecution
        public PythonOutputModel Get()
        {
            PythonScriptModel script = new PythonScriptModel()
            {
                scriptText = @"import SciServerUtils

print(""Hello world!"")",
                token = "56a10e45605c45b8ac8d6a16a69ba548",
                webDAVuser = new WebDAVUser()
                {
                    userName = "af7d799e8be5487e93c699ba87bf72d1",
                    password = "af7d799e8be5487e93c699ba87bf72d1"
                }
            };

            var pom = new PythonOutputModel();

            String outputString = "Python output:\n\n";

            pom.outputText = outputString + runPythonScript(script);

            return pom;
        }

        // GET api/scriptexecution/5
        public string Get(int id)
        {
            return "value";

        }
        // POST api/scriptexecution
        //public void Post([FromBody]string value)
        public PythonOutputModel Post([FromBody]PythonScriptModel script)
        {

            if (script != null)
            {

                var pom = new PythonOutputModel();

                String outputString = "Python output:\n\n";

                pom.outputText = outputString + runPythonScript(script);

                return pom;
            }
            else
            {
                Debug.WriteLine("Script is null.");
                return null;
            }

        }

        // PUT api/scriptexecution/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/scriptexecution/5
        public void Delete(int id)
        {
        }

        private String runPythonScript(PythonScriptModel script)
        {


            String workingDirectory = ConfigurationManager.AppSettings["ScriptExecutor.LocalWorkingDirectory"];

            //var scriptText = WebDAVScriptFileAccessor.getScriptTextFromPath(ConfigurationManager.AppSettings["OwnCloud.ServerUri"] + ConfigurationManager.AppSettings["OwnCloud.WebDAVServicePath"] + "/" + script.scriptPath, script.webDAVuser);
            var scriptText = script.scriptText;

            //write user info to file
            var scriptFileStream = System.IO.File.Open(workingDirectory + "/WebDAVUser.json", FileMode.Create);
            var streamWriter = new StreamWriter(scriptFileStream);

            streamWriter.Write(JsonConvert.SerializeObject(script.webDAVuser));
            streamWriter.Close();
            scriptFileStream.Close();

            //write the script file locally
            String fileName = workingDirectory + "\\" + Guid.NewGuid().ToString() + ".py";
            Debug.WriteLine("File Name: " + fileName);
            scriptFileStream = System.IO.File.Open(fileName, FileMode.Create);
            streamWriter = new StreamWriter(scriptFileStream);

            streamWriter.Write(scriptText);
            streamWriter.Close();
            scriptFileStream.Close();


            String output = "";

            var startInfo = new ProcessStartInfo("python");
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.FileName = ConfigurationManager.AppSettings["Python.ExecutablePath"];
            startInfo.Arguments = fileName;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            var process = new Process();
            process.StartInfo = startInfo;
            try
            {
                process.Start();

                string s;
                //s = process.StandardOutput.ReadToEnd();
                while ((s = process.StandardOutput.ReadLine()) != null)
                {
                    output += s + System.Environment.NewLine;
                }

                while ((s = process.StandardError.ReadLine()) != null)
                {
                    output += s + System.Environment.NewLine;
                }

                process.WaitForExit();
                process.Close();

            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }

            

            //delete files
            System.IO.File.Delete(fileName);

            return output;
        }
    }
}
