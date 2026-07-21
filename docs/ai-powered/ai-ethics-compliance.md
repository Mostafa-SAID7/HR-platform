# AI Ethics & Compliance

Comprehensive guide to ethical AI, bias mitigation, compliance, and responsible AI practices.

---

## AI Ethics Framework

**Core Principles:**
1. **Fairness** - Avoid discriminatory outcomes
2. **Transparency** - Explain decisions
3. **Accountability** - Responsibility & oversight
4. **Privacy** - Protect employee data
5. **Security** - Safeguard against misuse

---

## Bias Detection & Mitigation

### Types of Bias

**1. Historical Bias** (Data reflects past discrimination)
```
Example: Turnover models trained on data showing women 
leave at higher rates due to lack of sponsorship
→ Result: Model flags women as higher risk (continues bias)
```

**2. Measurement Bias** (How we measure things)
```
Example: Using LinkedIn profile updates as engagement signal
→ Result: Different demographics use LinkedIn differently
```

**3. Aggregation Bias** (Averaging hides subgroup issues)
```
Example: Average pay gap is small, but women in tech have 20% gap
→ Result: Aggregate metric hides real problems
```

### Fairness Metrics Tracked

```python
from fairlearn.metrics import demographic_parity_difference, equalized_odds_difference

def evaluate_fairness(y_true, y_pred, protected_attr):
    """Comprehensive fairness evaluation"""
    
    metrics = {}
    
    # Demographic Parity: Same prediction rate across groups
    dpd = demographic_parity_difference(
        y_true, y_pred, 
        sensitive_features=protected_attr
    )
    metrics['demographic_parity_diff'] = dpd
    
    # Equalized Odds: Same TPR and FPR across groups
    eod = equalized_odds_difference(
        y_true, y_pred,
        sensitive_features=protected_attr
    )
    metrics['equalized_odds_diff'] = eod
    
    # Calibration: Predicted prob = actual prob within groups
    for group in protected_attr.unique():
        mask = protected_attr == group
        calibration = calibration_curve(y_true[mask], y_pred[mask])
        metrics[f'calibration_{group}'] = calibration
    
    return metrics
```

### Bias Mitigation Strategies

**1. Data-Level Mitigation**
```python
# Balanced sampling - Oversample underrepresented groups
from imblearn.over_sampling import RandomOverSampler

sampler = RandomOverSampler(random_state=42)
X_balanced, y_balanced = sampler.fit_resample(X, y)

# Reweighting - Give higher weight to underrepresented samples
sample_weights = compute_class_weight('balanced', y, y)
model.fit(X, y, sample_weight=sample_weights)
```

**2. Algorithm-Level Mitigation**
```python
# Fairness constraints in loss function
from fairlearn.reductions import GridSearch, DemographicParity

constraint = DemographicParity(difference_bound=0.01)
sweep = GridSearch(
    estimator=LogisticRegression(),
    constraints=constraint,
    grid_size=71
)
sweep.fit(X_train, y_train, sensitive_features=protected_features_train)
```

**3. Post-Processing Mitigation**
```python
# Threshold adjustment per group
from fairlearn.postprocessing import ThresholdOptimizer

threshold_optimizer = ThresholdOptimizer(
    estimator=model,
    constraints="equalized_odds_difference"
)
threshold_optimizer.fit(X_val, y_val, sensitive_features=protected_features_val)
```

---

## Transparency & Explainability

### SHAP Explanations

```python
import shap

# Generate explanations
explainer = shap.TreeExplainer(model)
shap_values = explainer.shap_values(X)

# User-facing explanation
def generate_explanation(employee_id, shap_values, feature_names):
    """Generate human-readable explanation"""
    
    explanation = {
        'employee_id': employee_id,
        'prediction': prediction,
        'primary_factors': []
    }
    
    # Top 5 driving factors
    top_factors = np.argsort(np.abs(shap_values[employee_id]))[-5:]
    
    for factor_idx in top_factors:
        explanation['primary_factors'].append({
            'name': feature_names[factor_idx],
            'contribution': float(shap_values[employee_id][factor_idx]),
            'direction': 'increases risk' if shap_values[...][factor_idx] > 0 else 'decreases risk'
        })
    
    return explanation
```

### Documentation Requirements

**Every Model Deployment Requires:**
1. ✅ **Model Card** - What does it do, training data, performance
2. ✅ **Data Sheet** - Data sources, preprocessing, issues
3. ✅ **Bias Assessment** - Fairness metrics, identified biases
4. ✅ **Use Case Guidelines** - Intended use, limitations
5. ✅ **Human Review Process** - When humans override model

---

## Compliance Frameworks

### GDPR (EU Data Protection)

**Key Requirements:**

