#!/bin/bash
# MongoDB Production Setup Script for Hypesoft Challenge X
# This script should be run as root or with sudo privileges

set -e

# Configuration
MONGO_VERSION="5.0"
ADMIN_USER="hypesoft_admin"
ADMIN_PASSWORD=$(openssl rand -base64 32)
APP_DB="HypesoftDB_Prod"
APP_USER="hypesoft_app"
APP_PASSWORD=$(openssl rand -base64 32)
BACKUP_DIR="/var/backups/mongodb"
LOG_DIR="/var/log/mongodb"
DATA_DIR="/var/lib/mongodb"

# Install MongoDB
function install_mongodb() {
    echo "🔧 Installing MongoDB ${MONGO_VERSION}..."
    
    # Import the public key
    wget -qO - https://www.mongodb.org/static/pgp/server-${MONGO_VERSION}.asc | sudo apt-key add -
    
    # Create list file
    echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu $(lsb_release -sc)/mongodb-org/${MONGO_VERSION} multiverse" | \
        sudo tee /etc/apt/sources.list.d/mongodb-org-${MONGO_VERSION}.list
    
    # Update and install
    apt-get update
    apt-get install -y mongodb-org
    
    # Create necessary directories
    mkdir -p ${BACKUP_DIR} ${LOG_DIR}
    chown -R mongodb:mongodb ${BACKUP_DIR} ${LOG_DIR}
    
    echo "✅ MongoDB installed successfully"
}

# Configure MongoDB
function configure_mongodb() {
    echo "🔧 Configuring MongoDB..."
    
    # Create MongoDB config
    cat > /etc/mongod.conf << EOL
# mongod.conf

# for documentation of all options, see:
#   http://docs.mongodb.org/manual/reference/configuration-options/

# Where and how to store data.
storage:
  dbPath: ${DATA_DIR}
  journal:
    enabled: true
  wiredTiger:
    engineConfig:
      cacheSizeGB: 1

# where to write logging data.
systemLog:
  destination: file
  logAppend: true
  path: ${LOG_DIR}/mongod.log
  logRotate: reopen
  timeStampFormat: iso8601-utc

# network interfaces
net:
  port: 27017
  bindIp: 127.0.0.1  # Only listen on localhost initially for security
  tls:
    mode: requireTLS
    certificateKeyFile: /etc/ssl/mongodb.pem
    CAFile: /etc/ssl/mongodb-ca.pem
    disabledProtocols: TLS1_0,TLS1_1
    allowConnectionsWithoutCertificates: false

# how the process runs
processManagement:
  timeZoneInfo: /usr/share/zoneinfo

# security settings
security:
  authorization: enabled
  keyFile: /var/lib/mongodb/mongodb-keyfile
  javascriptEnabled: false

# operationProfiling:
#   mode: slowOp
#   slowOpThresholdMs: 100

# replication:
#   replSetName: rs0
EOL

    # Create keyfile for replica set authentication
    openssl rand -base64 756 > /var/lib/mongodb/mongodb-keyfile
    chmod 400 /var/lib/mongodb/mongodb-keyfile
    chown mongodb:mongodb /var/lib/mongodb/mongodb-keyfile
    
    # Enable and restart MongoDB
    systemctl enable mongod
    systemctl restart mongod
    
    echo "✅ MongoDB configured successfully"
}

