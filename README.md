# isz-lockbox-service

## Development

Debugging includes tasks to run the Docker Compose command against `docker-compose.yaml`.  This will start two containers prior to the service:

 - dynamodb
 - keycloak

This service is intended to provide middleware services to `isz-sercrets-mgmt-module`, a micro-frontend Pilet that provides capabilities to manage secrets within a Lockbox.

### Keycloak
This application allows for testing against OpenID Connect (OIDC), which is a protocol that sits on top of OATH2.  This container automatically imports a realm, but if a new realm is required, it can be exported: `/opt/keycloak/bin/kc.sh export --dir /opt/keycloak/data/import --realm isz-realm --users realm_file` from within the running container.  Saving this to the folder repository `./docker/keycloak` will allow for the next container run to pick up changes.

