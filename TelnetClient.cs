using System;
using System.Net.Sockets;
using System.Text;

namespace NathanAlden.FirefoxAutoRefresh
{
	public class TelnetClient
	{
		private readonly Action _connectedDelegate;
		private readonly StringBuilder _data = new StringBuilder();
		private readonly string _host;
		private readonly ushort _port;
		private readonly Action _readyDelegate;
		private TcpClient _client;

		public TelnetClient(string host, ushort port, Action connectedDelegate, Action readyDelegate)
		{
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (connectedDelegate == null)
			{
				throw new ArgumentNullException("connectedDelegate");
			}
			if (readyDelegate == null)
			{
				throw new ArgumentNullException("readyDelegate");
			}

			_host = host;
			_port = port;
			_connectedDelegate = connectedDelegate;
			_readyDelegate = readyDelegate;
		}

		public string Host
		{
			get
			{
				return _host;
			}
		}

		public ushort Port
		{
			get
			{
				return _port;
			}
		}

		public bool Ready
		{
			get;
			private set;
		}

		public void Connect()
		{
			Disconnect();

			_client = new TcpClient(_host, _port);

			_connectedDelegate();

			NetworkStream stream = _client.GetStream();
			var buffer = new byte[_client.ReceiveBufferSize];

			stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
		}

		public void WriteBrowserReload()
		{
			NetworkStream stream = _client.GetStream();
			byte[] buffer = Encoding.ASCII.GetBytes(";BrowserReload();");

			stream.BeginWrite(buffer, 0, buffer.Length, WriteCallback, null);
		}

		public void Disconnect()
		{
			if (_client == null)
			{
				return;
			}

			_client.Close();
			_client = null;
			_data.Clear();
			Ready = false;
		}

		private void ReadCallback(IAsyncResult ar)
		{
			NetworkStream stream = _client.GetStream();
			var buffer = (byte[])ar.AsyncState;
			int bytesRead = stream.EndRead(ar);

			if (bytesRead == 0)
			{
				return;
			}

			string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

			_data.Append(data);

			if (_data.ToString().EndsWith("repl> "))
			{
				Ready = true;
				_readyDelegate();
			}
			else
			{
				stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
			}
		}

		private void WriteCallback(IAsyncResult ar)
		{
			NetworkStream stream = _client.GetStream();

			stream.EndWrite(ar);
		}
	}
}