# Secure MongoDB
function secure_mongodb() {
    echo "🔒 Securing MongoDB..."
    
    # Wait for MongoDB to start
    sleep 10
    
    # Create admin user
    mongo --eval "
    db = db.getSiblingDB('admin');
    db.createUser({
        user: '${ADMIN_USER}',
        pwd: '${ADMIN_PASSWORD}',
        roles: [
            { role: 'userAdminAnyDatabase', db: 'admin' },
            { role: 'readWriteAnyDatabase', db: 'admin' },
            { role: 'dbAdminAnyDatabase', db: 'admin' },
            { role: 'clusterAdmin', db: 'admin' },
            { role: 'restore', db: 'admin' },
            { role: 'backup', db: 'admin' }
        ]
    });"
    
    # Create application database and user
    mongo -u "${ADMIN_USER}" -p "${ADMIN_PASSWORD}" --authenticationDatabase admin --eval "
    db = db.getSiblingDB('${APP_DB}');
    db.createUser({
        user: '${APP_USER}',
        pwd: '${APP_PASSWORD}',
        roles: [
            { role: 'readWrite', db: '${APP_DB}' },
            { role: 'dbAdmin', db: '${APP_DB}' }
        ]
    });"
    
    # Create a credentials file
    cat > /root/.mongorc.js << EOL
// MongoDB credentials for admin user
admin = db.getSiblingDB('admin');
admin.auth('${ADMIN_USER}', '${ADMIN_PASSWORD}');

// Auto-switch to application database
use ${APP_DB};
EOL
    
    # Save credentials to a secure file
    cat > /root/.mongodb_credentials << EOL
# MongoDB Admin Credentials
MONGODB_ADMIN_USER="${ADMIN_USER}"
MONGODB_ADMIN_PASSWORD="${ADMIN_PASSWORD}"

# Application Database Credentials
MONGODB_APP_DB="${APP_DB}"
MONGODB_APP_USER="${APP_USER}"
MONGODB_APP_PASSWORD="${APP_PASSWORD}"

# Connection Strings
MONGODB_CONNECTION_STRING="mongodb://${APP_USER}:${APP_PASSWORD}@localhost:27017/${APP_DB}?authSource=admin&tls=true"
MONGODB_ADMIN_CONNECTION_STRING="mongodb://${ADMIN_USER}:${ADMIN_PASSWORD}@localhost:27017/admin?tls=true"
EOL
    
    # Secure the credentials file
    chmod 600 /root/.mongodb_credentials
    
    echo "✅ MongoDB secured successfully"
    echo "📝 Credentials saved to /root/.mongodb_credentials"
    echo "🔑 Admin user: ${ADMIN_USER}"
    echo "🔑 Application user: ${APP_USER}"
    echo "📁 Application database: ${APP_DB}"
}

# Setup backup
function setup_backup() {
    echo "💾 Setting up MongoDB backup..."
    
    # Create backup script
    cat > /usr/local/bin/backup-mongodb.sh << 'EOL'
#!/bin/bash
# MongoDB Backup Script

# Configuration
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_DIR="/var/backups/mongodb"
LOG_FILE="/var/log/mongodb/backup-${TIMESTAMP}.log"
KEEP_DAYS=30

# Load credentials
source /root/.mongodb_credentials

# Create backup directory
mkdir -p ${BACKUP_DIR}/${TIMESTAMP}

# Dump all databases
mongodump --uri="${MONGODB_ADMIN_CONNECTION_STRING}" \
    --out=${BACKUP_DIR}/${TIMESTAMP} \
    --gzip \
    --oplog \
    --authenticationDatabase=admin \
    >> ${LOG_FILE} 2>&1

# Compress the backup
cd ${BACKUP_DIR}
tar -czf ${TIMESTAMP}.tar.gz ${TIMESTAMP}
rm -rf ${TIMESTAMP}

# Remove old backups
find ${BACKUP_DIR} -name "*.tar.gz" -type f -mtime +${KEEP_DAYS} -delete

# Log completion
echo "Backup completed at $(date)" >> ${LOG_FILE}
EOL
    
    # Make backup script executable
    chmod +x /usr/local/bin/backup-mongodb.sh
    
    # Add to crontab for daily backup at 2 AM
    (crontab -l 2>/dev/null; echo "0 2 * * * /usr/local/bin/backup-mongodb.sh") | crontab -
    
    echo "✅ MongoDB backup configured successfully"
}

# Main execution
function main() {
    # Check if running as root
    if [ "$(id -u)" -ne 0 ]; then
        echo "❌ This script must be run as root"
        exit 1
    fi
    
    # Check if MongoDB is already installed
    if command -v mongod &> /dev/null; then
        echo "⚠️  MongoDB is already installed. Skipping installation."
    else
        install_mongodb
    fi
    
    configure_mongodb
    secure_mongodb
    setup_backup
    
    echo "\n🎉 MongoDB setup completed successfully!"
    echo "🔐 IMPORTANT: Credentials have been saved to /root/.mongodb_credentials"
    echo "📋 Please copy these credentials to a secure location and then remove them from the server."
}

# Run the main function
main "$@"
