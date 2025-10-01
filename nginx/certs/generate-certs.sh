#!/bin/bash

# Generate self-signed SSL certificates for development
# This script creates certificates for localhost

CERTS_DIR="$(dirname "$0")"

cd "$CERTS_DIR"

# Generate private key
openssl genrsa -out localhost.key 2048

# Generate certificate signing request
openssl req -new -key localhost.key -out localhost.csr -subj "/C=BR/ST=SP/L=Sao Paulo/O=Hypesoft/CN=localhost"

# Generate self-signed certificate
openssl x509 -req -days 365 -in localhost.csr -signkey localhost.key -out localhost.crt

# Clean up CSR file
rm localhost.csr

echo "SSL certificates generated successfully:"
echo "- Private key: localhost.key"
echo "- Certificate: localhost.crt"
echo ""
echo "Certificates are valid for 365 days."
