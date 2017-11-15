using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("Player")]
	public Object PlayerPrefab; 
	
	private readonly CommunicationManager _communicationManager = new CommunicationManager();
	private ClientSharedData _clientSharedData;
	private List<PlayerNetworkView> _players = new List<PlayerNetworkView>();
	
	private void Start()
	{
		_clientSharedData = GameObject.FindGameObjectWithTag("ClientSharedData").GetComponent<ClientSharedData>();
	}
	
	public CommunicationManager CommunicationManager
	{
		get 
		{
			return _communicationManager;
		}
	}
	
	private void Update()
	{
		ProcessMessages();
		
		if (Input.GetKeyDown(KeyCode.RightAlt))
		{
			var connectPlayerMessage = ConnectPlayerMessage.CreateConnectPlayerMessageToSend(_clientSharedData.PlayerId);
			_communicationManager.SendMessage(connectPlayerMessage);			
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Sended connect player message.");
		}
		
		var playerInput = new PlayerInput();
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			playerInput.up = true;
			var playerInputMessage = new PlayerInputMessage(_clientSharedData.PlayerId, playerInput);
			_communicationManager.SendMessage(playerInputMessage);			
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Sended player input (UP) message.");
		}
		
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			playerInput.down = true;
			var playerInputMessage = new PlayerInputMessage(_clientSharedData.PlayerId, playerInput);
			_communicationManager.SendMessage(playerInputMessage);			
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Sended player input (DOWN) message.");
		}
		
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			playerInput.left = true;
			var playerInputMessage = new PlayerInputMessage(_clientSharedData.PlayerId, playerInput);
			_communicationManager.SendMessage(playerInputMessage);			
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Sended player input (LEFT) message.");
		}
		
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			playerInput.right = true;
			var playerInputMessage = new PlayerInputMessage(_clientSharedData.PlayerId, playerInput);
			_communicationManager.SendMessage(playerInputMessage);			
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Sended player input (RIGHT) message.");
		}
		
		if (Input.GetKeyDown(KeyCode.Space))
		{
			playerInput.shoot = true;
			var playerInputMessage = new PlayerInputMessage(_clientSharedData.PlayerId, playerInput);
			_communicationManager.SendMessage(playerInputMessage);			
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Client - Sended player input (SHOOT) message.");
		}
	}

	private void ProcessMessages()
	{
		while (_communicationManager.HasMessage())
		{
			var message = _communicationManager.GetMessage();
			switch (message.Type) {
				case MessageType.PLAYER_CONNECTED:
					ProcessPlayerConnected(message as PlayerConnectedMessage);
					break;
					
				case MessageType.SNAPSHOT:
					ProcessSnapshot(message as SnapshotMessage);
					break;
			}
		}
	}

	private void ProcessPlayerConnected(PlayerConnectedMessage playerConnectedMessage)
	{
		var playerGo = Instantiate(PlayerPrefab) as GameObject;
		if (playerGo != null)
		{
			playerGo.name = "PlayerNetworkView " + _clientSharedData.PlayerId;
			var player = playerGo.GetComponent<PlayerNetworkView>();
			player.Id = _clientSharedData.PlayerId;
			_players.Add(player);
		}
	}
	
	private void ProcessSnapshot(SnapshotMessage snapshotMessage)
	{
		Debug.Log("Snapshot received: " + snapshotMessage.GameSnapshot.Time);
	}
	
}
