# n8n Workflows & Automation

Complete guide to building automated HR workflows using n8n, including pre-built templates and integration patterns.

---

## n8n Overview

**n8n** is a low-code/no-code workflow automation platform with 500+ node integrations.

### Why n8n for HR Analytics?

✅ **500+ integrations** - Connect any HR system
✅ **No-code builder** - Non-technical users can build workflows
✅ **Self-hosted option** - Data stays on-premises
✅ **Open source** - Full transparency
✅ **Flexible** - JSON, JavaScript, custom code
✅ **Affordable** - Free self-hosted version

---

## n8n Architecture

```
┌────────────────────────────────────────────────────────────┐
│                 n8n Workflow Engine                        │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ ┌─────────────┐    ┌──────────────┐    ┌────────────────┐ │
│ │ Triggers    │    │ Nodes        │    │ Outputs        │ │
│ │ (Start)     │───▶│ (Actions)    │───▶│ (Send Results) │ │
│ ├─────────────┤    ├──────────────┤    ├────────────────┤ │
│ │ Webhook     │    │ HTTP Request │    │ Slack          │ │
│ │ Schedule    │    │ Transform    │    │ Email          │ │
│ │ Manual      │    │ SQL Query    │    │ HR System      │ │
│ │ File        │    │ Condition    │    │ Database       │ │
│ │ Webhook     │    │ Loop         │    │ Webhook        │ │
│ │             │    │ Code         │    │ File           │ │
│ └─────────────┘    └──────────────┘    └────────────────┘ │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

---

## Pre-Built Workflow Templates

### Template 1: Turnover Risk Alert & Notification

**Purpose:** Alert HR immediately when high turnover risk detected

**Trigger:** Webhook from HR Analytics Platform

**Workflow:**
```
1. TRIGGER
   └─ HR Analytics sends turnover_risk_high webhook

2. DATA ENRICHMENT
   └─ Fetch employee details from HRIS
   └─ Get manager contact info
   └─ Lookup employee feedback history

3. CONDITION CHECK
   └─ If risk_score > 0.8 AND engagement_declined
      └─ Send urgent alert (critical)
      └─ Else if risk_score > 0.7
         └─ Send warning (high)

4. NOTIFICATIONS (Parallel)
   ├─ POST to Slack #hr-alerts
   │  └─ Message: "🚨 John Smith (Engineering) HIGH turnover risk"
   │  └─ Include: Risk score, driver, recommendation
   │
   ├─ Email to HR Manager
   │  └─ Subject: "Action Required: Retention Risk - John Smith"
   │  └─ Include: Risk analysis, suggested actions
   │
   ├─ POST to Microsoft Teams
   │  └─ Message in retention-alerts channel
   │
   └─ Create Asana task
      └─ Title: "Retention Risk: John Smith"
      └─ Due: Next business day
      └─ Assign to: HR Manager

5. LOGGING
   └─ Store in database for audit trail
   └─ Track follow-up actions
```

**n8n JSON Configuration:**
```json
{
  "name": "Turnover Risk Alert",
  "nodes": [
    {
      "name": "HR Analytics Webhook",
      "type": "n8n-nodes-base.webhook",
      "position": [250, 300],
      "webhook": {
        "path": "turnover-risk-alert"
      }
    },
    {
      "name": "Get Employee",
      "type": "n8n-nodes-base.http",
      "position": [450, 300],
      "parameters": {
        "url": "https://api.hrsystem.com/employees/{{ $json.employee_id }}",
        "method": "GET"
      }
    },
    {
      "name": "Slack Notification",
      "type": "n8n-nodes-base.slack",
      "position": [650, 200],
      "parameters": {
        "channel": "#hr-alerts",
        "message": "🚨 High Turnover Risk\nEmployee: {{ $node[\"Get Employee\"].json.name }}\nRisk Score: {{ $json.risk_score }}\nPrimary Factor: {{ $json.primary_driver }}"
      }
    },
    {
      "name": "Email to HR",
      "type": "n8n-nodes-base.emailSend",
      "position": [650, 350],
      "parameters": {
        "toEmail": "hr@company.com",
        "subject": "Action Required: Retention Risk - {{ $node[\"Get Employee\"].json.name }}",
        "message": "Employee is at high risk of leaving...",
        "attachHtml": true
      }
    }
  ]
}
```

---

### Template 2: Compensation Equity Audit

**Purpose:** Regular audit for pay equity gaps and compliance

**Trigger:** Monthly schedule (1st day of month)

**Workflow:**
```
1. TRIGGER
   └─ Scheduled: First day of month, 9 AM

