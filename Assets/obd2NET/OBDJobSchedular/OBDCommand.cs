using System;

namespace obd2NET.OBDJobSchedular
{
	/*
	* This class hold the object for the OBD command
	*/
	class OBDCommand
	{
		public  String _obdCommandName { get; set; }
		public  String _obdCode { get; set; }

		public  OBDCommand(){}

		public OBDCommand(String obdCommandName, String obdCode)
		{
			_obdCommandName = obdCommandName;
			_obdCode = obdCode;
		}
	}
}
