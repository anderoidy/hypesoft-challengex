# Hypesoft Challenge X - Deployment Guide

This guide provides instructions for deploying the Hypesoft Challenge X API to a production environment.

## Prerequisites

- .NET 7.0 SDK or later
- MongoDB 5.0 or later
- A domain name (e.g., api.yourdomain.com)
- SSL/TLS certificate for HTTPS
- Reverse proxy (Nginx, Apache, or IIS)
- Monitoring solution (Application Insights, New Relic, etc.)

## 1. Server Setup

### 1.1. Install Dependencies

```bash
# Update package lists
sudo apt update && sudo apt upgrade -y

# Install .NET 7.0 Runtime
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-7.0

# Install MongoDB
wget -qO - https://www.mongodb.org/static/pgp/server-5.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu focal/mongodb-org/5.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-5.0.list
sudo apt-get update
sudo apt-get install -y mongodb-org

# Start MongoDB
sudo systemctl start mongod
sudo systemctl enable mongod
```

## 2. Application Configuration

### 2.1. Environment Variables

Create a `.env` file in the application root directory with the following variables:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5000;https://localhost:5001

# MongoDB Configuration
MongoDB__ConnectionString=mongodb://localhost:27017
MongoDB__DatabaseName=HypesoftDB_Prod

# JWT Configuration
JwtSettings__Secret=your-256-bit-secret-key-here
JwtSettings__Issuer=Hypesoft.API
JwtSettings__Audience=Hypesoft.Clients
JwtSettings__ExpirationInMinutes=1440

# Application Insights
APPINSIGHTS_INSTRUMENTATIONKEY=your-application-insights-key
```

### 2.2. Secure Configuration

1. Generate a secure JWT secret:
   ```bash
   openssl rand -base64 32
   ```

2. Set file permissions:
   ```bash
   chmod 600 .env
   chown www-data:www-data .env
   ```

## 3. Nginx Configuration

Install Nginx:
```bash
sudo apt install -y nginx
```

Create a new Nginx configuration file at `/etc/nginx/sites-available/hypesoft-api`:

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/api.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.yourdomain.com/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable the site and test the configuration:
```bash
sudo ln -s /etc/nginx/sites-available/hypesoft-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

## 4. SSL Certificate

Install Certbot and obtain a Let's Encrypt certificate:

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.yourdomain.com
```

Set up automatic renewal:
```bash
sudo certbot renew --dry-run
```

## 5. Systemd Service

Create a systemd service file at `/etc/systemd/system/hypesoft-api.service`:

```ini
[Unit]
Description=Hypesoft Challenge X API
After=network.target

[Service]
WorkingDirectory=/var/www/hypesoft-api
ExecStart=/usr/bin/dotnet /var/www/hypesoft-api/Hypesoft.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=hypesoft-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
EnvironmentFile=/var/www/hypesoft-api/.env

[Install]
WantedBy=multi-user.target
```

Enable and start the service:
```bash
sudo systemctl enable hypesoft-api
sudo systemctl start hypesoft-api
```

## 6. Monitoring and Logging

### 6.1. Application Insights

1. Create an Application Insights resource in Azure Portal
2. Add the instrumentation key to your `.env` file
3. Monitor your application in the Azure Portal

### 6.2. Log Rotation

Create a log rotation configuration at `/etc/logrotate.d/hypesoft-api`:

```
/var/www/hypesoft-api/Logs/*.log {
    daily
    rotate 30
    compress
    delaycompress
    missingok
    notifempty
    create 644 www-data www-data
    sharedscripts
    postrotate
        systemctl kill -s HUP hypesoft-api
    endscript
}
```

## 7. Backup Strategy

### 7.1. MongoDB Backup

Create a backup script at `/usr/local/bin/backup-mongodb.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/var/backups/mongodb"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

mkdir -p $BACKUP_DIR
mongodump --out $BACKUP_DIR/mongodb-$TIMESTAMP

# Keep backups for 30 days
find $BACKUP_DIR -type d -name "mongodb-*" -mtime +30 -exec rm -rf {} \;
```

Make it executable and set up a cron job:
```bash
chmod +x /usr/local/bin/backup-mongodb.sh
(crontab -l 2>/dev/null; echo "0 2 * * * /usr/local/bin/backup-mongodb.sh") | crontab -
```

## 8. Security Hardening

1. Configure firewall:
   ```bash
   sudo ufw allow ssh
   sudo ufw allow http
   sudo ufw allow https
   sudo ufw enable
   ```

2. Keep the system updated:
   ```bash
   sudo apt update && sudo apt upgrade -y
   sudo apt autoremove -y
   ```

3. Configure SSH security:
   - Disable root login
   - Use SSH keys only
   - Change default SSH port

## 9. Deployment Automation

For automated deployments, you can use the GitHub Actions workflow (`.github/workflows/dotnet.yml`) that's already configured in the repository.

## 10. Maintenance

### 10.1. Application Updates

1. Pull the latest changes
2. Run database migrations
3. Restart the application service

### 10.2. Monitoring

- Set up alerts for:
  - High CPU/Memory usage
  - Disk space
  - Application errors
  - Failed health checks

## Support

For issues or questions, please contact the development team at dev@hypesoft.com.
