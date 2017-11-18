using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

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

		ProcessSnapshots();
		UpdateSimulationTime();
	}

	private void ProcessMessages()
	{
		while (_communicationManager.HasMessage())
		{
			var message = _communicationManager.GetMessage();
			switch (message.Type) {
				case MessageType.PLAYER_CONNECTED:
					ProcessPlayerConnectedMessage(message as PlayerConnectedMessage);
					break;
					
				case MessageType.SNAPSHOT:
					ProcessSnapshotMessage(message as SnapshotMessage);
					break;
			}
		}
	}

	private void ProcessPlayerConnectedMessage(PlayerConnectedMessage playerConnectedMessage)
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

	private void ProcessSnapshotMessage(SnapshotMessage snapshotMessage)
	{
		var gameData = snapshotMessage.GameSnapshot;
		_snapshots.Add(gameData);
	}
	
	public int AmountOfBufferedSnapshots = 2;
	
	private readonly List<GameData> _snapshots = new List<GameData>();
	private double _simulationTime = -1.0;
	
	private void UpdateSimulationTime()
	{
		if (_simulationTime >= 0)
		{	
			_simulationTime += Time.deltaTime;
		}
	}
	
	private void ProcessSnapshots()
	{
		if (_snapshots.Count < AmountOfBufferedSnapshots || (_snapshots[1].Time <= _simulationTime && _snapshots.Count - 1 < AmountOfBufferedSnapshots))
		{
			ClientDebug.Log(LogLevel.Info, _clientSharedData.Level, "Insufficient snapshots to render...");
			return;
		}
	
		// If the simulation time is not initialized, we need to set the simulation time equal to the time of the 
		// first snapshot.
		if (_simulationTime < 0)
		{
			_simulationTime = _snapshots[0].Time;
		}
		
		var currentSnapshot = _snapshots[0];
		var nextSnapshot = _snapshots[1];
		if (nextSnapshot.Time <= _simulationTime)
		{
			_snapshots.RemoveAt(0);
			currentSnapshot = _snapshots[0];
			nextSnapshot = _snapshots[1];
		}
	
		foreach (var player in _players)
		{
			var initialPosition = getPlayerDataForPlayerId(player.Id, currentSnapshot).Position;
			var lastPostion = getPlayerDataForPlayerId(player.Id, nextSnapshot).Position;
			var t = (float) ((_simulationTime - currentSnapshot.Time) / (nextSnapshot.Time - currentSnapshot.Time));
			var interpolatedPosition = Vector2.Lerp(initialPosition, lastPostion, t);
			player.SetPosition(interpolatedPosition);	
		}
	}
	
	private PlayerNetworkView GetPlayerWithId(int playerId)
	{
		return _players.FirstOrDefault(player => player.Id == playerId);
	}

	private PlayerData getPlayerDataForPlayerId(int id, GameData gameData)
	{
		return gameData.Players.FirstOrDefault(playerGameData => playerGameData.PlayerId == id);
	}
	
}
