# Database Schema Design & Data Modeling

Comprehensive schema design, entity relationships, and data modeling for HR Analytics Platform.

---

## Schema Overview

```sql
-- Core Tables
- companies (multi-tenant)
- departments
- employees
- managers (employee → manager relationship)
- employee_events (audit trail)

-- Transactional Tables
- payroll_records
- leave_requests
- performance_ratings
- goals

-- Analytics Tables
- turnover_risks
- hire_success_scores
- promotion_readiness
- compensation_analysis
- engagement_scores
- compliance_flags

-- System Tables
- users
- audit_logs
- integrations
- data_syncs
```

---

## Entity Relationship Diagram (ERD)

```
┌─────────────┐         ┌──────────────┐         ┌────────────────┐
│  Companies  │◄────────│ Departments  │────────►│   Employees    │
└─────────────┘         └──────────────┘         └────────────────┤
      │                                                    │       │
      │                                          ┌─────────┴───────┤
      │                                          │                 │
      └──────────────────────────────────────────┤  Employee       │
                                                 │  Analytics      │
                ┌─────────────────────────┐      │                 │
                │ Turnover Risks          │◄─────┤  ├─────────────┤
                │ Hire Success Scores     │  ┌───┤  │Turnover Risk│
                │ Promotion Readiness     │  │   │  │Hire Success │
                │ Compensation Analysis   │  │   │  │Promotion    │
                │ Engagement Scores       │  │   │  │Compensation│
                │ Compliance Flags        │  │   │  │Engagement  │
                └─────────────────────────┘  │   │  │Compliance  │
                                             └───┤  │            │
                ┌─────────────────────────┐      │  └─────────────┘
                │ Payroll Records         │
                │ Leave Requests          │
                │ Performance Ratings     │
                │ Goals                   │
                │ Employee Events (Audit) │
                └─────────────────────────┘
```

---

## Core Tables

### 1. Companies (Multi-Tenant)

```sql
CREATE TABLE companies (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    industry VARCHAR(100),
    size_range VARCHAR(50),  -- "250-500", "500-1000", etc
    country VARCHAR(50),
    timezone VARCHAR(50),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    
    -- Subscription
    subscription_tier VARCHAR(50),  -- "starter", "professional", "enterprise"
    subscription_start_date DATE,
    subscription_end_date DATE,
    
    CONSTRAINT unique_company_name UNIQUE(name)
);

CREATE INDEX idx_company_active ON companies(is_active);
CREATE INDEX idx_company_tier ON companies(subscription_tier);
```

### 2. Departments

```sql
CREATE TABLE departments (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    head_count INT DEFAULT 0,
    budget DECIMAL(12,2),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    CONSTRAINT unique_department_name UNIQUE(company_id, name)
);

CREATE INDEX idx_department_company ON departments(company_id);
CREATE INDEX idx_department_active ON departments(is_active);
```

### 3. Employees (Core Entity)

```sql
CREATE TABLE employees (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    external_id VARCHAR(255),  -- ID from Workday/ADP
    email VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    date_of_birth DATE,
    phone VARCHAR(20),
    
    -- Job Information
    job_title VARCHAR(255),
    job_level VARCHAR(50),  -- "IC1", "IC2", "Manager", etc
    department_id INT,
    manager_id INT,  -- Self-referential (employee → manager)
    cost_center VARCHAR(100),
    
    -- Compensation
    base_salary DECIMAL(12,2),
    bonus_target DECIMAL(12,2),
    total_compensation DECIMAL(12,2),
    compensation_currency VARCHAR(3) DEFAULT 'USD',
    
    -- Employment
    hire_date DATE NOT NULL,
    employment_type VARCHAR(50),  -- "Full-time", "Contractor", etc
    status VARCHAR(50),  -- "Active", "Leave", "Terminated"
    termination_date DATE,
    termination_reason VARCHAR(255),
    
    -- Demographics (GDPR-compliant)
    gender VARCHAR(50),
    ethnicity VARCHAR(100),
    country_of_origin VARCHAR(100),
    
    -- System
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (department_id) REFERENCES departments(id),
    FOREIGN KEY (manager_id) REFERENCES employees(id),
    CONSTRAINT unique_employee_email UNIQUE(company_id, email),
    CONSTRAINT unique_employee_external_id UNIQUE(company_id, external_id)
);

CREATE INDEX idx_employee_company ON employees(company_id);
CREATE INDEX idx_employee_department ON employees(department_id);
CREATE INDEX idx_employee_manager ON employees(manager_id);
CREATE INDEX idx_employee_status ON employees(status);
CREATE INDEX idx_employee_hire_date ON employees(hire_date);
CREATE INDEX idx_employee_active ON employees(is_active);
```

