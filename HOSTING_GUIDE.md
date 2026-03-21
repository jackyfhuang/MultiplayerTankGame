# Quick Hosting Guide - Let Others Connect to Your Game

## Your Current IP Address: **192.168.1.76**

---

## Step 1: Start the Server

1. Open a terminal in the `TankGameServer` folder
2. Run: `dotnet run`
3. You should see: "Now listening on: http://0.0.0.0:5190"

**Keep this terminal open!** The server must be running for others to connect.

---

## Step 2: Configure Windows Firewall (One-Time Setup)

Windows Firewall might block incoming connections. Here's how to allow it:

### Quick Method:
1. When you run `dotnet run`, Windows may pop up asking to allow access
2. Click **"Allow access"** or **"Unblock"**

### Manual Method (if no popup):
1. Press `Windows Key` → Type "Firewall" → Open **Windows Defender Firewall**
2. Click **"Advanced settings"** (left side)
3. Click **"Inbound Rules"** → Click **"New Rule..."** (right side)
4. Select **"Port"** → Next
5. Select **"TCP"** → Enter port **5190** → Next
6. Select **"Allow the connection"** → Next
7. Check all three boxes (Domain, Private, Public) → Next
8. Name it: **"Tank Game Server"** → Finish

---

## Step 3: Configure Your Unity (Host)

1. In Unity, find the **NetworkManager** GameObject in your scene
2. In the Inspector, set:
   - **Server Address**: `localhost` (or `192.168.1.76` - both work)
   - **Server Port**: `5190`

---

## Step 4: Give Your Friend These Instructions

### For Your Friend (Connecting from Another Computer):

1. **Get the Project:**
   - They need the Unity project (clone from Git or copy the folder)

2. **Find Your IP Address:**
   - Tell them your IP: **192.168.1.76**
   - (If your IP changes, run `ipconfig` and look for "IPv4 Address")

3. **Configure Their Unity:**
   - In Unity, find the **NetworkManager** GameObject
   - In the Inspector, set:
     - **Server Address**: `192.168.1.76` (YOUR IP - change this!)
     - **Server Port**: `5190`

4. **Make Sure You're on the Same Network:**
   - Both computers must be on the **same WiFi/router**
   - If on different networks, you'll need port forwarding or a VPN

5. **Start Playing:**
   - **You**: Start the server (`dotnet run` in TankGameServer folder)
   - **You**: Press Play in Unity
   - **Your Friend**: Press Play in Unity
   - Both should connect and see each other's tanks!

---

## Troubleshooting

### "Connection failed" Error
- ✅ **Check firewall**: Port 5190 must be allowed (see Step 2)
- ✅ **Check IP address**: Your IP might have changed - run `ipconfig` again
- ✅ **Check server is running**: Server terminal should show "Now listening on..."
- ✅ **Check same network**: Both must be on the same WiFi/router

### Can't See Each Other's Tanks
- ✅ Check Unity Console for "Connected to SignalR server!" message
- ✅ Check for "Assigned as Player1" or "Assigned as Player2" messages
- ✅ Make sure `TankSpawner` is in the scene with prefabs assigned

### Server Won't Start
- ✅ Make sure you're in the `TankGameServer` folder
- ✅ Run `dotnet restore` first if needed
- ✅ Check that port 5190 isn't already in use (close other instances)

---

## Quick Checklist

**Host (You):**
- [ ] Server is running (`dotnet run` in TankGameServer folder)
- [ ] Firewall allows port 5190
- [ ] NetworkManager in Unity has Server Address = `localhost` (or your IP)
- [ ] NetworkManager in Unity has Server Port = `5190`

**Friend:**
- [ ] Has the Unity project
- [ ] NetworkManager in Unity has Server Address = `192.168.1.76` (YOUR IP)
- [ ] NetworkManager in Unity has Server Port = `5190`
- [ ] On the same network as you

---

## Finding Your IP Address Again

If your IP changes (e.g., after reconnecting to WiFi):

**Windows:**
```powershell
ipconfig | Select-String "IPv4"
```

Look for the IPv4 address under your active network adapter (usually Wi-Fi or Ethernet).
