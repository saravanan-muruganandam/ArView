using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace obd2NET.OBDJobSchedular
{
	 class  OBDJobManager
	{

		private static List<CommandJob> runningJobsList = new List<CommandJob> { };

		

		public  void executeCommandJob(String  commandName) {
			OBDCommand command = new OBDCommand(commandName, "0000");
			try
			{
				if (!runningJobsList.Any(c => (c.getCommand()._obdCommandName == command._obdCommandName) && (c._commandTask.Status == TaskStatus.Running)))
				{
					CommandJob obdCommandJob = new CommandJob();
					obdCommandJob._command = command;
					obdCommandJob._commandTask = Task.Run(() => StartSendingCommand(command));
					Debug.Log("NEW TASK ADDED FOR : " + command._obdCommandName);
					obdCommandJob._commandTask.Start();
					obdCommandJob.setState(CommandJob.ObdCommandJobState.RUNNING);
					runningJobsList.Add(obdCommandJob);
					Debug.Log("NEW TASK ADDED FOR : " +command._obdCommandName);
				}
				else if (runningJobsList.Any(c => (c.getCommand()._obdCommandName == command._obdCommandName) && !(c._commandTask.Status == TaskStatus.Running)))
				{
					CommandJob obdCommandJob = runningJobsList.Find(c => (c.getCommand()._obdCommandName == command._obdCommandName));
					obdCommandJob._commandTask.Start();
					Debug.Log("RESTARTING TASK ADDED FOR : " + command._obdCommandName);
				}
			}
			catch
			{
			}
			finally
				{

			}

	}
		private String StartSendingCommand(OBDCommand command)
		{
			
			while (true)
			{

				return new System.Random().Next(200,210).ToString();

			}
		}
	}
}