**1. Right to Explanation**
```
Requirement: Employees can request explanation for AI decisions
Implementation: 
- Store SHAP values for all predictions
- Generate explanation on request
- Return in 30 days (GDPR deadline)
```

**2. Right to Deletion**
```
Requirement: Employees can request data deletion
Implementation:
- Flag employee data for deletion
- Purge from all systems (data lake, models, logs)
- Certify deletion
```

**3. Data Minimization**
```
Requirement: Only use data necessary for purpose
Implementation:
- Feature selection: 45 features (not 1,000)
- Regular audit: Remove unnecessary features
- Document justification for each feature
```

**4. Lawful Processing Basis**
```
GDPR Article 6 - Lawful basis for processing:
✅ Consent - Employee agrees to analysis
✅ Contract - Analysis supports employment agreement
✅ Legal obligation - Compliance/audit requirements
✅ Legitimate interests - Business benefit > privacy harm
```

### CCPA (California Privacy Rights)

**Key Requirements:**

```python
ccpa_compliance = {
    'right_to_know': {
        'requirement': 'Tell consumers what data is collected',
        'implementation': 'Privacy policy lists all 45 features'
    },
    'right_to_delete': {
        'requirement': 'Delete consumer data on request',
        'implementation': 'GDPR compliance (see above)'
    },
    'right_to_opt_out': {
        'requirement': 'Opt out of data sale',
        'implementation': 'Do not sell data (verified in contracts)'
    },
    'no_discrimination': {
        'requirement': 'No penalty for exercising rights',
        'implementation': 'HR policy forbids retaliation for data requests'
    }
}
```

### FCRA (Fair Credit Reporting Act) - US

**Applies to: HR Analytics if used for employment decisions**

**Requirements:**
1. ✅ Accuracy and completeness
2. ✅ Opt-in/notice for automated decisions
3. ✅ Right to dispute
4. ✅ Right to know if report used against applicant
5. ✅ No adverse action without notice

**Implementation:**
```python
class FCRACompliance:
    def before_adverse_action(self, employee_id, prediction):
        """Required steps before taking action on prediction"""
        
        # 1. Verify data accuracy
        self.verify_source_data(employee_id)
        
        # 2. Notify employee
        self.send_notice(
            employee_id,
            f"We're considering {prediction['action']} based on our analysis"
        )
        
        # 3. Allow dispute period (30 days)
        self.create_dispute_window(employee_id, days=30)
        
        # 4. Only proceed if no dispute or dispute resolved
        if not self.has_unresolved_dispute(employee_id):
            return True
```

### EEOC (Equal Employment Opportunity Commission)

**Requirement: Non-discrimination in employment**

**What's Prohibited:**
- Discrimination based on: race, color, religion, sex, national origin, age, disability, genetic info
- Retaliation for complaint

**Our Compliance:**
```python
class EEOCCompliance:
    def audit_for_discrimination(self):
        """Regular audit for adverse impact"""
        
        protected_classes = ['race', 'gender', 'age', 'disability']
        
        for protected_class in protected_classes:
            # Calculate adverse impact ratio
            # If selection rate for group < 80% of other group → adverse impact
            
            for decision_type in ['hire', 'promotion', 'termination']:
                by_group = self.analyze_by_group(decision_type, protected_class)
                
                if by_group.adverse_impact_detected():
                    self.alert_legal_team()
                    self.document_business_justification()
```

---

## Responsible AI Checklist

**Before Deploying Any Model:**

□ **Data Quality**
  - [ ] Data sources documented
  - [ ] Data quality rules in place
  - [ ] Missing data handled
  - [ ] Outliers identified & handled

□ **Fairness**
  - [ ] Demographic parity check (within 5% between groups)
  - [ ] Equalized odds check
  - [ ] Calibration check
  - [ ] Bias mitigation applied
  - [ ] Fairness documented

□ **Transparency**
  - [ ] SHAP explanations generated
  - [ ] Feature importance documented
  - [ ] Model card completed
  - [ ] Limitations clearly stated
  - [ ] Human review process defined

□ **Privacy**
  - [ ] GDPR compliance verified
  - [ ] CCPA compliance verified
  - [ ] Data minimization enforced
  - [ ] Encryption in place
  - [ ] Access controls restricted

□ **Security**
  - [ ] Model adversarial testing done
  - [ ] Data poisoning resistance checked
  - [ ] Access audit completed
  - [ ] Incident response plan created
  - [ ] Monitoring enabled

□ **Legal**
  - [ ] Legal review completed
  - [ ] Employment law compliance verified
  - [ ] Notification language approved
  - [ ] Right to dispute mechanism built
  - [ ] Audit trail enabled