---

## Analytics Tables

### 4. Turnover Risks

```sql
CREATE TABLE turnover_risks (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    employee_id INT NOT NULL,
    
    -- Risk Scoring
    risk_score DECIMAL(3,2),  -- 0-1.0
    risk_level VARCHAR(50),  -- "LOW", "MEDIUM", "HIGH", "CRITICAL"
    prediction_confidence DECIMAL(3,2),  -- 0-1.0
    
    -- Primary Drivers
    primary_driver VARCHAR(255),  -- "compensation_gap", "engagement_decline", etc
    compensation_gap DECIMAL(5,2),  -- % difference from market
    engagement_score DECIMAL(3,1),  -- 1-10 scale
    engagement_trend DECIMAL(3,1),  -- change in engagement
    manager_stability_score DECIMAL(3,1),  -- 1-10
    
    -- Timeline
    prediction_date DATE,
    prediction_window_days INT DEFAULT 90,  -- 90 days out
    
    -- Action Tracking
    recommended_actions TEXT[],  -- Array of recommended actions
    actions_taken TEXT[],
    action_date DATE,
    retention_outcome VARCHAR(50),  -- "stayed", "left", "pending"
    
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    CONSTRAINT unique_turnover_risk UNIQUE(company_id, employee_id, prediction_date)
);

CREATE INDEX idx_turnover_company ON turnover_risks(company_id);
CREATE INDEX idx_turnover_employee ON turnover_risks(employee_id);
CREATE INDEX idx_turnover_risk_score ON turnover_risks(risk_score);
CREATE INDEX idx_turnover_prediction_date ON turnover_risks(prediction_date);
```

### 5. Hire Success Scores

```sql
CREATE TABLE hire_success_scores (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    candidate_id VARCHAR(255),  -- From ATS
    hired_employee_id INT,
    
    -- Prediction
    success_score DECIMAL(3,2),  -- 0-1.0
    success_level VARCHAR(50),  -- "LOW", "MEDIUM", "HIGH"
    estimated_retention_prob DECIMAL(3,2),
    high_performer_prob DECIMAL(3,2),
    
    -- Contributing Factors
    interview_score DECIMAL(3,2),
    background_fit_score DECIMAL(3,2),
    culture_fit_score DECIMAL(3,2),
    compensation_alignment_risk VARCHAR(50),  -- "low", "medium", "high"
    
    -- Outcome Tracking
    hired_date DATE,
    performance_rating DECIMAL(3,1),
    retention_months INT,
    is_active BOOLEAN,
    
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (hired_employee_id) REFERENCES employees(id)
);

CREATE INDEX idx_hire_success_company ON hire_success_scores(company_id);
CREATE INDEX idx_hire_success_score ON hire_success_scores(success_score);
```

### 6. Promotion Readiness

```sql
CREATE TABLE promotion_readiness (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    employee_id INT NOT NULL,
    
    -- Assessment
    readiness_score DECIMAL(3,2),  -- 0-1.0
    readiness_timeline VARCHAR(50),  -- "now", "6-months", "12-months"
    promotion_probability DECIMAL(3,2),
    success_probability DECIMAL(3,2),  -- If promoted, will succeed?
    
    -- Skill Assessment
    current_level INT,
    target_level INT,
    skill_gaps TEXT[],  -- Skills needed
    strength_areas TEXT[],
    
    -- Development Plan
    development_plan TEXT,
    recommended_training TEXT[],
    mentor_assigned VARCHAR(255),
    
    -- Status
    status VARCHAR(50),  -- "ready", "developing", "blocked"
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    CONSTRAINT unique_promotion_readiness UNIQUE(company_id, employee_id)
);

CREATE INDEX idx_promotion_readiness_score ON promotion_readiness(readiness_score);
CREATE INDEX idx_promotion_readiness_status ON promotion_readiness(status);
```

