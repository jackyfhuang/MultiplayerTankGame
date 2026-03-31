# Hosting Guide (DigitalOcean Droplet)

This guide helps you host `TankGameServer` on a DigitalOcean droplet so players on different networks can connect.

## 0) What your Unity client expects

Your client connects to:

- `http://<server-address>:5190/tankgame`
- WebSocket transport is used directly

So the droplet must allow **TCP 5190** and keep the server process running.

---

## 1) Create the droplet

1. In DigitalOcean, create an **Ubuntu 22.04 or 24.04** droplet.
2. Choose at least **Basic / shared CPU / 1 GB RAM**.
3. Add your SSH key during creation (recommended).
4. After creation, note the droplet **Public IPv4**.

---

## 2) Open firewall on DigitalOcean + Ubuntu

### A. DigitalOcean Cloud Firewall

Attach a firewall to the droplet and allow inbound:

- `22` (SSH) from your admin IP
- `5190` (game server) from `0.0.0.0/0`

### B. Ubuntu UFW (inside VM)

```bash
sudo ufw allow OpenSSH
sudo ufw allow 5190/tcp
sudo ufw enable
sudo ufw status
```

---

## 3) Install .NET runtime/sdk on droplet

Use Microsoft instructions for your Ubuntu version, then verify:

```bash
dotnet --info
```

If you want the simplest setup, install the **same major SDK version** you use locally.

---

## 4) Deploy the server code

Copy your `TankGameServer` project to the droplet (including `.csproj` and all source files).

Example with SCP:

```bash
scp -r ./TankGameServer root@<DROPLET_IP>:/opt/
```

Then SSH into droplet:

```bash
ssh root@<DROPLET_IP>
cd /opt/TankGameServer
dotnet restore
dotnet run
```

Expected startup includes listening on:

- `http://0.0.0.0:5190`

Quick remote connectivity test from your local PC:

```bash
curl http://<DROPLET_IP>:5190/tankgame
```

(`404` or protocol-related response is normal; timeout/refused means firewall or process issue.)

---

## 5) Run server as a background service (systemd)

Create service file:

```bash
sudo nano /etc/systemd/system/tankgame.service
```

Paste:

```ini
[Unit]
Description=Tank Game SignalR Server
After=network.target

[Service]
WorkingDirectory=/opt/TankGameServer
ExecStart=/usr/bin/dotnet run --no-launch-profile
Restart=always
RestartSec=3
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Enable + start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable tankgame
sudo systemctl start tankgame
sudo systemctl status tankgame
```

Logs:

```bash
journalctl -u tankgame -f
```

---

## 6) Unity client setup for all players

In Unity (`NetworkManager` component):

- **Server Address** = droplet public IP (or DNS name)
- **Server Port** = `5190`

Do **not** use `localhost` on client machines.

---

## 7) Troubleshooting

### Connection failed

- Confirm service is running: `sudo systemctl status tankgame`
- Confirm app listening: `ss -tulpen | grep 5190`
- Confirm both firewalls allow `5190/tcp`
- Confirm client uses droplet public IP and port `5190`

### Works locally but not from internet

- Usually Cloud Firewall or UFW is blocking port `5190`
- Recheck DigitalOcean firewall attachment to correct droplet

### Unity WebGL note

- WebGL/browser builds usually require secure context (`https/wss`) and stricter CORS.
- For desktop Unity clients (Editor/Windows build), current `http` setup is fine.

---

## 8) Optional production hardening

- Create non-root Linux user for the service.
- Use domain + reverse proxy (Nginx/Caddy) with TLS for `https/wss`.
- Add basic monitoring/alerts and automatic restart checks.
