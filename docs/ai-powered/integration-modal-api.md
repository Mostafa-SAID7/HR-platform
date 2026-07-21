# Integration Modal & API Strategy

Comprehensive guide to integration methods: Modal UI, REST APIs, webhooks, and architectural patterns.

---

## Integration Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│          HR Analytics Platform - Integration Hub            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────┐    ┌─────────────────┐               │
│  │ Modal UI        │    │ REST API        │               │
│  │ (Point & Click) │    │ (Developer)     │               │
│  └────────┬────────┘    └────────┬────────┘               │
│           │                      │                        │
│  ┌────────────────────────────────┴──────────────────┐    │
│  │    Core Integration Engine                        │    │
│  ├────────────────────────────────────────────────────┤    │
│  │ • Connector framework (50+ connectors)            │    │
│  │ • Data validation & transformation               │    │
│  │ • Real-time sync & batch processing              │    │
│  │ • Error handling & retry logic                   │    │
│  │ • Monitoring & alerting                          │    │
│  └────────────┬───────────────────────────┬─────────┘    │
│               │                           │              │
│  ┌────────────▼──────────┐   ┌────────────▼──────────┐   │
│  │ n8n Workflows        │   │ Webhook Events       │   │
│  │ (No-Code Automation) │   │ (Real-Time Triggers) │   │
│  └──────────────────────┘   └─────────────────────┘   │
│                                                        │
│  ┌────────────────────────────────────────────────┐   │
│  │ Data Infrastructure                           │   │
│  │ (Kafka, PostgreSQL, Snowflake)               │   │
│  └────────────────────────────────────────────────┘   │
│                                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 1. Modal Integration (Easiest)

**Use Case:** HR manager wants to connect Workday with zero technical knowledge

### What is Modal Integration?

Modal UI is an embedded user interface that guides customers through system connection without code.

### Modal Flow

```
Step 1: Click "Connect System"
├─ Select Workday from dropdown
└─ Describe data needed

Step 2: Authenticate
├─ Login to Workday (OAuth)
└─ Grant permissions

Step 3: Configure
├─ Select which data to sync
├─ Set sync frequency
└─ Map fields (automated)

Step 4: Activate
├─ Test connection
├─ Run initial sync
└─ Monitor status
```

### Technical Implementation

**Frontend (React Component):**
```typescript
import { IntegrationModal } from '@hr-analytics/integrations';

export function WorkdayIntegration() {
  const [connected, setConnected] = useState(false);

  return (
    <IntegrationModal
      systemType="workday"
      onConnect={(config) => {
        // Save configuration
        saveIntegrationConfig(config);
        setConnected(true);
      }}
      dataFields={['employees', 'jobs', 'compensation']}
      syncFrequency="hourly"
    />
  );
}
```

**Backend (Connector Manager):**
```python
class WorkdayConnector(BaseConnector):
    """Workday integration connector"""
    
    system_type = "workday"
    auth_method = "oauth2"
    
    def authenticate(self, credentials):
        """OAuth flow"""
        token = get_oauth_token(
            client_id=credentials['client_id'],
            client_secret=credentials['client_secret'],
            tenant=credentials['tenant']
        )
        return token
    
    def sync_data(self, sync_config):
        """Fetch and sync data"""
        data = self.fetch_from_api('/workers')
        return self.transform_and_load(data)
    
    def validate_config(self, config):
        """Validate before saving"""
        return self.test_connection()
```

### Modal Features

✅ **50+ Pre-built Connectors**
- Workday, ADP, SAP, Oracle, Odoo, Next ERP
- Greenhouse, Lever, Workable
- Culture Amp, Lattice, 15Five
- LinkedIn Learning, Cornerstone
- And 30+ more

✅ **Automated Features**
- OAuth authentication
- Field mapping (AI-assisted)
- Conflict resolution
- Testing & validation
- Error handling

✅ **User Experience**
- Guided setup (5-10 minutes)
- Clear status & progress
- Error messages in plain English
- Support chat integration
- Knowledge base links

### Modal Configuration