### 7. Compensation Analysis

```sql
CREATE TABLE compensation_analysis (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    employee_id INT NOT NULL,
    
    -- Salary Analysis
    actual_salary DECIMAL(12,2),
    market_salary DECIMAL(12,2),  -- Benchmark
    salary_gap_percent DECIMAL(5,2),
    salary_percentile DECIMAL(3,1),  -- 1-100, where they rank
    
    -- Equity Analysis (Gender, Race, Age)
    gender_pay_gap DECIMAL(5,2),
    race_pay_gap DECIMAL(5,2),
    age_pay_gap DECIMAL(5,2),
    
    -- Risk Assessment
    equity_risk_level VARCHAR(50),  -- "low", "medium", "high"
    compliance_status VARCHAR(50),  -- "compliant", "at_risk"
    
    -- Recommendations
    recommended_adjustment DECIMAL(12,2),
    adjustment_justification TEXT,
    
    analysis_date DATE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    CONSTRAINT unique_compensation_analysis UNIQUE(company_id, employee_id, analysis_date)
);

CREATE INDEX idx_compensation_company ON compensation_analysis(company_id);
CREATE INDEX idx_compensation_employee ON compensation_analysis(employee_id);
CREATE INDEX idx_compensation_risk ON compensation_analysis(equity_risk_level);
```

### 8. Engagement Scores

```sql
CREATE TABLE engagement_scores (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    employee_id INT NOT NULL,
    
    -- Engagement Metrics
    engagement_score DECIMAL(3,1),  -- 1-10 scale
    engagement_trend DECIMAL(3,1),  -- Month-over-month change
    nps_score INT,  -- -100 to +100
    eNPS_score INT,  -- Employee NPS
    
    -- Component Scores
    work_satisfaction DECIMAL(3,1),
    manager_relationship DECIMAL(3,1),
    peer_relationship DECIMAL(3,1),
    career_growth DECIMAL(3,1),
    work_life_balance DECIMAL(3,1),
    company_mission_alignment DECIMAL(3,1),
    
    -- Correlation
    turnover_risk_correlation DECIMAL(3,2),
    correlated_with_turnover BOOLEAN,
    
    survey_date DATE,
    survey_type VARCHAR(50),  -- "annual", "pulse", "exit"
    
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id)
);

CREATE INDEX idx_engagement_company ON engagement_scores(company_id);
CREATE INDEX idx_engagement_score ON engagement_scores(engagement_score);
CREATE INDEX idx_engagement_date ON engagement_scores(survey_date);
```

---

## Audit & System Tables

### 9. Employee Events (Audit Trail)

```sql
CREATE TABLE employee_events (
    id BIGSERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    employee_id INT NOT NULL,
    
    -- Event Details
    event_type VARCHAR(100),  -- "hire", "promotion", "termination", etc
    event_date DATE,
    event_details JSONB,  -- Flexible schema for event data
    
    -- Change Tracking
    changed_fields JSONB,  -- {"field": "salary", "old_value": 90000, "new_value": 95000}
    changed_by VARCHAR(255),  -- User ID
    source_system VARCHAR(100),  -- "workday", "atp", "manual", etc
    source_id VARCHAR(255),  -- ID in source system
    
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id)
);

CREATE INDEX idx_employee_events_employee ON employee_events(employee_id);
CREATE INDEX idx_employee_events_company ON employee_events(company_id);
CREATE INDEX idx_employee_events_type ON employee_events(event_type);
CREATE INDEX idx_employee_events_date ON employee_events(event_date);
```

### 10. Audit Logs