2. DATA EXTRACTION
   ├─ Query HR Analytics: Compensation equity risks
   ├─ Filter: risk_score > 0.7
   ├─ Get: employee name, dept, role, salary, gap %

3. ENRICHMENT
   ├─ Add market data from LinkedIn/Glassdoor
   ├─ Add performance ratings
   ├─ Add tenure & promotion history

4. GENERATE REPORT
   ├─ Create summary spreadsheet
   │  ├─ Column: Employee, Department, Role
   │  ├─ Column: Salary, Market Rate, Gap
   │  ├─ Column: Gap %, Risk Level
   │  └─ Conditional formatting: Red for >10% gap
   │
   └─ Create visualizations
      ├─ Department gap trends
      ├─ Gender pay gap over time
      └─ Remediation impact estimate

5. REVIEW & APPROVAL
   └─ Send to CFO & Chief People Officer
   └─ Wait for approval (manual review)

6. REMEDIATION PLANNING
   ├─ For approved adjustments:
   │  ├─ Create Workday change order
   │  ├─ Schedule effective date (next pay cycle)
   │  ├─ Add to budget review
   │  └─ Document business reason
   │
   └─ For deferred:
      └─ Create reminder for next review

7. NOTIFICATIONS
   ├─ Email summary to leadership
   ├─ Post summary to Teams
   ├─ Create Jira tickets for HR follow-up
   └─ Archive report in SharePoint

8. LOGGING & AUDIT TRAIL
   └─ Store all decisions for compliance proof
```

**Key Nodes:**
- Webhook (schedule trigger)
- HTTP (query HR Analytics API)
- Spreadsheet (generate Excel)
- Conditional (decision logic)
- Wait (approval step)
- Email/Slack (notifications)
- Database (audit log)

---

### Template 3: New Hire Success Onboarding

**Purpose:** Execute onboarding steps when new hire is created

**Trigger:** Webhook from HRIS when employee hired

**Workflow:**
```
1. TRIGGER
   └─ HRIS sends employee.hired webhook

2. DATA VALIDATION
   ├─ Check required fields (name, email, dept, manager)
   ├─ Validate start date is future
   └─ Verify manager exists

3. ONBOARDING SEQUENCE
   ├─ Day 0 (Offer Accepted)
   │  ├─ Create email welcome message
   │  ├─ Send equipment order to IT
   │  ├─ Create accounts (email, laptop)
   │  └─ Add to company directory
   │
   ├─ Day -2 (Before start)
   │  ├─ Prepare workspace
   │  ├─ Send manager onboarding guide
   │  ├─ Notify team of new hire
   │  └─ Create team social event
   │
   ├─ Day 1 (First day)
   │  ├─ Send welcome email (1st day surprises)
   │  ├─ Add to orientation meeting
   │  ├─ Create Teams/Slack channel
   │  └─ Assign buddy/mentor
   │
   ├─ Week 1
   │  ├─ Send compliance training links
   │  ├─ Confirm benefits enrollment
   │  ├─ Schedule 1-on-1 with manager
   │  └─ Check-in: "How's your first week?"
   │
   ├─ Week 4
   │  ├─ Check-in from HR
   │  ├─ Confirm training completion
   │  ├─ Schedule 30-day review
   │  └─ Gather feedback
   │
   └─ Day 90
      ├─ Check AI prediction: Hire success score
      ├─ If score < 60: Trigger intervention
      │  ├─ Manager deep dive meeting
      │  ├─ Development plan creation
      │  ├─ Additional training/mentoring
      │  └─ Weekly check-ins started
      │
      └─ If score >= 60: Mark as successful
         └─ Archive onboarding workflow
```

**Expected Outcomes:**
- New hire success rate: 85%+ (vs 70% industry average)
- Time to productivity: Reduced by 40%
- Attrition in year 1: Reduced by 30%

---

### Template 4: Manager Performance Insights

**Purpose:** Weekly insights for managers on team performance & risks

**Trigger:** Weekly schedule (Monday 8 AM)

**Workflow:**
```
1. TRIGGER
   └─ Scheduled: Every Monday, 8 AM

