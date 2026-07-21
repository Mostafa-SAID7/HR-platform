# Database Architecture & Strategy

Comprehensive database documentation for HR Analytics Platform covering database selection, ORM strategies, schema design, performance optimization, and best practices.

---

## 📑 Contents

### Strategic Planning
- **[database-selection.md](./database-selection.md)** - Database comparison (PostgreSQL, MySQL, SQL Server, MongoDB)
- **[orm-comparison.md](./orm-comparison.md)** - ORM strategy (Entity Framework Core vs Dapper vs others)
- **[schema-design.md](./schema-design.md)** - Data modeling, schema architecture, relationships

### Implementation
- **[performance-optimization.md](./performance-optimization.md)** - Indexing, query optimization, caching strategies
- **[migration-strategy.md](./migration-strategy.md)** - Database migrations, versioning, flyway/Entity Framework migrations

### Operations & Security
- **[backup-recovery.md](./backup-recovery.md)** - Backup strategies, disaster recovery, business continuity
- **[security-compliance.md](./security-compliance.md)** - Data security, encryption, compliance (GDPR, HIPAA)
- **[monitoring-troubleshooting.md](./monitoring-troubleshooting.md)** - Performance monitoring, alerting, debugging

---

## 🎯 Quick Decision Summary

### Recommended Stack for HR Analytics Platform

**Database:** PostgreSQL (Primary) + Snowflake (Analytics)
- PostgreSQL: OLTP transactions, strong reliability, great ecosystem
- Snowflake: Analytics queries, scales horizontally, cost-effective

**ORM Strategy:** Hybrid Approach
- **Entity Framework Core:** Domain models, business logic, CRUD operations
- **Dapper:** Complex queries, reporting, analytics, performance-critical code
- **Raw SQL:** Stored procedures for bulk operations, complex analytics

**Rationale:**
- PostgreSQL is open-source, scalable, widely used in mid-market
- EF Core handles 80% of application code cleanly
- Dapper handles 15% of complex queries efficiently
- Raw SQL for 5% of specialized operations

---

## 🗄️ Database Architecture Overview

```
┌──────────────────────────────────────────────────────────┐
│             HR Analytics Platform Database               │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Operational Database (PostgreSQL)               │   │
│  │ ├─ Employee Master Data                         │   │
│  │ ├─ Transactions (Payroll, Hiring, etc)         │   │
│  │ ├─ Real-time Analytics Data                    │   │
│  │ └─ Application State                           │   │
│  └─────────────────────────────────────────────────┘   │
│                                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Analytics Database (Snowflake)                  │   │
│  │ ├─ Denormalized Data Warehouse                 │   │
│  │ ├─ Historical Data (Audit Trail)               │   │
│  │ ├─ ML Training Data                            │   │
│  │ └─ Business Intelligence Queries               │   │
│  └─────────────────────────────────────────────────┘   │
│                                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Caching Layer (Redis)                           │   │
│  │ ├─ User Sessions                               │   │
│  │ ├─ Frequently Accessed Data                    │   │
│  │ └─ Real-time Scores (Risk, Success, etc)      │   │
│  └─────────────────────────────────────────────────┘   │
│                                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Search Layer (Elasticsearch)                    │   │
│  │ ├─ Full-text Employee Search                   │   │
│  │ ├─ Audit Log Search                            │   │
│  │ └─ Analytics Data Discovery                    │   │
│  └─────────────────────────────────────────────────┘   │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 📊 Data Flow

```
HR Systems (50+)
├─ Workday, ADP, Greenhouse, etc
└─ API / Webhooks
    ↓
┌─────────────────────────────┐
│  Data Ingestion Layer       │
│  (Apache Kafka, Lambda)     │
└──────────┬──────────────────┘
           ↓
┌─────────────────────────────┐
│ PostgreSQL (OLTP)           │
│ • Employee Data             │
│ • Real-time Updates         │
│ • Application Transactions  │
└──────────┬──────────────────┘
           ↓
┌─────────────────────────────┐
│ Redis Cache                 │
│ • Hot Data                  │
│ • Session Cache             │
│ • Score Cache               │
└──────────┬──────────────────┘
           ↓
┌─────────────────────────────┐
│ Snowflake (Analytics)       │
│ • Historical Data           │
│ • Analytics Queries         │
│ • ML Training Data          │
└──────────┬──────────────────┘
           ↓
