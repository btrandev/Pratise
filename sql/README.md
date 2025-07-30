# Database Migration with Flyway

This directory contains SQL migration scripts used by Flyway to manage the database schema for the Admin Service.

## Migration Files

- `V0__Initial_Schema.sql` - Creates the initial database tables and constraints
- `V1__InitialPermissionSeed.sql` - Seeds the database with system tenant, admin user, and permissions

## Running Migrations

### Using Flyway CLI

1. Install Flyway CLI: https://flywaydb.org/documentation/usage/commandline/

2. Navigate to the `sql` directory:
   ```
   cd sql
   ```

3. Run migrations:
   ```
   flyway -configFiles=flyway.conf migrate
   ```

### Using Docker

```bash
docker run --rm \
  -v $(pwd)/migrations:/flyway/sql \
  -v $(pwd)/flyway.conf:/flyway/conf/flyway.conf \
  flyway/flyway:latest \
  migrate
```

## Common Flyway Commands

- `flyway migrate` - Apply pending migrations
- `flyway clean` - Delete all objects in the schema (use with caution!)
- `flyway info` - Show the status of all migrations
- `flyway validate` - Validate applied migrations against the ones available
- `flyway repair` - Repair the schema history table

## Adding New Migrations

1. Create a new SQL file in the `migrations` directory
2. Name it following the Flyway naming convention: `V{VERSION}__{NAME}.sql`
   - Example: `V2__Add_Settings_Table.sql`
3. Run `flyway migrate` to apply the new migration

## Integration with docker-compose

Update your `docker-compose.yml` file to include a Flyway service:

```yaml
# Database Migrations (runs once and exits)
migrations:
  image: flyway/flyway:latest
  container_name: adminservice-migrations
  volumes:
    - ./sql/migrations:/flyway/sql
    - ./sql/flyway.conf:/flyway/conf/flyway.conf
  depends_on:
    - postgres
  networks:
    - admin-network
  command: migrate
```