2. DATA FETCH
   ├─ Get all managers
   ├─ For each manager:
   │  ├─ Get team members
   │  ├─ Fetch turnover risks for team
   │  ├─ Get engagement scores
   │  ├─ Get performance trends
   │  └─ Get promotion readiness
   │
   └─ Summarize by manager

3. INSIGHTS GENERATION
   ├─ Team health score (0-100)
   ├─ At-risk count (high turnover risk)
   ├─ Engagement trend (up/down)
   ├─ Top performer highlights
   ├─ Development opportunities
   └─ Recommended actions (personalized)

4. FORMAT MESSAGE
   ├─ Create Slack message
   │  └─ Thread format with emoji for clarity
   │  └─ Quick scan dashboard link
   │
   └─ Create email version
      └─ More detailed with explanations

5. DELIVER
   ├─ POST to Slack (manager-specific message)
   ├─ Send email summary
   ├─ Create Teams adaptive card with drill-down
   └─ Log delivery & engagement

6. FOLLOW-UP
   ├─ If manager opens link: Track
   ├─ If manager took action: Reward
   │  └─ Recognize high-engagement managers
   │
   └─ If manager disengaged: Escalate
      └─ Notify HR of non-responsive managers
```

**Personalization Example:**
```
Manager: Jane Smith
Team: 8 people, Engineering

📊 Your Team Health Score: 82/100

🚨 At Risk (1):
- John (Risk: 78%) - Primary: Comp gap -15%
  → Recommended: Salary review + 1-on-1 this week

📈 Engagement Trend: ↑ +1.2 pts
- Great job on regular check-ins!

⭐ Star Performers:
- Sarah - Ready for promotion in 6 months
- Mike - Excelling in new role

💡 Top Action This Week:
- Schedule 1-on-1 with John (retention risk)
- Discuss Sarah's promotion timeline
```

---

### Template 5: Hiring Pipeline Optimization

**Purpose:** Auto-update hiring managers on candidate status & AI insights

**Trigger:** Webhook from ATS (Greenhouse, Lever) when candidate moves

**Workflow:**
```
1. TRIGGER
   └─ ATS sends candidate.stage_changed webhook

2. GET AI INSIGHTS
   ├─ Call HR Analytics API
   │  ├─ Get hire success prediction
   │  ├─ Get culture fit score
   │  ├─ Get flag risks (comp expectations, etc)
   │  └─ Get recommended interview focus areas
   │
   └─ Fetch market benchmark for role

3. CONDITION CHECK
   ├─ If moving to "Offer Stage"
   │  ├─ Validate comp vs benchmark
   │  ├─ Check hire success score
   │  ├─ If score < 60:
   │  │  └─ Alert hiring manager with concerns
   │  └─ If score >= 60:
   │     └─ Proceed normally
   │
   ├─ If moving to "Interviewing"
   │  ├─ Send interview guides to panel
   │  └─ Suggest interview focus areas
   │
   └─ If rejected
      ├─ Classify rejection reason
      └─ Add to funnel analytics

4. HIRING MANAGER UPDATE
   ├─ Post to Slack #hiring channel
   │  └─ "Sarah moved to Offer stage (Hire Success: 85%)"
   │
   ├─ Send email with AI insights
   │  └─ "Recommended offer: $105k (market: $100-110k)"
   │
   └─ Add to Asana task
      └─ "Prepare offer for Sarah"

5. CANDIDATE COMMUNICATION
   ├─ If advancing to next stage:
   │  └─ Send encouraging email
   │
   └─ If candidate could be at-risk:
      └─ Schedule recruiter call

6. PIPELINE ANALYTICS
   ├─ Update pipeline forecast
   ├─ Recalculate hiring timeline
   ├─ Update executive dashboard
   └─ Notify if timeline at risk
```

---

## n8n Installation & Setup

### Option 1: n8n Cloud (Easiest)

```bash
1. Sign up at https://n8n.cloud
2. Create free workspace (10k executions/month)
3. Start building workflows
4. Connect integrations via UI
```

### Option 2: Self-Hosted (Docker)

```bash
# Docker Compose setup
docker run -d \
  -p 5678:5678 \
  --name n8n \
  -e DB_TYPE=postgres \
  -e DB_POSTGRESDB_HOST=postgres \
  n8nio/n8n

