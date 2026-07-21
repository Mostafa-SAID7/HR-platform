# Database Backup, Recovery & Disaster Planning

Comprehensive guide to backup strategies, disaster recovery procedures, and business continuity for HR Analytics Platform.

---

## Backup Strategy

### Recovery Requirements

| Metric | Target | Rationale |
|--------|--------|-----------|
| **RPO** (Recovery Point Objective) | < 1 hour | Max data loss acceptable |
| **RTO** (Recovery Time Objective) | < 4 hours | Max downtime acceptable |
| **Backup Frequency** | Every 6 hours | Meets < 1 hour RPO |
| **Retention Period** | 30 days (hot) + 7 years (archive) | GDPR + compliance |
| **Geographic Redundancy** | Multi-region | Disaster resilience |

---

## Backup Architecture

### Three-Tier Backup System

```
┌─────────────────────────────────────────────────────┐
│           Production Database (PostgreSQL)          │
└─────────────────────────────────────────────────────┘
           ↓              ↓              ↓
    ┌──────────┐   ┌──────────┐   ┌──────────┐
    │  Tier 1  │   │  Tier 2  │   │  Tier 3  │
    │  Local   │   │  AWS S3  │   │ Glacier  │
    │  Backup  │   │  (30day) │   │ (7year)  │
    └──────────┘   └──────────┘   └──────────┘
    • Every 6h    • Every 24h     • Monthly
    • Keep 7 days • Redundant     • Compliance
    • Fast access • Cost-effective│ Archive
```

### Tier 1: Local Backups (On-Premise/EC2)

```bash
#!/bin/bash
# Backup script: backup-local.sh
# Runs every 6 hours via cron

BACKUP_DIR="/backups/postgresql"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DB_NAME="hr_analytics"
DB_USER="backup_user"

# Create backup directory
mkdir -p $BACKUP_DIR

# Full backup using pg_dump
pg_dump -U $DB_USER -F c -b -v -f "$BACKUP_DIR/backup_$TIMESTAMP.dump" $DB_NAME

# Verify backup integrity
pg_restore -U $DB_USER --list "$BACKUP_DIR/backup_$TIMESTAMP.dump" > /dev/null
if [ $? -eq 0 ]; then
    echo "Backup successful: backup_$TIMESTAMP.dump"
    
    # Compress backup
    gzip "$BACKUP_DIR/backup_$TIMESTAMP.dump"
    
    # Keep only last 7 days (42 backups at 6h interval)
    find $BACKUP_DIR -name "backup_*.dump.gz" -mtime +7 -delete
else
    echo "Backup verification failed!" | mail -s "Backup Alert" ops@company.com
    exit 1
fi
```

**Cron Job:**
```bash
# Every 6 hours (0, 6, 12, 18)
0 0,6,12,18 * * * /scripts/backup-local.sh >> /logs/backup.log 2>&1
```

**Storage:**
- Location: EBS volume attached to backup server
- Size: ~500GB for 7 days of backups
- Cost: ~$50/month

### Tier 2: AWS S3 Backups (30-day retention)

```bash
#!/bin/bash
# Backup script: backup-s3.sh
# Runs daily at 2 AM

BACKUP_DIR="/backups/postgresql"
TIMESTAMP=$(date +"%Y%m%d")
DB_NAME="hr_analytics"
S3_BUCKET="hr-analytics-backups"
S3_PREFIX="daily/$TIMESTAMP"
AWS_REGION="us-east-1"

# Create full backup
pg_dump -U backup_user -F c -b -v \
  -f "/tmp/backup_$TIMESTAMP.dump" $DB_NAME

# Compress
gzip "/tmp/backup_$TIMESTAMP.dump"

# Upload to S3 with encryption
aws s3 cp "/tmp/backup_$TIMESTAMP.dump.gz" \
  "s3://$S3_BUCKET/$S3_PREFIX/" \
  --sse AES256 \
  --region $AWS_REGION \
  --storage-class STANDARD_IA  # Infrequent access for cost savings

# Verify upload
if [ $? -eq 0 ]; then
    echo "S3 backup uploaded: $TIMESTAMP"
    rm "/tmp/backup_$TIMESTAMP.dump.gz"
    
    # Set lifecycle policy: delete after 30 days
    # (configured separately in S3)
else
    echo "S3 upload failed!" | mail -s "Backup Alert" ops@company.com
    exit 1
fi
```

**S3 Lifecycle Policy:**
```json
{
  "Rules": [
    {
      "Id": "DeleteOldBackups",
      "Status": "Enabled",
      "Filter": {"Prefix": "daily/"},
      "Expiration": {"Days": 30},
      "NoncurrentVersionExpiration": {"NoncurrentDays": 30}
    }
  ]
}
```