┌─────────────────────────────┐
│ Application Layer (ASP.NET) │
│ • EF Core ORM               │
│ • Dapper for Complex Queries│
│ • Business Logic            │
└──────────┬──────────────────┘
           ↓
┌─────────────────────────────┐
│ Elasticsearch               │
│ • Search Capability         │
│ • Log Aggregation           │
└─────────────────────────────┘
```

---

## 🔄 Multi-Database Strategy

### Why Not Single Database?

**PostgreSQL alone is great for:**
- ✅ Transactional consistency
- ✅ ACID compliance
- ✅ Real-time data
- ❌ But struggles with analytical queries on large datasets
- ❌ And expensive to scale for analytics

**Solution: Polyglot Persistence**
- PostgreSQL: Fast transactions, small queries
- Snowflake: Large analytics, historical data
- Redis: Ultra-fast cached lookups
- Elasticsearch: Full-text search

---

## 🛠️ ORM Strategy

### Entity Framework Core (Primary: 80% of code)

**Best for:**
- CRUD operations
- Business logic
- Type safety
- Relationships
- Migrations

**Example:**
```csharp
var employee = await context.Employees
    .Include(e => e.Department)
    .FirstOrDefaultAsync(e => e.Id == id);
employee.Salary = newSalary;
await context.SaveChangesAsync();
```

### Dapper (Complex Queries: 15% of code)

**Best for:**
- Complex queries with multiple joins
- Reporting queries
- Bulk operations
- Performance-critical code
- Analytics

**Example:**
```csharp
var turnoverRisks = connection.Query<TurnoverRiskDto>(
    @"SELECT e.Id, e.Name, tr.RiskScore, tr.PrimaryDriver
      FROM Employees e
      JOIN TurnoverRisks tr ON e.Id = tr.EmployeeId
      WHERE tr.RiskScore > @threshold
      ORDER BY tr.RiskScore DESC",
    new { threshold = 0.7 }
);
```

### Raw SQL (Specialized: 5% of code)

**Best for:**
- Stored procedures
- Bulk operations (millions of records)
- Triggers
- Complex calculations

---

## 📈 Scaling Strategy

### Vertical Scaling (Single Server)
- **Phase 1 (Year 1):** Single PostgreSQL instance with replication
- **Max:** 10-50 GB data, 10k concurrent queries

### Horizontal Scaling (Year 2+)
- **Read Replicas:** Scale reads (analytics queries)
- **Sharding:** Partition by company_id or tenant
- **Snowflake:** Off-load analytics completely

### Caching Strategy
- **L1 Cache:** Application memory (1 min TTL)
- **L2 Cache:** Redis (5-60 min TTL)
- **L3 Cache:** Database (persistent)

---

## 🔒 Security Layers

**Defense in Depth:**
1. Network: VPC, security groups, firewalls
2. Authentication: IAM roles, database users
3. Authorization: Row-level security, column-level encryption
4. Encryption: TLS in transit, AES-256 at rest
5. Audit: All queries logged, compliance checks

---

## 📋 Database Characteristics for HR Analytics

### Operational Database (PostgreSQL)
- **Type:** OLTP (Online Transaction Processing)
- **Purpose:** Real-time employee data, transactions
- **Size:** 50 GB (Year 1), 200 GB (Year 3)
- **Concurrent Users:** 1,000
- **Query Pattern:** Fast, small result sets
- **Data Consistency:** Strong (ACID)

### Analytics Database (Snowflake)
- **Type:** OLAP (Online Analytical Processing)
- **Purpose:** Historical analytics, ML training
- **Size:** 500 GB (Year 1), 5 TB (Year 3)
- **Concurrent Users:** 100 (analytical queries)
- **Query Pattern:** Slow, large result sets
- **Data Consistency:** Eventual (near real-time)

---

## 📊 Data Volume Estimates

### Year 1
- **Employees:** 1,000 companies × 500 employees avg = 500k employee records
- **Monthly Events:** 500k employees × 30 events/month = 15M events
- **Predictions:** 500k × 6 models = 3M prediction records
- **Total Storage:** ~50 GB

### Year 3
- **Employees:** 2,000 companies × 800 employees avg = 1.6M employee records
- **Monthly Events:** 1.6M × 30 = 48M events
- **Predictions:** 1.6M × 6 = 9.6M prediction records
- **Historical Data:** 3 years of data
- **Total Storage:** ~200 GB (PostgreSQL) + ~5 TB (Snowflake)

---

## 🚀 Technology Stack

**Database Engines:**
- PostgreSQL 14+ (OLTP)
- Snowflake (Analytics, Cloud Data Warehouse)
- Redis 7+ (Caching)
- Elasticsearch 8+ (Search)

**ORMs & Query Tools:**
- Entity Framework Core 7+ (.NET 7+)
- Dapper 2.0+
- Npgsql (PostgreSQL driver)

**Infrastructure:**
- Docker (containerization)
- Kubernetes (orchestration)
- AWS RDS (managed database)
- AWS ElastiCache (managed Redis)

**Monitoring & Operations:**
- CloudWatch (AWS monitoring)
- Datadog (APM, database monitoring)
- Prometheus/Grafana (metrics)
- ELK Stack (logging)

---

## 📋 Quick Reference

### When to Use PostgreSQL (OLTP)
✅ Real-time employee data
✅ User transactions
✅ Current state queries
✅ Strong consistency required
✅ Relational data

### When to Use Snowflake (Analytics)
✅ Historical analysis
✅ Large dataset joins
✅ Aggregations & grouping
✅ Machine learning training
✅ Cost-effective at scale

### When to Use Redis (Caching)
✅ Employee lookup (by ID)
✅ Risk scores (hot data)
✅ User sessions
✅ Real-time dashboards
✅ Leaderboards/rankings

### When to Use Entity Framework Core
✅ CRUD operations (Create, Read, Update, Delete)
✅ Business logic with entities
✅ Relationships (navigation properties)
✅ Migrations & schema versioning

### When to Use Dapper
✅ Complex multi-table queries
✅ Reporting & analytics
✅ Bulk operations
✅ Performance-critical code
✅ Stored procedures

---

## 🔍 Key Decisions Made

### Decision 1: PostgreSQL as Primary Database
**Rationale:**
- Open-source (cost-effective)
- Strong ACID compliance
- Great for mid-market scale
- Excellent JSON support (for flexible schemas)
- Large community, mature ecosystem
- AWS RDS support (managed service)

**Alternatives Considered:**
- ❌ MySQL: Less robust for complex queries
- ❌ SQL Server: Expensive licensing
- ❌ MongoDB: Not ideal for transactional consistency

### Decision 2: Hybrid ORM Strategy (EF Core + Dapper)
**Rationale:**
- EF Core: Clean code, maintainability, rapid development
- Dapper: Performance where needed, explicit control
- Best of both worlds: productivity + performance

**Alternatives Considered:**
- ❌ EF Core only: Too slow for complex queries
- ❌ Dapper only: Verbose, hard to maintain
- ❌ NHibernate: Overkill, less community support

### Decision 3: Snowflake for Analytics
**Rationale:**
- Cloud-native (no infrastructure)
- Auto-scaling (pay for what you use)
- SQL familiar (standard queries)
- Time-travel (audit trail)
- Excellent for ML data preparation

**Alternatives Considered:**
- ❌ PostgreSQL analytics: Would slow down OLTP
- ❌ BigQuery: Google ecosystem lock-in
- ❌ Redshift: More infrastructure overhead

---

## 🎯 Success Criteria

**Database Architecture Success Looks Like:**
- ✅ Sub-100ms response times (P99)
- ✅ 99.99% uptime
- ✅ Real-time data within 5 minutes
- ✅ Analytics queries < 30 seconds
- ✅ Cost efficient (< $10k/month at Year 1 scale)
- ✅ GDPR/CCPA compliant
- ✅ Easy for developers to work with

---

## 📚 Document Guide

| Document | Purpose | Audience |
|----------|---------|----------|
| database-selection.md | Choose right database | Architects, DevOps |
| orm-comparison.md | Choose right ORM | Developers, Tech Leads |
| schema-design.md | Build data model | Backend developers |
| performance-optimization.md | Tune queries | DevOps, DBAs |
| migration-strategy.md | Manage schema changes | DevOps, Developers |
| backup-recovery.md | Plan disaster recovery | DevOps, Security |
| security-compliance.md | Secure data | Security, Compliance |
| monitoring-troubleshooting.md | Monitor & debug | DevOps, DBAs |

---

**Last Updated:** July 2026
**Status:** Active Database Strategy
**Version:** 1.0
**Review Cycle:** Quarterly or on major schema changes
