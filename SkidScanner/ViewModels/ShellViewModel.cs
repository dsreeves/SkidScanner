using Caliburn.Micro;
using CoreScanner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using SkidScanner.Models;
using SkidScanner.Views;
using System.Windows;

namespace SkidScanner.ViewModels
{
	public class ShellViewModel : Screen, INotifyPropertyChanged
	{
		private string _Skid;
		private bool reset = true;
		public bool TagEnable { get { return reset; } set { reset = value; } }
		public string Skid
		{
			get {return _Skid; }
			set
			{
				_Skid = value;

				if (_Skid.Length == 6 && _Skid.Contains("-"))
				{
					MessageBoxButton btn = MessageBoxButton.YesNo;
					MessageBoxImage img = MessageBoxImage.Question;
					MessageBoxResult rslt = MessageBox.Show($"Is {_Skid} the correct Tag number?", "Stocktag", btn, img);

					switch (rslt)
					{
						case MessageBoxResult.Yes:
							_skid = new SkidLst(_Skid);
							if (_skid._iniSuccess)
							{
								_tagAcq = true;
								SkidColor = Brushes.White;
								LastScanned = "Scan First Pump";
								TagEnable = false;
								
							}
							else
							{
								MessageBoxButton btn2 = MessageBoxButton.OK;
								MessageBoxImage img2 = MessageBoxImage.Question;
								MessageBox.Show($"Stocktag already scanned!", "Stocktag", btn2, img2);
								Skid = "";
							}
							break;

						case MessageBoxResult.No:
							_tagAcq = false;
							SkidColor = Brushes.Yellow;
							break;
						default:
							_tagAcq = false;
							SkidColor = Brushes.Yellow;
							break;
					}

				}
			}

		}
		public int Count { get; set; } = 0;
		public string LastScanned { get; set; } = "Enter Stocktag #";
		public SolidColorBrush LblColor { get; set; } = Brushes.Green;
		public SolidColorBrush SkidColor { get; set; } = Brushes.Yellow;
		public SkidLst _skid;
		private bool _tagAcq = false;

		#region Properties/Var
		private CCoreScanner cCoreScannerClass = new CCoreScanner();
		private string outXML;
		private int status = 1;

		private bool ScanDisabled = false;

		public bool sConnected = false;
		public string _Operator { get; set; }
		public string _Fixture { get; set; }
		public string _DateTime { get; set; }
		public string _RawIN { get; set; }
		public enum ExecCodes
		{
			//Scanner SDK Commands
			GET_VERSION = 1000,
			REGISTER_FOR_EVENTS = 1001,
			UNREGISTER_FOR_EVENTS = 1002,

			//General Commands
			AIM_OFF = 2002,
			AIM_ON = 2003,
			DEVICE_PULL_TRIGGER = 2011,
			DEVICE_RELEASE_TRIGGER = 2012,
			SCAN_DISABLE = 2013,
			SCAN_ENABLE = 2014,
			SET_PARAMETER_DEFAULTS = 2015,
			DEVICE_SET_PARAMETERS = 2016,
			SET_PARAMETER_PERSISTANCE = 2017,
			REBOOT_SCANNER = 2019,

			//Operational modes
			DEVICE_CAPTURE_IMAGE = 3000,
			DEVICE_CAPTURE_BARCODE = 3500,
			DEVICE_CAPTURE_VIDEO = 4000,

			//Attributes
			ATTR_GETALL = 5000,
			ATTR_GET = 5001,
			ATTR_SET = 5004,
			ATTR_STORE = 5005,

			//Used to invoke beeps, leds, ect..
			SET_ACTION = 6000,
			_1HighS = 0,
			_2HighS = 1,
			_3HighS = 2,
			_4HighS = 3,
			_5HighS = 4,
			_1LowS = 5,
			_2LowS = 6,
			_3LowS = 7,
			_4LowS = 8,
			_5LowS = 9,
			_1HighL = 10,
			_2HighL = 11,
			_3HighL = 12,
			_4HighL = 13,
			_5HighL = 14,
			_1LowL = 15,
			_2LowL = 16,
			_3LowL = 17,
			_4LowL = 18,
			_5LowL = 19,
			_HighLow = 22,
			_LowHigh = 23,
			_HLH = 24,
			_LHL = 25,
			_HHLL = 26,
			_GreenLEDOff = 42,
			_GreenLEDOn = 43,
			_RedLEDOff = 48,
			_RedLEDOn = 47,

			//Start(0), Stop(1)
			HostTriggerSession = 6005,

			//Event ID's
			SUBSCRIBE_BARCODE = 1,
			SUBSCRIBE_IMAGE = 2,
			SUBSCRIBE_VIDEO = 4,
			SUBSCRIBE_RMD = 8,
			SUBSCRIBE_PNP = 16,
			SUBSCRIBE_OTHER = 32,
		};
		#endregion


		public void Main()
		{
			short[] scannerTypes = new short[1];                        // Scanner Types you are interested in
			scannerTypes[0] = 2;                                        // 1 for all scanner types, 2 for SNAPI mode only
			short numberOfScannerTypes = 1;                             // Size of the scannerTypes array
			cCoreScannerClass.Open(0, scannerTypes, numberOfScannerTypes, out status);
			ScannerDisable();
			IniScanner();
			CreateNewBarcodeEvent();
			ScannerEnable();
		}