**Costs:**
- Storage: ~$0.023/GB/month = ~$12/month for 500GB rotating
- Transfer: ~$0.02/GB = ~$50/month (estimate)
- Total: ~$70/month

### Tier 3: AWS Glacier Archive (7-year compliance)

```bash
#!/bin/bash
# Backup script: backup-glacier.sh
# Runs monthly on 1st at 3 AM

BACKUP_DIR="/backups/postgresql"
MONTH=$(date +"%Y%m")
DB_NAME="hr_analytics"
S3_BUCKET="hr-analytics-archive"
GLACIER_PREFIX="archive/$MONTH"

# Create comprehensive backup with metadata
pg_dump -U backup_user -F c -b -v \
  -f "/tmp/backup_archive_$MONTH.dump" $DB_NAME

# Compress with maximum compression
gzip -9 "/tmp/backup_archive_$MONTH.dump"

# Create metadata file
cat > "/tmp/backup_metadata_$MONTH.txt" << EOF
Date: $(date)
Database: $DB_NAME
Size: $(du -h /tmp/backup_archive_$MONTH.dump.gz | cut -f1)
Version: $(psql -U backup_user -t -c "SELECT version()")
Tables: $(psql -U backup_user -t -c "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'")
EOF

# Upload to S3 with Glacier transition
aws s3 cp "/tmp/backup_archive_$MONTH.dump.gz" \
  "s3://$S3_BUCKET/$GLACIER_PREFIX/" \
  --sse AES256 \
  --region us-east-1

aws s3 cp "/tmp/backup_metadata_$MONTH.txt" \
  "s3://$S3_BUCKET/$GLACIER_PREFIX/" \
  --sse AES256 \
  --region us-east-1

# Archive verification
aws s3api head-object --bucket $S3_BUCKET \
  --key "$GLACIER_PREFIX/backup_archive_$MONTH.dump.gz"

if [ $? -eq 0 ]; then
    echo "Glacier archive uploaded: $MONTH"
    rm "/tmp/backup_archive_$MONTH.dump.gz"
    rm "/tmp/backup_metadata_$MONTH.txt"
fi
```

**S3 Glacier Lifecycle:**
```json
{
  "Rules": [
    {
      "Id": "ArchiveToGlacier",
      "Status": "Enabled",
      "Filter": {"Prefix": "archive/"},
      "Transitions": [
        {
          "Days": 0,
          "StorageClass": "GLACIER"
        }
      ],
      "Expiration": {"Days": 2555}  // 7 years
    }
  ]
}
```

**Costs:**
- Storage: ~$0.004/GB/month = ~$2/month for 500GB/year × 7 years
- Retrieval: ~$0.05/GB (only on actual recovery)
- Total: ~$3/month + retrieval fees

---

## Recovery Procedures

### Scenario 1: Point-in-Time Recovery (Hour 0-6)

**Use Case:** Accidental data deletion, should recover to specific time

**Recovery Steps:**

1. **Stop application** (prevent writes during recovery)
   ```bash
   systemctl stop hr-api
   ```

2. **Restore from local backup** (fastest)
   ```bash
   # List available backups
   ls -lh /backups/postgresql/backup_*.dump.gz
   
   # Restore to new database
   pg_restore -U backup_user -d hr_analytics_recovery \
     /backups/postgresql/backup_20260720_120000.dump.gz
   
   # Verify data integrity
   psql -U backup_user -d hr_analytics_recovery \
     -c "SELECT COUNT(*) FROM employees;"
   ```

3. **Verify recovery** (test queries)
   ```bash
   # Test recovery database
   psql -U backup_user -d hr_analytics_recovery \
     -c "SELECT COUNT(*), MAX(updated_at) FROM employees;"
   ```

4. **Promote recovery database**
   ```bash
   # Backup current (corrupted) database
   pg_dump -U backup_user -F c \
     -f "/backups/postgresql/corrupted_$(date +%s).dump.gz" \
     hr_analytics
   
   # Drop corrupted database
   psql -U backup_user -c "DROP DATABASE hr_analytics;"
   
   # Rename recovery database
   psql -U backup_user \
     -c "ALTER DATABASE hr_analytics_recovery RENAME TO hr_analytics;"
   ```

5. **Restart application**
   ```bash
   systemctl start hr-api
   ```

**RTO:** < 30 minutes
**RPO:** < 6 hours (to last backup)

---

### Scenario 2: Corruption Recovery (Hour 6-24)