# Or use Docker Compose
docker-compose up -d
```

### Option 3: Self-Hosted (Kubernetes)

```bash
# Deploy to Kubernetes cluster
kubectl apply -f n8n-deployment.yaml
kubectl apply -f n8n-service.yaml
kubectl port-forward svc/n8n 5678:80
```

---

## Common n8n Patterns

### Pattern 1: Conditional Branching

```
IF turnover_risk > 0.7
  THEN send_alert()
ELSE IF turnover_risk > 0.5
  THEN send_warning()
ELSE
  THEN log_low_risk()
```

### Pattern 2: Loop Over Items

```
FOR EACH employee IN high_risk_employees
  - Get employee details
  - Fetch manager info
  - Send notification
  - Log action
END
```

### Pattern 3: Wait & Retry

```
SEND request
IF failed
  WAIT 5 seconds
  RETRY (up to 3 times)
ELSE
  LOG success
```

### Pattern 4: Error Handling

```
TRY
  FETCH from HR system
CATCH error
  IF timeout
    RETRY with backoff
  ELSE IF auth error
    ALERT ops team
  ELSE
    LOG error and continue
```

---

## n8n Integrations for HR Analytics

### Available Nodes (500+)

**HR Systems:**
- Workday (via HTTP API)
- ADP (via API)
- SAP SuccessFactors
- BambooHR
- Rippling
- Odoo
- Next ERP

**Recruiting:**
- Greenhouse
- Lever
- LinkedIn
- Workable

**Notifications:**
- Slack
- Microsoft Teams
- Email
- Twilio (SMS)
- Discord

**Data:**
- HTTP (any REST API)
- SQL (PostgreSQL, MySQL)
- Spreadsheet (Google Sheets, Excel)
- Database (MongoDB, DynamoDB)

**Automation:**
- Zapier (alternative)
- Stripe (payroll integrations)
- Asana/Jira (task management)
- Google Calendar

**File Storage:**
- Google Drive
- OneDrive/SharePoint
- Dropbox
- AWS S3

---

## Monitoring & Debugging

### Workflow Execution Logs

```
Each workflow stores:
- Execution timestamp
- Duration
- Success/failure status
- Detailed logs per node
- Error messages
- Data passed between nodes
```

### Debug Mode

```
Enable in workflow settings:
- Log all node executions
- Show data at each step
- Pause on errors
- Step-by-step debugging
```

### Error Alerts

```
Configure notifications for:
- Workflow failures
- Repeated failures
- Performance degradation
- Quota limits approaching
```

---

## Best Practices

✅ **Use Version Control**
- Save workflow versions
- Document changes
- Enable rollback

✅ **Error Handling**
- Handle failures gracefully
- Retry with backoff
- Notify on critical failures

✅ **Performance**
- Batch operations where possible
- Avoid unnecessary API calls
- Use caching where available

✅ **Security**
- Store credentials securely
- Use OAuth when available
- Rotate API keys regularly
- Enable audit logging

✅ **Testing**
- Test workflows before deploying
- Use test data
- Monitor first 24 hours
- Gradual rollout

---

## ROI: n8n Automation

**Time Saved (per month):**
- Turnover alert automation: 8 hours
- Compensation audit: 20 hours
- New hire onboarding: 12 hours
- Manager insights: 5 hours
- Hiring pipeline updates: 10 hours
- **Total: 55 hours/month = 660 hours/year**

**Cost Savings:**
- HR FTE: $80k/year average
- 660 hours saved: $32k value/year
- n8n cost: $0 (self-hosted) to $5k/year (cloud)
- **Net ROI: $27-32k/year**

**Quality Improvements:**
- Alerts faster (minutes vs days)
- Fewer manual errors (0% vs 2-5%)
- More consistent processes
- Better audit trails

---

## Conclusion

**n8n enables:**
1. ✅ No-code workflow automation
2. ✅ 500+ pre-built integrations
3. ✅ Real-time alerts & notifications
4. ✅ Process automation & optimization
5. ✅ Cost savings (55+ hours/month)
6. ✅ Improved employee experience

**Next Steps:**
1. Identify top 3 workflows to automate
2. Build in n8n sandbox
3. Test with small group
4. Deploy to production
5. Monitor & optimize
6. Scale to more workflows
