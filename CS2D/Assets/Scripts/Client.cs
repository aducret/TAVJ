using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
	[Header("Client")] 
	public PlayerController PlayerController;
	
	[Header("Connection")] 
	public string ServerIP;
	public int ServerPort;
	public int ClientPort;
	
	private Channel _channel;
	private CommunicationManager _communicationManager;
	private ClientSharedData _clientSharedData;
	
	private void Start()
	{
		_channel = new Channel(ServerIP, ClientPort, ServerPort);
		_clientSharedData = GameObject.FindGameObjectWithTag("ClientSharedData").GetComponent<ClientSharedData>();
		_communicationManager = PlayerController.CommunicationManager;
	}

	private void OnDestroy()
	{
		_channel.Disconnect();
	}

	private void Update()
	{
		SendMessages();
			
		var inPacket = _channel.GetPacket();
		if (inPacket != null) 
		{
			var bitBuffer = inPacket.buffer;
			var messageCount = bitBuffer.GetInt();
			for (var i = 0; i < messageCount; i++)
			{
				var serverMessage = ReadServerMessage(bitBuffer);
				if (serverMessage != null)
				{
					_communicationManager.ReceiveMessage(serverMessage);
				}
			}
		}
	}
	
	private void SendMessages()
	{
		var packet = _communicationManager.BuildPacket();
		if (packet != null)
		{
			_channel.Send(packet);	
		}
	}
	
	private Message ReadServerMessage(BitBuffer bitBuffer)
	{
		var messageType = bitBuffer.GetEnum<MessageType>((int)MessageType.TOTAL);
		Message serverMessage = null;
		switch (messageType)
		{
			case MessageType.PLAYER_CONNECTED:
				serverMessage = PlayerConnectedMessage.CreatePlayerConnectedMessageToReceive();
				break;
				
			case MessageType.PLAYER_DISCONNECTED:
				serverMessage = PlayerDisconnectedMessage.CreatePlayerDisconnectedMessageToReceive();
				break;
				
			case MessageType.SNAPSHOT:
				serverMessage = new SnapshotMessage();
				break;
			
			case MessageType.ACK_RELIABLE_MAX_WAIT_TIME:
				serverMessage = AckReliableMessage.CreateAckReliableMessageMessageToReceive();			
				break;
				
			case MessageType.ACK_RELIABLE_SEND_EVERY_PACKET:
				serverMessage = AckReliableSendEveryFrameMessage.CreateAckReliableSendEveryFrameMessageMessageToReceive();
				break;
				
			default:
                return null;
		}
		serverMessage.Load(bitBuffer);
		LogServerMessage(messageType, serverMessage);

		return serverMessage;
	}

	private void LogServerMessage(MessageType messageType, Message serverMessage)
	{
		if (_clientSharedData.Level.Equals(LogLevel.Off))
			return;
		
		switch (messageType)
		{
			case MessageType.PLAYER_CONNECTED:
				ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, 
					"Client - Player " + ((PlayerConnectedMessage)serverMessage).PlayerId + " connected received.");
				break;
				
			case MessageType.PLAYER_DISCONNECTED:
				ClientDebug.Log(LogLevel.Info, _clientSharedData.Level,
					"Client - Player " + ((PlayerDisconnectedMessage)serverMessage).PlayerId + " disconnected received.");
				break;

			case MessageType.SNAPSHOT:
				ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Snapshot received.");
				ClientDebug.Log(LogLevel.Full, _clientSharedData.Level,
					"Client - Snapshot " + ((SnapshotMessage)serverMessage).GameSnapshot.Time + " received.");	
				break;
			
			case MessageType.ACK_RELIABLE_MAX_WAIT_TIME:
				ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - ACK reliable max wait time received.");
				break;
				
			case MessageType.ACK_RELIABLE_SEND_EVERY_PACKET:
				ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - ACK reliable send every packey received.");
				break;
				
			default:
				ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Got a server message that cannot be understood.");
				break;
		}
	}
	
}