**Use Case:** Database corruption, need recovery from S3 backup

**Recovery Steps:**

1. **Restore from S3 backup** (if local copies aged out)
   ```bash
   # Download from S3
   aws s3 cp s3://hr-analytics-backups/daily/20260720/ /tmp/
   
   # Restore
   pg_restore -U backup_user -d hr_analytics_recovery \
     /tmp/backup_20260720.dump.gz
   ```

2. **Advanced recovery:** Using WAL (Write-Ahead Logs)
   ```bash
   # If you have WAL files, can recover beyond last backup
   # Requires PostgreSQL PITR (Point-in-Time Recovery) setup
   
   # Restore base backup
   pg_restore -U backup_user -d hr_analytics_pitr \
     /backups/postgresql/base_backup.dump.gz
   
   # Restore WAL logs up to specific point
   # (Handled by PostgreSQL recovery.conf or postgresql.conf)
   ```

**RTO:** 1-2 hours
**RPO:** < 24 hours

---

### Scenario 3: Disaster Recovery (Day 1+)

**Use Case:** Complete data center loss, recovery from Glacier archive

**Recovery Steps:**

1. **Request Glacier restore** (24-48 hour retrieval time)
   ```bash
   # Initiate restore from Glacier
   aws s3api restore-object \
     --bucket hr-analytics-archive \
     --key archive/202607/backup_archive_202607.dump.gz \
     --restore-request Days=7,GlacierJobParameters={Tier=Standard}
   ```

2. **While waiting, provision new infrastructure**
   ```bash
   # Launch new RDS PostgreSQL instance
   aws rds create-db-instance \
     --db-instance-identifier hr-analytics-dr \
     --db-instance-class db.t3.large \
     --engine postgres \
     --allocated-storage 500 \
     --backup-retention-period 30
   ```

3. **Download and restore** (once Glacier retrieval complete)
   ```bash
   # Download from S3
   aws s3 cp s3://hr-analytics-archive/archive/202607/ /tmp/ --recursive
   
   # Restore to new instance
   pg_restore -h new-rds-endpoint.amazonaws.com \
     -U admin -d hr_analytics \
     /tmp/backup_archive_202607.dump.gz
   ```

4. **Update application connection strings**
   ```csharp
   // Update connection in appsettings.json
   "ConnectionStrings": {
     "DefaultConnection": "Host=new-rds-endpoint.amazonaws.com;..."
   }
   ```

5. **Re-sync replicas** (if multi-region setup)
   ```bash
   # Promote read replicas to new primary
   aws rds promote-read-replica \
     --db-instance-identifier hr-analytics-replica-us-west-2
   ```

**RTO:** 24-72 hours (Glacier retrieval) + 2 hours (restore)
**RPO:** < 1 month

---

## Automated Recovery Testing

### Monthly Recovery Drill

```bash
#!/bin/bash
# recovery-test.sh - Run monthly to verify backups work

TIMESTAMP=$(date +"%Y%m%d")
TEST_DB="hr_analytics_test_$TIMESTAMP"

echo "=== Starting Recovery Test ==="
echo "Test Database: $TEST_DB"

# 1. Restore latest backup to test database
echo "Restoring backup..."
LATEST_BACKUP=$(ls -t /backups/postgresql/backup_*.dump.gz | head -1)
pg_restore -U backup_user -d $TEST_DB $LATEST_BACKUP

# 2. Run integrity checks
echo "Running integrity checks..."
psql -U backup_user -d $TEST_DB << EOF
-- Check table counts
SELECT 'employees' as table_name, COUNT(*) as row_count FROM employees
UNION ALL
SELECT 'departments', COUNT(*) FROM departments
UNION ALL
SELECT 'compensation_analysis', COUNT(*) FROM compensation_analysis
UNION ALL
SELECT 'turnover_risks', COUNT(*) FROM turnover_risks;

-- Check for orphaned foreign keys
SELECT e.id FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
WHERE e.department_id IS NOT NULL AND d.id IS NULL;

-- Check data freshness
SELECT MAX(updated_at) as latest_update FROM employees;
EOF

# 3. Performance check
echo "Running performance check..."
time psql -U backup_user -d $TEST_DB \
  -c "SELECT COUNT(*) FROM employees e 
      JOIN departments d ON e.department_id = d.id 
      WHERE e.company_id IN (1,2,3);"

# 4. Cleanup
echo "Cleaning up test database..."
psql -U backup_user -c "DROP DATABASE $TEST_DB;"

echo "=== Recovery Test Complete ==="
```

**Schedule:**
```bash
# First day of month at 2 AM
0 2 1 * * /scripts/recovery-test.sh >> /logs/recovery-test.log 2>&1
```

