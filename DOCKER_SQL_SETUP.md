# Docker SQL Server Setup Guide

## 1. Check if Container is Running

```bash
docker ps -a | findstr sqlserver-container
```

Expected output should show the container as "Up" (running).

## 2. Check Container Logs

```bash
docker logs sqlserver-container
```

Look for: `SQL Server is now ready for client connections`

## 3. Verify Port is Exposed

```bash
netstat -an | findstr 1433
```

You should see `0.0.0.0:1433` or `127.0.0.1:1433` in LISTENING state.

## 4. Test Connection with sqlcmd

```bash
sqlcmd -S 127.0.0.1,1433 -U sa -P "YourStrong@Password1" -Q "SELECT @@VERSION"
```

## 5. Create Database if it Doesn't Exist

If the database `prechecknewbackup` doesn't exist, create it:

```bash
sqlcmd -S 127.0.0.1,1433 -U sa -P "YourStrong@Password1" -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'prechecknewbackup') CREATE DATABASE prechecknewbackup"
```

Or using Docker exec:

```bash
docker exec -it sqlserver-container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Password1" -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'prechecknewbackup') CREATE DATABASE prechecknewbackup"
```

## 6. Restart Container if Needed

If the container is not responding:

```bash
# Stop the container
docker stop sqlserver-container

# Remove the container
docker rm sqlserver-container

# Start fresh
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Password1" -p 1433:1433 --name sqlserver-container -d mcr.microsoft.com/mssql/server:2022-latest

# Wait 30-60 seconds, then check logs
docker logs sqlserver-container
```

## 7. Alternative: Use Docker Network

If localhost/127.0.0.1 doesn't work, try using Docker's host network:

```bash
# Stop current container
docker stop sqlserver-container
docker rm sqlserver-container

# Run with host network (Windows)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Password1" -p 1433:1433 --name sqlserver-container --network host -d mcr.microsoft.com/mssql/server:2022-latest
```

## 8. Connection String Options

### Option 1: Without Encryption (Fastest for local development)
```json
"DefaultConnection": "Server=127.0.0.1,1433;Database=prechecknewbackup;User Id=sa;Password=YourStrong@Password1;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
```

### Option 2: With Encryption (More secure)
```json
"DefaultConnection": "Server=127.0.0.1,1433;Database=prechecknewbackup;User Id=sa;Password=YourStrong@Password1;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
```

### Option 3: Using Data Source (Alternative format)
```json
"DefaultConnection": "Data Source=127.0.0.1,1433;Initial Catalog=prechecknewbackup;User ID=sa;Password=YourStrong@Password1;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
```

## Troubleshooting

### Issue: Connection timeout
- **Solution**: Wait 30-60 seconds after starting container, verify with `docker logs`
- **Solution**: Check if port 1433 is not blocked by firewall
- **Solution**: Try `Encrypt=False` instead of `Encrypt=True`

### Issue: Login failed
- **Solution**: Verify password matches the one set in Docker run command
- **Solution**: Check if SQL Server Authentication is enabled (should be by default in Docker)

### Issue: Database doesn't exist
- **Solution**: Create the database using sqlcmd (see step 5 above)
- **Solution**: Restore from backup if you have one

### Issue: Port already in use
- **Solution**: Check if another SQL Server instance is using port 1433
- **Solution**: Change the port mapping: `-p 1434:1433` and update connection string to use port 1434
