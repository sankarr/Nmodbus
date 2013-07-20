using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using log4net;
using Modbus.IO;
using Modbus.Message;
using Unme.Common;
using System.Collections.Generic;

namespace Modbus.Device
{
	/// <summary>
	/// Modbus serial slave device.
	/// </summary>
	public class ModbusSerialSlave : ModbusSlave
	{
		private static readonly ILog _logger = LogManager.GetLogger(typeof(ModbusSerialSlave));


        static private Dictionary<byte, Data.DataStore> m_internalMap;

        
        

        
        private ModbusSerialSlave(byte unitId, ModbusTransport transport)
			: base(unitId, transport)
		{
		}



		private ModbusSerialTransport SerialTransport
		{
			get
			{
				var transport = Transport as ModbusSerialTransport;
				if (transport == null)
					throw new ObjectDisposedException("SerialTransport");

				return transport;
			}
		}

		/// <summary>
		/// Modbus ASCII slave factory method.
		/// </summary>
		public static ModbusSerialSlave CreateAscii(byte unitId, SerialPort serialPort)
		{
			if (serialPort == null)
				throw new ArgumentNullException("serialPort");
			
			return CreateAscii(unitId, new SerialPortAdapter(serialPort));
		}

		/// <summary>
		/// Modbus ASCII slave factory method.
		/// </summary>
		public static ModbusSerialSlave CreateAscii(byte unitId, IStreamResource streamResource)
		{
			if (streamResource == null)
				throw new ArgumentNullException("streamResource");
			
			return new ModbusSerialSlave(unitId, new ModbusAsciiTransport(streamResource));
		}

		/// <summary>
		/// Modbus RTU slave factory method.
		/// </summary>
		public static ModbusSerialSlave CreateRtu(byte unitId, SerialPort serialPort)
		{
			if (serialPort == null)
				throw new ArgumentNullException("serialPort");

			return CreateRtu(unitId, new SerialPortAdapter(serialPort));
		}

        /// <summary>
        /// This to create an rtu for multiple data store and multiple modbus IDs on a single port
        /// </summary>
        /// <param name="_internalMap"></param>
        /// <param name="serialPort"></param>
        /// <returns></returns>
        public static ModbusSerialSlave CreateRtu( Dictionary<byte, Data.DataStore> _internalMap, SerialPort serialPort)
        {
            if (serialPort == null)
                throw new ArgumentNullException("serialPort");

            m_internalMap = _internalMap;

            byte[] keys = new byte[m_internalMap.Keys.Count];
            int i=0;
            foreach(byte key in m_internalMap.Keys){
                keys[i++] = key;
            }


            return CreateRtu(keys[0], new SerialPortAdapter(serialPort));
        }

		/// <summary>
		/// Modbus RTU slave factory method.
		/// </summary>
		public static ModbusSerialSlave CreateRtu(byte unitId, IStreamResource streamResource)
		{
			if (streamResource == null)
				throw new ArgumentNullException("streamResource");

			return new ModbusSerialSlave(unitId, new ModbusRtuTransport(streamResource));
		}


              
		/// <summary>
		/// Start slave listening for requests.
		/// </summary>
		public  override void Listen()
		{
            // This external Variable can stop this process
            stop = false;

			while (!stop)
			{
				try
				{
					try
					{
						// read request and build message
						byte[] frame = SerialTransport.ReadRequest();
						IModbusMessage request = ModbusMessageFactory.CreateModbusRequest(frame);

                        // here for debugging purposes writes what port is being read
    //                    using (StreamWriter outHandle = new StreamWriter("Output.txt", true))
    //                    {

    //                        if (request.FunctionCode == Modbus.ReadHoldingRegisters)
    //                        {
    //                            outHandle.WriteLine("{0} Reading Port {1} functioncode {2} startadress {3} points {4}", DateTime.Now.ToString(), request.SlaveAddress, request.FunctionCode,
    //                                ((ReadHoldingInputRegistersRequest)request).StartAddress, ((ReadHoldingInputRegistersRequest)request).NumberOfPoints);

    //                        }
    //                        else if (request.FunctionCode == Modbus.ReadInputRegisters)
    //                        {
    //                            outHandle.WriteLine("{0} Reading Port {1} functioncode {2} startadress {3} points {4}", DateTime.Now.ToString(), request.SlaveAddress, request.FunctionCode,
    //((ReadHoldingInputRegistersRequest)request).StartAddress, ((ReadHoldingInputRegistersRequest)request).NumberOfPoints);
    //                        }
    //                        //response = ReadRegisters((ReadHoldingInputRegistersRequest) request, DataStore, DataStore.HoldingRegisters);
    //                //break;
    //            //case Modbus.ReadInputRegisters:


    //                        //outHandle.WriteLine("{0} Reading Port {1}", DateTime.Now.ToString(), request.SlaveAddress, request.FunctionCode );
    //                    }

						if (SerialTransport.CheckFrame && !SerialTransport.ChecksumsMatch(request, frame))
						{
							string errorMessage = String.Format(CultureInfo.InvariantCulture, "Checksums failed to match {0} != {1}", request.MessageFrame.Join(", "), frame.Join(", "));
							_logger.Error(errorMessage);
							throw new IOException(errorMessage);
						}

						// only service requests addressed to this particular slave

                        if (m_internalMap != null)
                        {
                            if (!doesIdExist(request.SlaveAddress))
                            {
                                _logger.DebugFormat("NModbus Slave {0} ignoring request intended for NModbus Slave {1}", UnitId, request.SlaveAddress);
                                continue;
                            }
                        }
                        else
                        {
                            if (request.SlaveAddress != UnitId)
                            {
                                _logger.DebugFormat("NModbus Slave {0} ignoring request intended for NModbus Slave {1}", UnitId, request.SlaveAddress);
                                continue;
                            }

                        }

						// perform action

                        if (m_internalMap != null)
                        {
                            DataStore = m_internalMap[request.SlaveAddress];
                        }

                        
                        
                        IModbusMessage response = ApplyRequest(request);

						// write response
						SerialTransport.Write(response);
					}
					catch (IOException ioe)
					{
						_logger.ErrorFormat("IO Exception encountered while listening for requests - {0}", ioe.Message);
						SerialTransport.DiscardInBuffer();
					}
					catch (TimeoutException te)
					{
						_logger.ErrorFormat("Timeout Exception encountered while listening for requests - {0}", te.Message);
						SerialTransport.DiscardInBuffer();
					}

					// TODO better exception handling here, missing FormatException, NotImplemented...
				}
				catch (InvalidOperationException)
				{
					// when the underlying transport is disposed
					break;
				}
			}
		}

        /// <summary>
        /// Checks to see if key exists in the data table
        /// </summary>
        /// <param name="keyToCheck"></param>
        /// <returns></returns>
        bool doesIdExist(byte keyToCheck)
        {
            
            foreach (byte key in m_internalMap.Keys)
            {
                if (key == keyToCheck)
                {
                    return true;
                }
            }

            return false;
        }


        
	}
}