```sql
CREATE TABLE audit_logs (
    id BIGSERIAL PRIMARY KEY,
    company_id INT,
    user_id VARCHAR(255),
    action VARCHAR(100),  -- "read", "create", "update", "delete"
    entity_type VARCHAR(100),  -- "employee", "department", etc
    entity_id INT,
    
    -- Details
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    
    -- Compliance
    is_sensitive_data BOOLEAN,
    data_classification VARCHAR(50),  -- "public", "internal", "confidential"
    
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (company_id) REFERENCES companies(id)
);

CREATE INDEX idx_audit_company ON audit_logs(company_id);
CREATE INDEX idx_audit_user ON audit_logs(user_id);
CREATE INDEX idx_audit_date ON audit_logs(created_date);
CREATE INDEX idx_audit_entity ON audit_logs(entity_type, entity_id);
```

---

## Key Design Decisions

### 1. Multi-Tenant Architecture

**Why:** Support multiple companies on same database

**Implementation:**
- company_id on every table
- Row-level security by company_id
- Tenant context in application layer

```sql
-- Query filters by tenant
SELECT * FROM employees 
WHERE company_id = :tenant_id;
```

### 2. JSONB for Flexible Data

**Why:** Some fields vary by company/integration

**Example:**
```sql
-- Employee event with flexible details
INSERT INTO employee_events (employee_id, event_type, event_details)
VALUES (123, 'custom_field_update', 
  '{"field_name": "emergency_contact", "value": "John Smith"}');
```

### 3. Audit Trail (employee_events)

**Why:** GDPR compliance, right to know

**Implementation:**
- Every change tracked with timestamp
- Who made change, when, what system
- Supports GDPR data subject requests

### 4. Self-Referential Foreign Key (manager)

**Why:** Tracks reporting relationships

```sql
-- Employee → Manager (another employee)
ALTER TABLE employees ADD CONSTRAINT fk_manager
  FOREIGN KEY (manager_id) REFERENCES employees(id);
```

### 5. Separate Analytics Tables

**Why:** Don't impact transactional performance

**Pattern:**
- Employees table: current state
- Turnover_risks table: computed risk scores
- Hire_success_scores table: prediction results

---

## Indexing Strategy

### High-Priority Indexes

```sql
-- Frequently Filtered
CREATE INDEX idx_employee_status ON employees(status);
CREATE INDEX idx_employee_department ON employees(department_id);
CREATE INDEX idx_turnover_risk_score ON turnover_risks(risk_score);

-- Date Ranges
CREATE INDEX idx_employee_hire_date ON employees(hire_date);
CREATE INDEX idx_turnover_prediction_date ON turnover_risks(prediction_date);

-- Multi-Column (Common Queries)
CREATE INDEX idx_employee_company_status 
  ON employees(company_id, is_active, status);
  
CREATE INDEX idx_turnover_company_risk 
  ON turnover_risks(company_id, risk_score DESC);
```

### Composite Indexes (Complex Queries)

```sql
-- For reporting queries
CREATE INDEX idx_compensation_analysis_report 
  ON compensation_analysis(company_id, analysis_date DESC, equity_risk_level);

CREATE INDEX idx_engagement_trends 
  ON engagement_scores(company_id, employee_id, survey_date DESC);
```

---

## Data Volume Estimates

### Year 1

| Table | Rows | Size |
|-------|------|------|
| companies | 500 | 50 KB |
| departments | 5,000 | 500 KB |
| employees | 500,000 | 200 MB |
| employee_events | 5,000,000 | 1 GB |
| turnover_risks | 500,000 | 50 MB |
| hire_success_scores | 50,000 | 10 MB |
| compensation_analysis | 500,000 | 50 MB |
| engagement_scores | 2,000,000 | 100 MB |
| audit_logs | 10,000,000 | 2 GB |
| **TOTAL** | | **~3.5 GB** |

---

## Relationships Summary

| From | To | Type | Description |
|------|----|----|---|
| employees | companies | Many-to-One | Employee belongs to company |
| employees | departments | Many-to-One | Employee in department |
| employees | employees | Many-to-One | Manager relationship |
| turnover_risks | employees | Many-to-One | Risk assessment per employee |
| compensation_analysis | employees | Many-to-One | Comp analysis per employee |
| engagement_scores | employees | Many-to-One | Engagement per employee |
| employee_events | employees | Many-to-One | Audit trail |

---

**Last Updated:** July 2026
**Status:** Active Database Schema
**Version:** 1.0
