using System;
using System.Net.Sockets;
namespace obd2NET
{
	public class TcpConnection : IOBDConnection
	{
		private String ipAddress { get; set; }
		private Int32 port { get; set; }

		private TcpClient client;


		public TcpConnection(String IpAddress, Int32 Port)
		{
			this.ipAddress = IpAddress;
			this.port = Port;
			client = new TcpClient(this.ipAddress, this.port);
		}


		public void Open() {
			try
			{
				if (!client.Client.Connected)
				{
					client.Client.Connect(ipAddress, port);
					return ;
				}
				else
				{
					Console.WriteLine("client is already connected");
				}
			}
			catch (SocketException e)
			{
				Console.WriteLine("SocketException: {0}", e);

			}

		}
		public void Close() {
			client.GetStream().Close();
			client.Close();
		}
		public ControllerResponse Query(Vehicle.Mode parameterMode, Vehicle.PID parameterID) {
			String responseData = String.Empty;
			try
			{
				Byte[] data = System.Text.Encoding.ASCII.GetBytes(Convert.ToUInt32(parameterMode).ToString("X2") + Convert.ToUInt32(parameterID).ToString("X2") + "\r");
				client.GetStream().Write(data, 0, data.Length);

				Console.WriteLine("Sent: {0}", data.ToString());

				data = new Byte[256];
				Int32 bytes = client.GetStream().Read(data, 0, data.Length);
				responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
				Console.WriteLine("Received: {0}", responseData);
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine("ArgumentNullException: {0}", e);
			}
			catch (SocketException e)
			{
				Console.WriteLine("SocketException: {0}", e);
			}

			return new ControllerResponse(responseData, parameterMode, parameterID);

		}
		public ControllerResponse Query(Vehicle.Mode parameterMode) {
			String responseData = String.Empty;
			try
			{
				Byte[] data = System.Text.Encoding.ASCII.GetBytes(Convert.ToUInt32(parameterMode).ToString("X2") + "\r");
				client.GetStream().Write(data, 0, data.Length);

				Console.WriteLine("Sent: {0}", data.ToString());

				data = new Byte[256];
				Int32 bytes = client.GetStream().Read(data, 0, data.Length);
				responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
				Console.WriteLine("Received: {0}", responseData);
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine("ArgumentNullException: {0}", e);
			}
			catch (SocketException e)
			{
				Console.WriteLine("SocketException: {0}", e);
			}

			return new ControllerResponse(responseData, parameterMode);

		}
	}
}
