﻿using System;
using System.Activities.XamlIntegration;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using rpa_robot.Classes;
using Serilog;
using System.Threading;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using rpa_robot.Formats;

namespace rpa_robot
{
    public class Handler
    {
        /*
         * RunWorkFlow()
         * MainLoop() 
         * RobotAsyncListenerFromServiceFun()
         * RobotFun()
         */
        
        public static void RunWorkFlow()
        {
            if (!string.IsNullOrEmpty(Globals.WorkflowFilePath))
            {
                try
                {
                    var workflow = ActivityXamlServices.Load(Globals.WorkflowFilePath);
                    var wa = new WorkflowApplication(workflow);
                    wa.Extensions.Add(new AsyncTrackingParticipant());
                    wa.Run();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public static void RobotFun(object sender, DoWorkEventArgs e)
        {
            while (true)
            {

                if ((Globals.RobotAsyncListenerFromService.ProcessQueue.Count > 0) || (Globals.LogQueue.Count > 0))
                {
                    if ((Globals.RobotAsyncListenerFromService.ProcessQueue.Count > 0)) 
                    {
                        lock (Globals.RobotAsyncListenerFromService.ProcessQueue)
                        {

                            //Log.Information(Globals.RobotAsyncListenerFromService.ProcessQueue.Dequeue() + "FROM Q");
                            //Globals.List.Add(new RobotReport("hi", Info.INFO));
                            //Globals.AddToList("hi", Info.INFO);
                            var CommandFromService = Globals.RobotAsyncListenerFromService.ProcessQueue.Dequeue();
                            ServiceRobotCMD CMD = JsonConvert.DeserializeObject<ServiceRobotCMD>(CommandFromService);
                            if (CMD.Command.Equals("print"))
                            {
                                Globals.LogsTxtBox.AppendText(CMD.Data + "\n");
                            }

                        }
                    }
                    if ((Globals.LogQueue.Count > 0)) 
                    {
                        string log = "";
                        lock (Globals.LogQueue)
                        {
                             log = Globals.LogQueue.Dequeue();
                            
                        }
                        Globals.RobotAsyncClientFromService.SendToSocket(log);
                    }
                    
                }
                else
                {
                    //== THIS LINE IS WRITTEN TO AVOID THE OVEDHEAD DUE TO THE WHILE LOOP, LOOPING ON NOTHING ==//
                    Thread.Sleep(100);
                }
            }

        }
        public static void RobotAsyncListenerFromServiceFun(object sender, DoWorkEventArgs e)
        {
            Globals.RobotAsyncListenerFromService.StartListening();
        }
        //public static void RobotFun(object sender, DoWorkEventArgs e)
        //{
        //    MainLoop();
        //}
    }
}