---

## Backup Monitoring & Alerts

### Monitoring Checklist

```
Daily Checks:
□ Backup completed successfully
□ Backup file size normal (not 0 bytes)
□ S3 upload successful
□ Backup verification passed
□ No corrupted backups

Weekly Checks:
□ Local backup storage < 80% full
□ S3 backup count correct (7-8 daily backups)
□ Backup speed acceptable (< 1 hour)
□ Recovery test passed

Monthly Checks:
□ Glacier archive created
□ Archive metadata correct
□ Restore test successful
□ Retention policies applied
```

### Alert Configuration

```yaml
# Prometheus alert rules
groups:
  - name: database_backups
    rules:
      # Backup failed
      - alert: BackupFailed
        expr: increase(backup_failures_total[1h]) > 0
        annotations:
          summary: "Database backup failed"
          
      # Backup taking too long
      - alert: BackupSlow
        expr: backup_duration_seconds > 3600
        annotations:
          summary: "Backup taking > 1 hour"
          
      # Backup storage full
      - alert: BackupStorageFull
        expr: backup_storage_usage_percent > 80
        annotations:
          summary: "Backup storage > 80% full"
          
      # S3 upload failed
      - alert: S3UploadFailed
        expr: increase(s3_upload_failures_total[1h]) > 0
        annotations:
          summary: "S3 backup upload failed"
```

---

## GDPR Data Retention & Deletion

### Data Retention Policy

```sql
-- Identify data older than retention period
SELECT 
    'terminated_employees' as data_type,
    COUNT(*) as record_count,
    MIN(termination_date) as oldest_record
FROM employees
WHERE status = 'Terminated' 
  AND termination_date < CURRENT_DATE - INTERVAL '7 years';

-- Safe deletion with audit trail
INSERT INTO audit_logs (action, details, created_date)
VALUES ('DATA_RETENTION_DELETE', 'Deleted 250 terminated employee records', NOW());

-- Delete terminated employees from 7+ years ago
DELETE FROM employees
WHERE status = 'Terminated' 
  AND termination_date < CURRENT_DATE - INTERVAL '7 years';
```

### Right to Be Forgotten (Article 17)

```csharp
// GDPR right to deletion
public async Task DeleteEmployeeDataAsync(int employeeId)
{
    using (var transaction = await context.Database.BeginTransactionAsync())
    {
        // Anonymize personal data
        var employee = await context.Employees.FindAsync(employeeId);
        employee.FirstName = "DELETED";
        employee.LastName = "DELETED";
        employee.Email = $"deleted-{employeeId}@deleted.local";
        employee.Phone = null;
        employee.Address = null;
        employee.DateOfBirth = null;
        employee.SocialSecurityNumber = null;
        
        // Remove related events
        var events = context.EmployeeEvents.Where(e => e.EmployeeId == employeeId);
        context.EmployeeEvents.RemoveRange(events);
        
        // Keep audit logs (legal requirement)
        // But mark as GDPR deleted
        foreach (var log in context.AuditLogs.Where(l => l.EmployeeId == employeeId))
        {
            log.IsGdprDeleted = true;
        }
        
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // Invalidate any cached data
        await cache.DeleteAsync($"employee_{employeeId}");
    }
}
```

---

## Backup Compliance Checklist

- [ ] Backups created on schedule (every 6 hours)
- [ ] Backup integrity verified (pg_restore test)
- [ ] S3 backups encrypted (AES-256)
- [ ] Glacier archives encrypted
- [ ] Access logs monitored (CloudTrail)
- [ ] IAM policies restrict backup access
- [ ] Backup deletion policy enforced (30 days)
- [ ] Recovery tests run monthly
- [ ] RTO < 4 hours documented
- [ ] RPO < 1 hour documented
- [ ] GDPR retention periods enforced
- [ ] Right to be forgotten process documented

---

## Recovery Time Estimates

| Scenario | Backup Age | Restore Time | RTO | Cost |
|----------|------------|--------------|-----|------|
| Local corruption | < 6h | 15-30 min | 1h | $0 |
| Full database loss | < 24h | 1-2 hours | 4h | $0 |
| Multi-region failover | Any | 2-4 hours | 4-6h | $500-1k |
| Disaster recovery | < 30 days | 24-48h* | 72h* | $100-500 |
| Archive recovery | < 7 years | 48h* | 72h* | $500+ |

*Glacier retrieval time included

---

**Last Updated:** July 2026
**Status:** Active Backup Strategy
**Next Review:** January 2027
**Tested:** Monthly (first of month)
