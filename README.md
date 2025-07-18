# High Level Riptide

**High Level Riptide (HLRiptide)** is a high-level Unity networking abstraction built on top of [RiptideNetworking](https://github.com/RiptideNetworking/Riptide). It provides structured support for networked objects, authority-based command execution, and simplified network lifecycle management.

## Features

- High-level API for networked GameObjects and MonoBehaviours
- NetworkedCommand system with argument serialization and authority permissions
- Automatic object spawning and destruction during connection and disconnection
- Synchronization of position, rotation, and scale for networked objects
- Authority model for determining whether the server or client can issue commands
- Unified interface for server, client, and host modes

---

## Getting Started

### Prerequisites

- Unity 2020 or newer
- [RiptideNetworking](https://github.com/RiptideNetworking/Riptide)

### Installation

1. Clone or download this repository.
2. Add the `HLRiptide` folder to your Unity project's `Assets` directory.
3. Ensure RiptideNetworking is included or referenced in your project.
4. Add a `NetworkManager` GameObject to your scene and assign the `NetworkManager` component.

---

## Basic Usage

### 1. Creating a Networked Object

Add the `NetworkedObject` component to any prefab you want to synchronize across the network. Then include that prefab in the `networkedObjectPrefabs` array on the `NetworkManager`.

```csharp
// Example of spawning a networked object from the server
networkedObject.SpawnOnNetwork();
```

### 2. Creating a Custom NetworkedBehaviour

Inherit from `NetworkedBehaviour` to define a component that can register and execute networked commands.

```csharp
public class PlayerController : NetworkedBehaviour
{
    public override void OnRegisterCommands()
    {
        RegisterNetworkedCommand<Vector3>(Move, NetworkPermission.Client);
    }

    private void Move(Vector3 direction)
    {
        transform.position += direction;
    }

    public void RequestMove(Vector3 dir)
    {
        ExecuteCommandOnNetwork(Move, dir);
    }
}
```
### 3. Starting a Server or Client

```csharp
// Starting the server
NetworkManager.Singleton.StartServer(7777, 10, defaultSceneIndex: 0);

// Starting the client
NetworkManager.Singleton.StartClient("127.0.0.1", 7777);
```

---

### Command Authority
Each networked command can specify which side has authority to invoke it:

- `Server`: Only the server can issue the command

- `Client`: Only the client that owns the object can issue the command

```csharp
RegisterNetworkedCommand<Vector3>(SomeCommand, NetworkPermission.Server);
```

---

### Event System

The `NetworkManager` provides event hooks for key moments in the network lifecycle, including:

- `OnServerStart`
- `OnServerClientBeginConnect`
- `OnServerClientFinishConnect`
- `OnServerClientDisconnect`
- `OnLocalClientBeginConnect`
- `OnLocalClientFinishConnect`
- `OnLocalClientDisconnect`
- `OnTick`(called every network tick)
  
See `NetworkManager.cs` for the full list of events and their signatures.

---

### Architecture Overview

- **NetworkManager**
  Core singleton managing server/client lifecycle, object/command containers, and Riptide interface.

- **NetworkedObject**
  Attach to prefabs to make them spawnable and manageable over the network. Tracks authority, spawn state, and transform data.

- **NetworkedBehaviour**
  Custom MonoBehaviour class that allows high-level command registration and authority-aware logic.

- **NetworkedCommand<T>**
  Encapsulates argument-based commands for network invocation. Supports permission control and custom serialization.