```json
{
  "integrations": [
    {
      "id": "workday-prod",
      "system": "workday",
      "status": "connected",
      "auth": {
        "method": "oauth2",
        "tenant": "company.okta.com",
        "scope": ["employee_data", "payroll", "jobs"]
      },
      "sync": {
        "frequency": "hourly",
        "batch_size": 1000,
        "conflict_resolution": "source_of_truth"
      },
      "data_mapping": {
        "employees": {
          "workday.worker_id": "hr_system.employee_id",
          "workday.job_title": "hr_system.job_title"
        }
      },
      "monitoring": {
        "last_sync": "2026-07-20T09:15:00Z",
        "next_sync": "2026-07-20T10:15:00Z",
        "status": "healthy",
        "records_synced": 5234
      }
    }
  ]
}
```

---

## 2. REST API (For Developers)

**Use Case:** Custom integrations, specific data needs, advanced use cases

### API Architecture

**Base URL:** `https://api.hr-analytics.io/v1`

**Authentication:**
```bash
Authorization: Bearer sk_live_xxxxxxxxxxxxx
X-API-Key: ak_live_xxxxxxxxxxxxx
```

### Key Endpoints

**Authentication:**
```
POST /auth/login
POST /auth/oauth/authorize
POST /auth/refresh-token
```

**Integrations:**
```
GET    /integrations                 # List all integrations
POST   /integrations                 # Create new integration
GET    /integrations/{id}            # Get integration details
PUT    /integrations/{id}            # Update integration
DELETE /integrations/{id}            # Delete integration
GET    /integrations/{id}/status     # Check sync status
```

**Data Sync:**
```
POST   /integrations/{id}/sync       # Trigger sync
GET    /integrations/{id}/logs       # Get sync logs
POST   /integrations/{id}/test       # Test connection
```

**AI Insights:**
```
GET    /employees/{id}/risks         # Turnover risk
GET    /employees/{id}/recommendations # AI recommendations
POST   /analytics/cohort-analysis    # Custom cohort analysis
GET    /models/{model_id}/predictions # Batch predictions
```

### REST API Examples

**Example 1: Create Integration**
```bash
curl -X POST https://api.hr-analytics.io/v1/integrations \
  -H "Authorization: Bearer sk_live_xxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "system": "workday",
    "credentials": {
      "tenant": "company.okta.com",
      "client_id": "xxx",
      "client_secret": "xxx"
    },
    "sync_config": {
      "frequency": "hourly",
      "data_types": ["employees", "jobs", "compensation"]
    }
  }'

# Response:
{
  "id": "integ_1234567890",
  "system": "workday",
  "status": "authenticating",
  "created_at": "2026-07-20T09:15:00Z"
}
```

**Example 2: Get Turnover Risk**
```bash
curl https://api.hr-analytics.io/v1/employees/EMP123456/risks \
  -H "Authorization: Bearer sk_live_xxxxx"

# Response:
{
  "employee_id": "EMP123456",
  "risk_score": 0.78,
  "risk_level": "HIGH",
  "prediction_confidence": 0.91,
  "primary_drivers": [
    {
      "factor": "Compensation gap",
      "impact": 0.35
    },
    {
      "factor": "Engagement decline",
      "impact": 0.28
    }
  ],
  "recommendations": [
    {
      "action": "compensation_review",
      "priority": "HIGH",
      "expected_impact": "45% retention improvement"
    }
  ]
}
```

**Example 3: Batch Predictions**
```bash
curl -X POST https://api.hr-analytics.io/v1/models/turnover/predict \
  -H "Authorization: Bearer sk_live_xxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "employees": ["EMP001", "EMP002", "EMP003"],
    "model": "turnover_risk",
    "include_drivers": true,
    "include_recommendations": true
  }'

# Response:
{
  "predictions": [
    {
      "employee_id": "EMP001",
      "risk_score": 0.78,
      "primary_drivers": [...]
    },
    ...
  ]
}
```

### SDK Support

**Python SDK:**
```python
from hr_analytics import Client

client = Client(api_key="sk_live_xxxxx")

# Create integration
integration = client.integrations.create(
    system="workday",
    credentials={
        "tenant": "company.okta.com",
        "client_id": "xxx",
        "client_secret": "xxx"
    }
)

# Get predictions
risks = client.employees.get_risks("EMP123456")
print(f"Turnover risk: {risks.risk_score}")

# Batch analysis
cohort_analysis = client.analytics.cohort_analysis(
    filter={"department": "Engineering"},
    metrics=["turnover_risk", "engagement", "compensation"]
)
```

