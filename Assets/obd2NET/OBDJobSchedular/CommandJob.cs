using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obd2NET.OBDJobSchedular
{

	/**
	 * This class represents a job that ObdJobManager will have to execute and
	 * maintain until the job is finished. It is, thereby, the application
	 * representation of an ObdCommand instance plus a state that will be
	 * interpreted and manipulated by ObdGatewayService.
	 */
	class CommandJob
	{

		public enum ObdCommandJobState
		{
			NEW,
			RUNNING,
			FINISHED,
			EXECUTION_ERROR,
			BROKEN_PIPE,
			QUEUE_ERROR,
			NOT_SUPPORTED
		}

		private int _id;
		public OBDCommand _command{ get; set; }
		private ObdCommandJobState _state;
		public  Task _commandTask { get; set; }


		public int getId()
		{
			return _id;
		}

		public void setId(int id)
		{
			_id = id;
		}

		public OBDCommand getCommand()
		{
			return _command;
		}
		/**
		* Sets a new job state.
		*
		* @param state the new job state.
		*/
		public void setState(ObdCommandJobState state)
		{
			_state = state;
		}

		/**
		 * @return job current state.
		 */
		public ObdCommandJobState getState()
		{
			return _state;
		}

	}
}
