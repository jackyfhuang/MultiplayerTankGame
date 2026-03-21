# Network Multiplayer Setup (same Wi‑Fi only)

Both PCs must use the **same router** (same Wi‑Fi network or Ethernet into that router). The host runs `TankGameServer`; the other machine connects to the host’s **LAN** IP.

---

## Host (PC that runs the server)

1. **Firewall (once)** — Allow **inbound TCP 5190** on this PC.  
   - Easiest: when you first run the server, if Windows asks to allow access, click **Allow**.  
   - Or: **Windows Defender Firewall** → **Advanced settings** → **Inbound Rules** → **New Rule** → **Port** → **TCP** → **5190** → **Allow** → name it e.g. `Tank Game Server`.

2. **Start the server** (leave this terminal open):

   ```powershell
   cd "path\to\TankMultiplayer\TankGameServer"
   dotnet run
   ```

   The server listens on **5190** (`0.0.0.0:5190` in code), so other devices on the LAN can connect.

3. **Your LAN IPv4** — Run `ipconfig` on this PC and find **IPv4 Address** on the adapter you’re using (often `192.168.1.x` or `192.168.0.x`). **Give this address to your friend.**

4. **Unity on your machine** — **NetworkManager** → **Server Address** = `localhost` (or your LAN IP) → **Server Port** = `5190`.

---

## Friend (other PC on the same Wi‑Fi)

1. Same Unity project (clone or copy from the host).

2. **NetworkManager** → **Server Address** = the host’s **LAN** IP from `ipconfig` (**not** `localhost` on their PC) → **Server Port** = `5190`.

3. Host has **`dotnet run`** running; then **both** press **Play** in Unity.

---

## If something fails

- Confirm both PCs are on the **same** network (same router / SSID).
- Friend must use the **host’s** IP, not their own.
- **Server** must be running on the host before Play.
- Host **firewall** must allow **TCP 5190** (try **Private** network profile if Windows is strict).
- Unity console should show **Connected to SignalR server!** and **Player1** / **Player2**.
- **TankSpawner** in the scene, prefabs assigned.

### Server won’t start

- Run commands from the `TankGameServer` folder (where `TankGameServer.csproj` is).
- Try `dotnet restore` if needed.
- Something else may be using port **5190**.

---

## Quick reference

| | Host | Friend |
|---|------|--------|
| **Server Address** | `localhost` or own LAN IP | Host’s **LAN** IP (`ipconfig` on host) |
| **Server Port** | `5190` | `5190` |
| **Server process** | `dotnet run` in `TankGameServer` | *(none — connects to host)* |

If the host reconnects to Wi‑Fi, its LAN IP may change; the friend updates **Server Address** if connection breaks.