**JavaScript SDK:**
```javascript
import { HRAnalytics } from 'hr-analytics-js';

const client = new HRAnalytics({ apiKey: 'sk_live_xxxxx' });

// Get employee risks
const risks = await client.employees.getRisks('EMP123456');
console.log(`Turnover risk: ${risks.riskScore}`);

// Get recommendations
const recommendations = await client.employees.getRecommendations('EMP123456');
recommendations.forEach(rec => {
  console.log(`${rec.action}: ${rec.expectedImpact}`);
});
```

### Rate Limiting

```
- Standard: 100 requests/minute
- Premium: 1000 requests/minute
- Enterprise: Custom limits

Rate limit headers:
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1626696000
```

---

## 3. Webhook Events (Real-Time)

**Use Case:** Trigger actions when events occur (alerts, notifications, automation)

### Webhook Architecture

```
┌──────────────────────┐
│  HR Analytics        │
│  Event Stream        │
└──────────┬───────────┘
           │
       Trigger Event
    (e.g., high turnover
           risk detected)
           │
           ├─► POST /webhook/turnover-alert
           │   (Customer System A)
           │
           ├─► POST /webhook/slack-notify
           │   (Slack)
           │
           └─► Trigger n8n Workflow
               (Automation)
```

### Supported Webhook Events

```
Employee Events:
├── employee.created
├── employee.updated
├── employee.deleted
├── employee.hired
└── employee.terminated

Risk Events:
├── risk.turnover_high
├── risk.turnover_critical
├── risk.compensation_gap
├── risk.engagement_decline
└── risk.performance_decline

Recommendation Events:
├── recommendation.retention_action
├── recommendation.hiring_optimization
├── recommendation.promotion_ready
└── recommendation.compensation_adjustment

System Events:
├── sync.started
├── sync.completed
├── sync.failed
├── integration.connected
├── integration.disconnected
└── integration.error
```

### Webhook Configuration

**Create Webhook Subscription:**
```bash
curl -X POST https://api.hr-analytics.io/v1/webhooks \
  -H "Authorization: Bearer sk_live_xxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://customer.example.com/webhook",
    "events": ["risk.turnover_high", "risk.compensation_gap"],
    "description": "Alert on high turnover risk",
    "active": true,
    "retry_policy": {
      "max_attempts": 5,
      "backoff_multiplier": 2
    }
  }'
```

### Webhook Payload Examples

**Turnover Risk Alert:**
```json
{
  "event": "risk.turnover_high",
  "timestamp": "2026-07-20T09:15:00Z",
  "data": {
    "employee_id": "EMP123456",
    "risk_score": 0.85,
    "risk_level": "CRITICAL",
    "primary_drivers": [
      {"factor": "Compensation gap", "impact": 0.35}
    ],
    "recommended_actions": [
      "compensation_review",
      "manager_1on1"
    ]
  }
}
```

**Sync Completed:**
```json
{
  "event": "sync.completed",
  "timestamp": "2026-07-20T10:00:00Z",
  "data": {
    "integration_id": "integ_1234567890",
    "system": "workday",
    "status": "success",
    "records_synced": {
      "employees": 5234,
      "jobs": 892,
      "compensation": 5234
    },
    "duration_seconds": 145
  }
}
```

### Webhook Verification (Security)

```python
import hmac
import hashlib
import json

def verify_webhook(request_body, signature_header, webhook_secret):
    """Verify webhook authenticity"""
    
    # Create expected signature
    expected_sig = hmac.new(
        webhook_secret.encode(),
        request_body.encode(),
        hashlib.sha256
    ).hexdigest()
    
    # Compare signatures
    return hmac.compare_digest(signature_header, expected_sig)

# Usage:
if verify_webhook(request_body, signature_header, webhook_secret):
    process_webhook(json.loads(request_body))
else:
    raise Exception("Webhook verification failed")
```

---

## 4. Batch Import/Export

**Use Case:** One-time data import, data warehouse sync, backups

### CSV Import

**Format:**
```csv
employee_id,email,department,manager,salary,hire_date,engagement_score
EMP001,john@company.com,Engineering,MGR001,95000,2020-01-15,8.2
EMP002,jane@company.com,Sales,MGR002,75000,2019-06-20,7.1
...
```