		private void IniScanner()
		{
			InvokeBeep(ExecCodes._5HighS);
			InvokeBeep(ExecCodes._5LowS);
			InvokeBeep(ExecCodes._5HighS);
			InvokeBeep(ExecCodes._5LowS);
			ChangeLed(ExecCodes._GreenLEDOn);
			Thread.Sleep(500);
			ChangeLed(ExecCodes._RedLEDOn);
			Thread.Sleep(500);
			ChangeLed(ExecCodes._GreenLEDOn);
			Thread.Sleep(500);
			ChangeLed(ExecCodes._RedLEDOn);
			Thread.Sleep(500);
			ChangeLed(ExecCodes._GreenLEDOn);
		}

		private void CreateNewBarcodeEvent()
		{
			//Barcode scan event
			cCoreScannerClass.BarcodeEvent += new _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);
			// Method for Subscribe events
			string inXML = "<inArgs>" +
								"<cmdArgs>" +
									"<arg-int>1</arg-int>" +            // Number of events you want to subscribe
									"<arg-int>1</arg-int>" +            // Comma separated event IDs
								"</cmdArgs>" +
							"</inArgs>";
			cCoreScannerClass.ExecCommand(1001, ref inXML, out outXML, out status);
			Thread.Sleep(200);
		}

		private void OnBarcodeEvent(short eventType, ref string pscanData)
		{
			string dat = pscanData;
			if (!_tagAcq) { }
			if (dat != "")
			{
				DecodeXml(dat);
				if (_RawIN.Contains("G4D3"))
				{
					_DateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
					if (_skid.Add(_RawIN))
					{
						ChangeLed(ExecCodes._GreenLEDOn);
						LastScanned = _RawIN.Remove(12);
						LblColor = Brushes.Green;
						Count++;
						CheckCount();
					}
					else
					{
						ChangeLed(ExecCodes._RedLEDOn);
						InvokeBeep(ExecCodes._HLH);
						LblColor = Brushes.Yellow;
					}
				}
			}
		}

		private void CheckCount()
		{
			if (Count >= 144 && _tagAcq)
			{
				MessageBoxButton btn = MessageBoxButton.OK;
				MessageBoxImage img = MessageBoxImage.Question;
				MessageBox.Show($"Skid Finished! Start new skid!", "Done", btn, img);

				Count = 0;
				TagEnable = true;
				_tagAcq = false;
				LastScanned = "Enter Stocktag #";
				Skid = "";
				SkidColor = Brushes.Yellow;
			}
		}

		private void ScannerDisable()
		{
			ChangeLed(ExecCodes._RedLEDOn);
			ExecMiscCmd(ExecCodes.SCAN_DISABLE);
		}

		private void ScannerEnable()
		{
			ChangeLed(ExecCodes._GreenLEDOn);
			ExecMiscCmd(ExecCodes.SCAN_ENABLE);
		}

		private void InvokeBeep(ExecCodes value, int id = 1)
		{
			string inXML = "<inArgs>" +
								$"<scannerID>{id.ToString()}</scannerID>" + //Scanner ID
								"<cmdArgs>" +
									$"<arg-int>{(int)value}</arg-int>" +
								"</cmdArgs>" +
							"</inArgs>";
			cCoreScannerClass.ExecCommand((int)ExecCodes.SET_ACTION, ref inXML, out outXML, out status);
		}

		/// <summary>
		/// Change LED state/color
		/// </summary>
		/// <param name="value"></param>
		/// <param name="id">Scanner ID</param>
		private void ChangeLed(ExecCodes value, int id = 1)
		{
			string outXML;
			string inXML = "<inArgs>" +
								$"<scannerID>{id.ToString()}</scannerID>" + //Scanner ID
								"<cmdArgs>" +
									$"<arg-int>{(int)value}</arg-int>" +
								"</cmdArgs>" +
							"</inArgs>";
			cCoreScannerClass.ExecCommand((int)ExecCodes.SET_ACTION, ref inXML, out outXML, out status);
		}

		/// <summary>
		/// Misc. scanner function call.
		/// </summary>
		/// <param name="value">opcode</param>
		/// <param name="id">scanner id</param>
		private void ExecMiscCmd(ExecCodes opcode, int id = 1)
		{
			ScanDisabled = ((int)opcode == 2013) ? true : false;
			string inXML = "<inArgs>" +
								$"<scannerID>{id.ToString()}</scannerID>" +
								"</inArgs>"; ;

			cCoreScannerClass.ExecCommand((int)opcode, ref inXML, out outXML, out status);

		}

		private void DecodeXml(string strXml)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(strXml);

			string strData = String.Empty;
			string barcode = xmlDoc.DocumentElement.GetElementsByTagName("datalabel").Item(0).InnerText;
			string symbology = xmlDoc.DocumentElement.GetElementsByTagName("datatype").Item(0).InnerText;
			string[] numbers = barcode.Split(' ');

			foreach (string number in numbers)
			{
				if (String.IsNullOrEmpty(number))
				{
					break;
				}

				strData += ((char)Convert.ToInt32(number, 16)).ToString();
			}

			_RawIN = strData;
		}

		public void Disconnect()
		{
			cCoreScannerClass.Close(0, out status);
		}

		
	}
}