□ **Monitoring**
  - [ ] Performance metrics defined
  - [ ] Fairness metrics tracked
  - [ ] Drift detection enabled
  - [ ] Alert thresholds set
  - [ ] Retraining policy documented

---

## Responsible Use Guidelines

### ✅ Recommended Uses

**Retention Analysis:**
- ✅ Identify retention risks for proactive retention
- ✅ Support human decision-making
- ✅ Combined with manager judgment

**Hiring Support:**
- ✅ Predict hire success (not go/no-go decision)
- ✅ Flag for additional review
- ✅ Always human final decision

**Development:**
- ✅ Identify promotion readiness
- ✅ Suggest training/mentoring
- ✅ Support career development

### ❌ Prohibited Uses

**❌ Automatic Termination**
- Cannot automatically fire based on prediction
- Requires human review

**❌ Ranking Employees**
- Unfair to rank humans as "good" vs "bad"
- Creates toxic culture

**❌ Real-Time Monitoring**
- Don't track when employees look at job boards
- Privacy violation & illegal in many jurisdictions

**❌ Prediction-Based Compensation**
- Don't adjust pay based on turnover risk prediction
- Creates self-fulfilling prophecy

---

## Incident Response

### What to Do If Bias Detected

```
1. IMMEDIATE (Same day)
   └─ Pause model deployment
   └─ Alert leadership & legal
   └─ Notify affected employees

2. SHORT-TERM (1 week)
   └─ Investigate root cause
   └─ Assess impact (how many affected)
   └─ Determine if discrimination occurred
   └─ Plan remediation

3. MEDIUM-TERM (2 weeks)
   └─ Apply bias mitigation
   └─ Retrain model
   └─ Fairness audit
   └─ External audit (optional but recommended)

4. LONG-TERM (1 month)
   └─ Implement controls to prevent recurrence
   └─ Update policies & guidelines
   └─ Employee communications
   └─ External disclosure (if required)
```

---

## Transparency Report Template

**Annual AI Transparency Report:**

```markdown
# AI Transparency Report 2026

## Executive Summary
- 6 production AI models deployed
- 1.2M employees analyzed
- 0 discrimination incidents

## Models Deployed
1. Turnover Risk Scoring
   - Accuracy: 85%
   - Fairness: Demographic parity 3% (good)
   - Annual decisions: 50,000+

2. [Others...]

## Fairness & Bias
- Demographic parity (gender): 2% difference ✓
- Demographic parity (race): 4% difference ✓
- No models failed fairness thresholds

## Privacy & Data
- 0 data breaches
- 100% GDPR compliant
- 0 unauthorized access incidents

## Governance
- 12 model reviews conducted
- 2 bias mitigations implemented
- 100% compliance with Responsible AI checklist

## Employee Impact
- 5,000 employees received retention offer (stayed)
- 1,200 hired through optimized process
- 200 promoted based on readiness scores

## Recommendations
1. Expand fairness monitoring to real-time
2. Increase external audits (recommended annually)
3. Build explainability dashboard for employees
```

---

## Governance Structure

```
┌─────────────────────────────────────────────────────┐
│        AI Ethics & Compliance Board                 │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Chief People Officer (Chair)                      │
│  ├─ VP Data Science (Model Performance)            │
│  ├─ Chief Legal Officer (Compliance)               │
│  ├─ Chief Privacy Officer (Data Protection)        │
│  ├─ Head of Diversity & Inclusion (Fairness)      │
│  ├─ CISO (Security)                                │
│  └─ External Ethics Advisor (Oversight)            │
│                                                     │
│  Responsibilities:                                 │
│  ├─ Review new models before deployment           │
│  ├─ Monthly fairness audits                        │
│  ├─ Incident response                              │
│  ├─ Policy updates                                 │
│  └─ External communication                         │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## Conclusion

**HR Analytics AI Ethics & Compliance:**

✅ **Fairness:** Demographic parity < 5%, equalized odds tracked
✅ **Transparency:** SHAP explanations, model cards, audit trails
✅ **Privacy:** GDPR/CCPA compliant, data minimization enforced
✅ **Accountability:** Governance board, incident response, annual reports
✅ **Security:** Adversarial testing, access controls, monitoring

**Key Commitments:**
1. ✅ No discrimination - Fair to all protected classes
2. ✅ Transparency - Employees know how decisions made
3. ✅ Human oversight - Humans make final decisions
4. ✅ Continuous improvement - Regular fairness audits
5. ✅ Accountability - Document all decisions & impacts

**Next Steps:**
1. Implement fairness monitoring dashboard
2. Conduct annual external audit
3. Expand fairness metrics tracking
4. Build employee explanation interface
5. Develop adversarial testing protocol