**API Upload:**
```bash
curl -X POST https://api.hr-analytics.io/v1/data/import \
  -H "Authorization: Bearer sk_live_xxxxx" \
  -F "file=@employees.csv" \
  -F "data_type=employees"
```

### JSON Export

**Request:**
```bash
curl https://api.hr-analytics.io/v1/data/export \
  -H "Authorization: Bearer sk_live_xxxxx" \
  -d '{
    "format": "json",
    "data_types": ["employees", "risks", "recommendations"],
    "filters": {"department": "Engineering"}
  }' > export.json
```

**Response Format:**
```json
{
  "export_id": "exp_1234567890",
  "created_at": "2026-07-20T09:15:00Z",
  "data": {
    "employees": [...],
    "risks": [...],
    "recommendations": [...]
  }
}
```

---

## 5. Direct Database Connection

**Use Case:** Enterprise customers, data scientists, complex analytics

### Supported Databases

```
├── Snowflake (recommended)
├── PostgreSQL
├── MySQL
├── MongoDB
└── BigQuery
```

### Connection Setup

**Snowflake Example:**
```sql
-- Create share between HR Analytics and Customer
CREATE SHARE hr_analytics_share;

-- Grant access to tables
GRANT USAGE ON DATABASE hr_analytics_db TO SHARE hr_analytics_share;
GRANT USAGE ON SCHEMA analytics TO SHARE hr_analytics_share;

-- Customer imports share
CREATE DATABASE hr_analytics_prod
  FROM SHARE account_name.hr_analytics_share;

-- Now customer can query directly
SELECT * FROM hr_analytics_prod.analytics.employees;
SELECT * FROM hr_analytics_prod.analytics.turnover_risks;
```

### Example Queries

**Query 1: High-Risk Employees**
```sql
SELECT 
  e.employee_id,
  e.name,
  e.department,
  r.risk_score,
  r.primary_driver,
  r.recommended_action
FROM hr_analytics_prod.analytics.employees e
JOIN hr_analytics_prod.analytics.turnover_risks r 
  ON e.employee_id = r.employee_id
WHERE r.risk_score > 0.7
ORDER BY r.risk_score DESC;
```

**Query 2: Compensation Equity Analysis**
```sql
SELECT 
  department,
  gender,
  AVG(salary) as avg_salary,
  COUNT(*) as count
FROM hr_analytics_prod.analytics.employees
GROUP BY department, gender
HAVING COUNT(*) > 5;
```

---

## Comparison: Which Integration Method?

| Aspect | Modal | API | n8n | Direct DB |
|--------|-------|-----|-----|-----------|
| **Ease** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| **Setup Time** | 15 min | 1-2 days | 2-4 hours | 2-4 weeks |
| **Cost** | Included | Included | Free (self-hosted) | Included |
| **Power** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Real-Time** | Yes | Yes | Yes | Yes |
| **Best For** | HR teams | Developers | Automation | Analytics |

---

## Security & Compliance

### API Security

✅ **OAuth 2.0** - Industry standard
✅ **API Keys** - Scoped permissions
✅ **HTTPS/TLS** - Encrypted transport
✅ **Rate Limiting** - DDoS protection
✅ **Webhook Verification** - HMAC signatures

### Data Privacy

✅ **Encryption at rest** - AES-256
✅ **Encryption in transit** - TLS 1.3
✅ **Field-level encryption** - PII protected
✅ **Access controls** - Role-based
✅ **Audit trails** - All actions logged

### Compliance

✅ **GDPR** - Data subject rights, retention
✅ **CCPA** - Privacy controls
✅ **HIPAA** - Healthcare compliance
✅ **SOC 2** - Security certification
✅ **ISO 27001** - Information security

---

## Conclusion

**Integration Options:**
1. ✅ **Modal** - Fastest for HR teams (15 min)
2. ✅ **API** - Most powerful for developers
3. ✅ **n8n** - Best for automation & no-code
4. ✅ **Direct DB** - Enterprise analytics

**Choose based on:**
- **Simplicity needed?** → Modal
- **Custom integration?** → REST API
- **Automation/no-code?** → n8n
- **Complex analytics?** → Direct DB Connection

**All options offer:**
- Real-time data sync
- High availability
- Security & compliance
- 24/7 support
