Nmodbus
=======

A fork of Nmodbus http://code.google.com/p/nmodbus/ with the following stuff added:

Modbus implementation for .net with some added features

The following things were added:

The ability to have multiple slave ids for a single connection. For example a 485 line might have multiple pieces of equipment. To simulate that you would do the following:

        DataStore data1 = DataStoreFactory.CreateDefaultDataStore();
        DataStore data2 = DataStoreFactory.CreateDefaultDataStore();
        DataStore data3 = DataStoreFactory.CreateDefaultDataStore();
        DataStore data3 = DataStoreFactory.CreateDefaultDataStore();
        
       Dictionary<byte, DataStore> map = new Dictionary<byte, DataStore>();
       
      byte unitID1 = 1;
      byte unitID2 = 2;
      byte unitID3 = 3;
      byte unitID4 = 4;



       map.Add(unitID1, data1);
       map.Add(unitID2, data2);
       map.Add(unitID3, data3);
       map.Add(unitID4, data4);
       
       
       Also added the ability to specify and modbus ID for tcp/ip connection:
       
       CreateIp(TcpClient tcpClient, byte modbusId)
       
       To use do the following:
       master = ModbusIpMaster.CreateIp(socket, modbusId);
