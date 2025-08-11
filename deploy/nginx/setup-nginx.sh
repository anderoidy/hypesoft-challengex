#!/bin/bash
# Nginx and Let's Encrypt Setup Script for Hypesoft Challenge X
# This script should be run as root or with sudo privileges

set -e

# Configuration
DOMAIN="api.yourdomain.com"
EMAIL="admin@yourdomain.com"
APP_PORT=5000
CERTBOT_EMAIL="certbot@${DOMAIN}"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Install Nginx
function install_nginx() {
    echo -e "${YELLOW}🔧 Installing Nginx...${NC}"
    
    # Update package list
    apt-get update
    
    # Install Nginx
    apt-get install -y nginx
    
    # Enable and start Nginx
    systemctl enable nginx
    systemctl start nginx
    
    echo -e "${GREEN}✅ Nginx installed successfully${NC}"
}

# Configure Nginx
function configure_nginx() {
    echo -e "${YELLOW}🔧 Configuring Nginx...${NC}"
    
    # Create Nginx configuration
    cat > /etc/nginx/sites-available/${DOMAIN} << EOL
# HTTP - Redirect to HTTPS
server {
    listen 80;
    listen [::]:80;
    server_name ${DOMAIN};
    return 301 https://\$host\$request_uri;
}

# HTTPS - Main server block
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name ${DOMAIN};

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/${DOMAIN}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${DOMAIN}/privkey.pem;
    ssl_trusted_certificate /etc/letsencrypt/live/${DOMAIN}/chain.pem;
    
    # SSL Protocols
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers on;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384';
    
    # SSL Session Settings
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:50m;
    ssl_session_tickets off;
    
    # HSTS (uncomment after testing)
    # add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    
    # OCSP Stapling
    ssl_stapling on;
    ssl_stapling_verify on;
    resolver 8.8.8.8 8.8.4.4 valid=300s;
    resolver_timeout 5s;
    
    # Security Headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    add_header Content-Security-Policy "default-src 'self' https: data: 'unsafe-inline' 'unsafe-eval';" always;
    
    # Logging
    access_log /var/log/nginx/${DOMAIN}-access.log;
    error_log /var/log/nginx/${DOMAIN}-error.log;
    
    # Proxy settings for the API
    location / {
        proxy_pass http://localhost:${APP_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        
        # Increase timeout for long-running requests
        proxy_connect_timeout 300s;
        proxy_send_timeout 300s;
        proxy_read_timeout 300s;
        send_timeout 300s;
    }
    
    # Health check endpoint
    location /health {
        access_log off;
        add_header Content-Type application/json;
        return 200 '{"status":"healthy","timestamp":"\"'\''"$(date -u +"%Y-%m-%dT%H:%M:%SZ")\"\'\''"}';
    }
}
EOL
    
    # Enable the site
    ln -sf /etc/nginx/sites-available/${DOMAIN} /etc/nginx/sites-enabled/
    rm -f /etc/nginx/sites-enabled/default
    
    # Test Nginx configuration
    nginx -t
    
    echo -e "${GREEN}✅ Nginx configured successfully${NC}"
}

# Install Certbot and obtain SSL certificate
function setup_ssl() {
    echo -e "${YELLOW}🔐 Setting up Let's Encrypt SSL...${NC}"
    
    # Install Certbot and Nginx plugin
    apt-get install -y certbot python3-certbot-nginx
    
    # Obtain and install the certificate
    certbot --nginx -d ${DOMAIN} --non-interactive --agree-tos -m ${EMAIL} --redirect
    
    # Set up automatic renewal
    (crontab -l 2>/dev/null; echo "0 12 * * * /usr/bin/certbot renew --quiet --deploy-hook 'systemctl reload nginx'") | crontab -
    
    echo -e "${GREEN}✅ SSL certificate obtained and configured${NC}"
}

# Set up log rotation
function setup_log_rotation() {
    echo -e "${YELLOW}📝 Setting up log rotation...${NC}"
    
    # Create log rotation configuration
    cat > /etc/logrotate.d/nginx << EOL
/var/log/nginx/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 0640 www-data adm
    sharedscripts
    postrotate
        if [ -f /var/run/nginx.pid ]; then
            kill -USR1 \`cat /var/run/nginx.pid\`
        fi
    endscript
}
EOL
    
    echo -e "${GREEN}✅ Log rotation configured${NC}"
}

# Main execution
function main() {
    # Check if running as root
    if [ "$(id -u)" -ne 0 ]; then
        echo -e "❌ This script must be run as root"
        exit 1
    fi
    
    # Check if domain is set
    if [ "${DOMAIN}" = "api.yourdomain.com" ]; then
        echo -e "❌ Please set the DOMAIN variable at the top of this script"
        exit 1
    fi
    
    # Install Nginx if not already installed
    if ! command -v nginx &> /dev/null; then
        install_nginx
    else
        echo -e "ℹ️  Nginx is already installed"
    fi
    
    # Configure Nginx
    configure_nginx
    
    # Set up SSL with Let's Encrypt
    setup_ssl
    
    # Set up log rotation
    setup_log_rotation
    
    # Restart Nginx to apply all changes
    systemctl restart nginx
    
    echo -e "\n🎉 Nginx setup completed successfully!"
    echo -e "\n🔍 Next steps:"
    echo -e "1. Update your DNS to point ${DOMAIN} to this server's IP address"
    echo -e "2. Test your SSL configuration at https://www.ssllabs.com/ssltest/analyze.html?d=${DOMAIN}"
    echo -e "3. Uncomment the HSTS header in the Nginx config after confirming everything works"
    echo -e "4. Monitor logs at /var/log/nginx/${DOMAIN}-{access,error}.log"
}

# Run the main function
main "$@"
