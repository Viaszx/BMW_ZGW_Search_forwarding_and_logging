using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BMW_ZGW_Search
{
	public partial class MainForm : Form
	{
		private static readonly int TcpPort1 = 6801;
		private static readonly int TcpPort2 = 6811;
		private static readonly int SshPort = 22;
		private static readonly int UdpPort = 6811;

		private List<Task> listeningTasks;
		private bool isListening;

		private TcpListener tcpListener1;
		private TcpListener tcpListener2;
		private TcpListener tcpListenerSsh;
		private TcpClient tcpClient;
		private UdpClient udpClient;
		private static string ZgwVIN;
		private static string ZgwIP;
		private static string ipAddressHU;
		
		private bool isDragging;
		private Point dragStartPoint;
		private const int UDP_DIAG_PORT = 6811;
		private const int TCP_DIAG_PORT = 6801;

		static byte[] helloZGW = new byte[] { 0, 0, 0, 0, 0, 0x11 };
		static byte[] helloHU = new byte[] { 0, 0, 0, 0x05, 0, 0x01, 0xF5, 0x63, 0x22, 0xF1, 0x50 };
		static byte[] IPHU = new byte[] { 0, 0, 0, 0x05, 0, 0x01, 0xF5, 0x63, 0x22, 0x17, 0x2A };
		byte[] pattern = new byte[] { 0xFF, 0xFF };

		static bool responseReceived = false;
		static bool dateTimeDisplayed = false;
		static bool CheckHeadUnit = false;
		
		private string logFilePath;
		private string currentTime;
		int LogForPort22 = 0;
		
		private readonly object lockObject = new object();
		private static byte[] aesKey = {0x64, 0x72, 0x61, 0x37, 0x2D, 0x69, 0x70, 0x75, 0x31, 0x2D, 0x66, 0x77, 0x2E, 0x78, 0x65, 0x6D};
		private static byte[] aesIV = {0x6F, 0x7F, 0x6F, 0x3F, 0x2F, 0x6F, 0x7F, 0x7F, 0x3F, 0x2F, 0x6F, 0x7F, 0x2F, 0x7F, 0x6F, 0x6F};

		
		private static MainForm instance;

		public MainForm()
		{
			InitializeComponent();
			currentTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss - ");
			MouseDown += MainForm_MouseDown;
			MouseMove += MainForm_MouseMove;
			MouseUp += MainForm_MouseUp;
			listeningTasks = new List<Task>();
			isListening = false;
			instance = this;
			if (DateTime.Now.Year > 2025)
			{
				MessageBox.Show("The current date is after 2025.", "Test version", MessageBoxButtons.OK, MessageBoxIcon.Information);
				Close();
			}
		}
		
		static IPAddress GetBroadcastIP(IPAddress host, IPAddress mask)
		{
			uint hostAddress = BitConverter.ToUInt32(host.GetAddressBytes(), 0);

			uint maskAddress = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			uint broadcastAddress = hostAddress | ~maskAddress;

			byte[] broadcastBytes = BitConverter.GetBytes(broadcastAddress);
			
			return new IPAddress(broadcastBytes);
		}

		static void PingZGW(NetworkInterface networkInterface, TextBox textBox)
		{
			var ipProps = networkInterface.GetIPProperties();
			foreach (var ipAddr in ipProps.UnicastAddresses)
			{
				if (!IPAddress.IsLoopback(ipAddr.Address) && ipAddr.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					var broadcast = GetBroadcastIP(ipAddr.Address, ipAddr.IPv4Mask);

					var ep = new IPEndPoint(broadcast, UDP_DIAG_PORT);
					using (var client = new UdpClient())
					{
						client.Client.ReceiveTimeout = 100;
						try
						{
							if (broadcast.ToString() == "255.255.255.255")
							{
								ep = new IPEndPoint(IPAddress.Broadcast, UDP_DIAG_PORT);
							}

							client.Send(helloZGW, helloZGW.Length, ep);

							IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
							byte[] data = client.Receive(ref sender);
							var ZGW_reply = Encoding.ASCII.GetString(data);
							ProcessZGWReply(ZGW_reply, sender, textBox, networkInterface);
							responseReceived = true;
							break;
						}
						catch (SocketException ex)
						{
							if (ex.SocketErrorCode == SocketError.TimedOut)
							{
								//textBox.AppendText("No response received from " + ep + " (Interface: " + networkInterface.Name + ")");
							}
							else
							{
								//textBox.AppendText("Socket Error: " + ex.Message + " (Interface: " + networkInterface.Name + ")");
							}
						}
					}
				}
			}
		}

		static async void ProcessZGWReply(string reply, EndPoint remoteEndpoint, TextBox textBox, NetworkInterface networkInterface)
		{
			var pattern = @"DIAGADR(.*)BMWMAC(.*)BMWVIN(.*)";
			var match = Regex.Match(reply, pattern);
			if (match.Success)
			{
				var diagAddr = match.Groups[1].Value.Trim();
				var macAddr = match.Groups[2].Value.Trim();
				ZgwVIN = match.Groups[3].Value.Trim();

				ZgwIP = ((IPEndPoint)remoteEndpoint).Address.ToString();
				if (!dateTimeDisplayed)
				{
					textBox.Invoke((MethodInvoker)(() => textBox.AppendText(DateTime.Now.ToString() + Environment.NewLine)));
					dateTimeDisplayed = true;
				}
				textBox.Invoke((MethodInvoker)(() => textBox.AppendText("Interface: " + networkInterface.Name + Environment.NewLine)));
				textBox.Invoke((MethodInvoker)(() => textBox.AppendText("ZGW VIN: " + ZgwVIN + Environment.NewLine)));
				textBox.Invoke((MethodInvoker)(() => textBox.AppendText("ZGW IP: " + ZgwIP + Environment.NewLine)));

				instance.OutputCheckBox.Visible = true;
				instance.StartStopButton_.Visible = true;
				CheckHeadUnit = true;
			}
			if (CheckHeadUnit)
			{
				MainForm instance = new MainForm();
				await instance.ForwardTcpDataHU(textBox);
			}
		}
		private async Task ForwardTcpDataHU(TextBox textBox)
		{
			try
			{
				tcpClient = new TcpClient();
				await tcpClient.ConnectAsync(ZgwIP, TCP_DIAG_PORT);
				NetworkStream networkStream = tcpClient.GetStream();

				// Отправка данных
				await networkStream.WriteAsync(IPHU, 0, IPHU.Length);
				byte[] buffer = new byte[2048];

				try
				{
					while (networkStream.CanRead)
					{
						string currentTime = CurrentDateTime();
						int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
						if (bytesRead <= 0)
						{
							break;
						}
						int index = FindPatternIndex(buffer, pattern);
						if (index != -1)
						{
							byte[] result = new byte[4];
							Array.Copy(buffer, index - 4, result, 0, 4);
							IPAddress ipAddress = new IPAddress(result);
							ipAddressHU = ipAddress.ToString();
							textBox.Invoke((MethodInvoker)(() => textBox.AppendText("Head Unit IP: " + ipAddressHU + Environment.NewLine)));
						}
						else
						{

						}
					}
				}
				catch (Exception ex)
				{
					if (OutputCheckBox.Checked)
					{
						string currentTime = CurrentDateTime();
						textBox.Invoke((MethodInvoker)(() => textBox.AppendText(currentTime + "An error occurred: " + ex.Message)));
					}
				}
			}
			catch (Exception ex)
			{
				if (OutputCheckBox.Checked)
				{
					string currentTime = CurrentDateTime();
					textBox.Invoke((MethodInvoker)(() => textBox.AppendText(currentTime + "An error occurred: ForwardTcpDataHU" + ex.Message)));
				}
			}
		}
		static int FindPatternIndex(byte[] response, byte[] pattern)
		{
			for (int i = 0; i < response.Length - pattern.Length + 1; i++)
			{
				bool found = true;
				for (int j = 0; j < pattern.Length; j++)
				{
					if (response[i + j] != pattern[j])
					{
						found = false;
						break;
					}
				}
				if (found)
				{
					return i;
				}
			}
			return -1;
		}
		private void BtnSearchClick(object sender, EventArgs e)
		{
			OutputCheckBox.Checked = false;
			textBox1.Enabled = true;
			if (isListening)
			{
				textBox1.Enabled = false;
				StopListening();
			}
			responseReceived = false;
			dateTimeDisplayed = false;
			CheckHeadUnit = false;
			textBox1.Clear();
			var interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (var networkInterface in interfaces)
			{
				if (networkInterface.OperationalStatus == OperationalStatus.Up)
				{
					PingZGW(networkInterface, textBox1);
				}
			}

			if (!responseReceived)
			{
				OutputCheckBox.Visible = false;
				StartStopButton_.Visible = false;
				textBox1.AppendText(currentTime + "No ZGW found." + Environment.NewLine);
			}
		}

		private void MainForm_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isDragging = true;
				dragStartPoint = e.Location;
			}
		}

		private void MainForm_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				Point screenPoint = PointToScreen(e.Location);
				Location = new Point(screenPoint.X - dragStartPoint.X, screenPoint.Y - dragStartPoint.Y);
			}
		}

		private void MainForm_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isDragging = false;
			}
		}

		private void CloseButtonClick(object sender, EventArgs e)
		{
			Close();
		}
		
		private void MinimizeButtonClick(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}

		private void ToggleListening()
		{
			textBox1.Enabled = false;
			if (isListening)
			{
				StopListening();
			}
			else
			{
				StartListening();
			}
		}
		
		private void StartListening()
		{
			string currentTime = CurrentDateTime();
			if (isListening)
			{
				AppendToTextBox(currentTime + "Already listening...");
				return;
			}

			tcpListener1 = new TcpListener(IPAddress.Any, TcpPort1);
			tcpListener2 = new TcpListener(IPAddress.Any, TcpPort2);
			tcpListenerSsh = new TcpListener(IPAddress.Any, SshPort);
			udpClient = new UdpClient(UdpPort);

			Task tcpPort1Task = Task.Run(() => HandleTcpPort(tcpListener1, TcpPort1));
			Task tcpPort2Task = Task.Run(() => HandleTcpPort(tcpListener2, TcpPort2));
			Task tcpPortSshTask = Task.Run(() => HandleTcpPort(tcpListenerSsh, SshPort));
			Task udpPortTask = Task.Run(() => HandleUdpPort(udpClient, UdpPort));

			listeningTasks.Add(tcpPort1Task);
			listeningTasks.Add(tcpPort2Task);
			listeningTasks.Add(tcpPortSshTask);
			listeningTasks.Add(udpPortTask);
			if (OutputCheckBox.Checked)
			{
				AppendToTextBox(currentTime + "TCP port 6801 is listening...");
				AppendToTextBox(currentTime + "TCP port 6811 is listening...");
				AppendToTextBox(currentTime + "TCP port 22 is listening...");
				AppendToTextBox(currentTime + "UDP port 6811 is listening...");
			} else {
				AppendToTextBox(currentTime + "Port listening...");
			}
			

			isListening = true;

			if (OutputCheckBox.Checked)
			{
				string currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				string fileName = currentDate + "_" + ZgwVIN + ".log";
				string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				logFilePath = Path.Combine(assemblyPath, fileName);

				string logsFolderPath = Path.Combine(assemblyPath, "Logs");
				if (!Directory.Exists(logsFolderPath))
				{
					Directory.CreateDirectory(logsFolderPath);
				}
				logFilePath = Path.Combine(logsFolderPath, fileName);
				using (File.Create(logFilePath)) { }
			}
			StartStopButton_.Text = "Stop Forwarding";
		}

		private async Task StopListening()
		{
			string currentTime = CurrentDateTime();
			if (!isListening)
			{
				AppendToTextBox(currentTime + "Not listening...");
				return;
			}

			foreach (Task listeningTask in listeningTasks)
			{
				if (!listeningTask.IsCompleted)
				{
					listeningTask.ContinueWith(task => { });
				}
			}

			listeningTasks.Clear();
			if (tcpListener1 != null)
			{
				tcpListener1.Stop();
				tcpListener1 = null;
			}

			if (tcpListener2 != null)
			{
				tcpListener2.Stop();
				tcpListener2 = null;
			}

			if (tcpListenerSsh != null)
			{
				tcpListenerSsh.Stop();
				tcpListenerSsh = null;
			}

			if (udpClient != null)
			{
				udpClient.Close();
				udpClient = null;
			}

			await Task.WhenAll(listeningTasks);

			AppendToTextBox(currentTime + "Listening stopped.");

			isListening = false;
			StartStopButton_.Text = "Start Forwarding";
		}

		private async Task HandleTcpPort(TcpListener tcpListener, int port)
		{
			tcpListener.Start();

			try
			{
				while (true)
				{
					string currentTime = CurrentDateTime();
					TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
					if (OutputCheckBox.Checked)
					{
						AppendToTextBox(currentTime + "TCP connection established with client: " + tcpClient.Client.RemoteEndPoint);
					} else {
						string remoteIP = tcpClient.Client.RemoteEndPoint.ToString();
						string ipAddressWithoutPort = remoteIP.Split(':')[0];
						AppendToTextBox(currentTime + "TCP connection established with client: " + ipAddressWithoutPort);
					}
					Task.Run(() => HandleTcpClient(tcpClient, port));
				}
			}
			catch (Exception ex)
			{
				if (OutputCheckBox.Checked)
				{
					string currentTime = CurrentDateTime();
					string ErrorToLog = currentTime + "An error occurred HandleTcpPort: " + ex.Message;
					WriteToLogFile(ErrorToLog);
				}
			}
			finally
			{
				tcpListener.Stop();
			}
		}
		
		private async Task HandleTcpClient(TcpClient tcpClient, int port)
		{
			using (tcpClient)
				using (NetworkStream stream = tcpClient.GetStream())
			{
				TcpClient tcpClient2 = new TcpClient();
				try
				{
					string currentTime = CurrentDateTime();
					if (port == 22)
					{
						await tcpClient2.ConnectAsync(ipAddressHU, port);
						LogForPort22 = 1;
					}
					else
					{
						await tcpClient2.ConnectAsync(ZgwIP, port);
						LogForPort22 = 0;
					}

					using (NetworkStream stream2 = tcpClient2.GetStream())
					{
						if (OutputCheckBox.Checked)
						{
							AppendToTextBox(currentTime + "Established remote TCP connection: " + tcpClient2.Client.RemoteEndPoint);
						}
						else
						{
							string remoteIP = tcpClient.Client.RemoteEndPoint.ToString();
							string ipAddressWithoutPort = remoteIP.Split(':')[0];
							AppendToTextBox(currentTime + "Established remote TCP connection: " + ipAddressWithoutPort);
						}
						await Task.WhenAll(
							ForwardTcpData(stream, stream2, tcpClient2),
							ForwardTcpData(stream2, stream, tcpClient)
						);
					}
				}
				catch (Exception ex)
				{
					if (OutputCheckBox.Checked)
					{
						string currentTime = CurrentDateTime();
						string ErrorToLog = currentTime + "An error occurred HandleTcpClient: " + ex.Message;
						WriteToLogFile(ErrorToLog);
					}
				}
				finally
				{
					tcpClient2.Close();
				}
			}
		}

		private async Task ForwardTcpData(NetworkStream sourceStream, NetworkStream destinationStream, TcpClient destinationClient)
		{
			byte[] buffer = new byte[10240];
			int destinationPort = 0;
			try
			{
				while (sourceStream.CanRead && destinationStream.CanWrite)
				{
					string currentTime = CurrentDateTime();
					string destinationIP = ((IPEndPoint)destinationClient.Client.RemoteEndPoint).Address.ToString();
					destinationPort = ((IPEndPoint)destinationClient.Client.RemoteEndPoint).Port;
					int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead <= 0)
					{
						break;
					}
					await destinationStream.WriteAsync(buffer, 0, bytesRead);
					if (OutputCheckBox.Checked && destinationPort != 22 && destinationIP != ipAddressHU && LogForPort22 == 0)
					{
						AppendToTextBox(currentTime + "TCP data forwarded to: " + destinationClient.Client.RemoteEndPoint + " data: " + BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ""));
					}
				}
			}
			catch (Exception ex)
			{
				if (OutputCheckBox.Checked)
				{
					string currentTime = CurrentDateTime();
					string ErrorToLog = currentTime + "An error occurred ForwardTcpData: " + ex.Message;
					WriteToLogFile(ErrorToLog);
				}
			}
		}

		private async Task HandleUdpPort(UdpClient udpClient, int port)
		{
			try
			{
				while (true)
				{
					UdpReceiveResult udpReceiveResult = await udpClient.ReceiveAsync();

					IPEndPoint ipEndPoint = udpReceiveResult.RemoteEndPoint;
					byte[] udpData = udpReceiveResult.Buffer;

					AppendToTextBox("Received UDP packet from: " + ipEndPoint);
					await HandleUdpClient(udpClient, udpData, ipEndPoint, port);
				}
			}
			catch (Exception ex)
			{
				if (OutputCheckBox.Checked)
				{
					string ErrorToLog = currentTime + "An error occurred HandleUdpPort: " + ex.Message;
					WriteToLogFile(ErrorToLog);
				}
			}
			finally
			{
				udpClient.Close();
			}
		}

		private async Task HandleUdpClient(UdpClient udpClient, byte[] udpData, IPEndPoint udpEndpoint, int port)
		{
			using (UdpClient udpClient2 = new UdpClient())
			{
				try
				{
					string currentTime = CurrentDateTime();
					await udpClient2.SendAsync(udpData, udpData.Length, ZgwIP.ToString(), port);
					AppendToTextBox(currentTime + "UDP packet forwarded to client: " + udpEndpoint);
					if (OutputCheckBox.Checked)
					{
						AppendToTextBox(currentTime + "Requested data: " + BitConverter.ToString(udpData).Replace("-", ""));
					}
					UdpReceiveResult udpReceiveResult = await udpClient2.ReceiveAsync();
					IPEndPoint receivedEndpoint = udpReceiveResult.RemoteEndPoint;
					byte[] receivedData = udpReceiveResult.Buffer;
					AppendToTextBox(currentTime + "Received UDP response from: " + receivedEndpoint);
					if (OutputCheckBox.Checked)
					{
						AppendToTextBox(currentTime + "Response data: " + BitConverter.ToString(receivedData).Replace("-", ""));
					}
					await udpClient.SendAsync(receivedData, receivedData.Length, udpEndpoint);
					if (OutputCheckBox.Checked)
					{
						AppendToTextBox(currentTime + "UDP response forwarded to original sender: " + udpEndpoint);
					}
				}
				catch (Exception ex)
				{
					if (OutputCheckBox.Checked)
					{
						string currentTime = CurrentDateTime();
						string ErrorToLog = currentTime + "An error occurred HandleUdpClient: " + ex.Message;
						WriteToLogFile(ErrorToLog);
					}
				}
			}
		}

		private void AppendToTextBox(string text)
		{
			if (textBox1.InvokeRequired)
			{
				textBox1.Invoke(new Action<string>(AppendToTextBox), new object[] { text });
			}
			else
			{
				if (OutputCheckBox.Checked)
				{
					WriteToLogFile(text);
				}
				textBox1.AppendText(text + Environment.NewLine);
			}
		}
		
		void StartStopButton_Click(object sender, EventArgs e)
		{
			textBox1.Clear();
			ToggleListening();
			OutputCheckBox.Enabled = !isListening;
		}
		
		private void WriteToLogFile(string text)
		{
			if (!string.IsNullOrEmpty(logFilePath))
			{
				byte[] encryptedBytes = EncryptStringToBytes(text);
				string base64Encoded = Convert.ToBase64String(encryptedBytes);
				int padding = base64Encoded.Length % 4;
				if (padding != 0)
				{
					base64Encoded += new string('=', 4 - padding);
				}
				File.AppendAllText(logFilePath, base64Encoded + Environment.NewLine, Encoding.UTF8);
			}
		}
		
		private static byte[] EncryptStringToBytes(string plainText)
		{
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = aesKey;
				aesAlg.IV = aesIV;

				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
						cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
						cryptoStream.FlushFinalBlock();
						return memoryStream.ToArray();
					}
				}
			}
		}
		
		private string CurrentDateTime()
		{
			return DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff - ");
		}
	}
}