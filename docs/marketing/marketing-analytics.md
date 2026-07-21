# Marketing Analytics & Measurement

KPIs, dashboards, tracking mechanisms, and ROI measurement framework.

---

## Marketing Funnel & Metrics

```
┌────────────────────────────────────────────────┐
│ AWARENESS (Top of Funnel)                      │
├────────────────────────────────────────────────┤
│ Impressions, Reach, Awareness                  │
│ Metrics: Impressions, reach, brand lift        │
│ Tools: Google Analytics, social analytics      │
│ Goal: 1M+ impressions/month                    │
└────────────────────┬─────────────────────────────┘
                     │
        Conversion Rate: 0.1-0.5%
                     │
                     ▼
┌────────────────────────────────────────────────┐
│ CONSIDERATION (Middle of Funnel)               │
├────────────────────────────────────────────────┤
│ Traffic, Engagement, Leads                     │
│ Metrics: Visitors, email opens, webinar reg    │
│ Tools: Google Analytics, email platform, CRM   │
│ Goal: 50k visitors/month, 500 MQLs/month       │
└────────────────────┬─────────────────────────────┘
                     │
        Conversion Rate: 5-10%
                     │
                     ▼
┌────────────────────────────────────────────────┐
│ DECISION (Bottom of Funnel)                    │
├────────────────────────────────────────────────┤
│ Leads, Trials, Demos, Sales Pipeline           │
│ Metrics: MQLs, SQLs, pipeline, close rate      │
│ Tools: CRM (Salesforce), marketing automation  │
│ Goal: 500 MQLs → 100 SQLs → 20-30 customers    │
└────────────────────────────────────────────────┘
```

---

## Tier 1: Business Metrics (Most Important)

### Monthly Recurring Revenue (MRR)
- **Owned by:** VP Revenue
- **Target:** $0 → $500k MRR (Year 1)
- **Calculation:** # Customers × Average Revenue Per Customer
- **Tracking:** Salesforce, financial dashboard
- **Target by Month:**
  - Q1: $50k MRR
  - Q2: $150k MRR
  - Q3: $300k MRR
  - Q4: $500k MRR

### Annual Contract Value (ACV)
- **Owned by:** Sales Leadership
- **Target:** $60k average
- **Calculation:** Total revenue ÷ # Customers
- **Tracking:** Salesforce
- **Segment by:**
  - Starter tier: $30k
  - Professional tier: $60k
  - Enterprise tier: $120k+

### Customer Acquisition Cost (CAC)
- **Owned by:** VP Marketing
- **Target:** <$500
- **Calculation:** Total marketing spend ÷ New customers acquired
- **Tracking:** Marketing automation + CRM
- **Formula:** CAC = Marketing spend / Customers acquired
- **Example:** $250k spent / 500 customers = $500 CAC
- **Payback Period:** CAC ÷ (ACV / 12 months) = ~10 months (good)

### Revenue Pipeline
- **Owned by:** VP Revenue
- **Target:** $20M+ pipeline by Q4
- **Calculation:** # Opportunities × Win rate × Average deal size
- **Tracking:** Salesforce
- **Example:** 330 opportunities × 25% win rate × $60k ACV = $4.95M → 20M needed for expansion

### Win Rate / Close Rate
- **Owned by:** VP Revenue
- **Target:** 20-30%
- **Calculation:** # Closed won ÷ # Opportunities
- **Tracking:** Salesforce
- **Benchmark:** 20% = conservative, 30% = strong

---

## Tier 2: Marketing Metrics

### Monthly Leads (MQLs)
- **Owned by:** Director, Demand Gen
- **Target:** 500 MQLs/month by Year 1
- **Calculation:** Sum of all lead sources
- **Tracking:** Marketo/HubSpot
- **Breakdown by source:**
  - Organic: 200/month (40%)
  - Paid ads: 200/month (40%)
  - Partnerships/Events: 100/month (20%)

### Lead Scoring
- **Owned by:** Marketing Operations
- **Threshold for MQL:** 40+ points (indicates ready to engage)
- **Scoring model:**
  - Job title (executive: +10, manager: +5)
  - Company size (250-2k: +10)
  - Engagement (email open: +2, download: +5, demo signup: +15)
  - Fit (our ICP: +20, close: +5)
- **Tracking:** Marketo lead score

### Lead Quality Score
- **Owned by:** VP Revenue
- **Measurement:** % of MQLs that convert to SQL (20%+)
- **Tracking:** Salesforce, CRM
- **Target:** 20%+ of MQLs become SQLs (engaged, ready to sales)

### Cost Per Lead (CPL)
- **Owned by:** Director, Demand Gen
- **Calculation:** Marketing spend ÷ Total leads generated
- **Tracking:** Marketo + UTM parameters
- **Target:** <$100 CPL
- **By channel:**
  - Organic: ~$50 CPL (content, SEO)
  - Paid ads: $250-500 CPL (LinkedIn, Google)
  - Partnerships: $150-200 CPL (events, referrals)

### Marketing Qualified Lead (MQL) to Sales Qualified Lead (SQL) Conversion
- **Owned by:** VP Revenue + VP Marketing
- **Target:** 20-30% of MQLs → SQLs
- **Calculation:** SQLs / MQLs
- **Tracking:** Salesforce
- **Improvement:** Better scoring, nurturing, sales follow-up

---

## Tier 3: Website & Traffic Metrics

### Monthly Website Visitors
- **Owned by:** Director, Content & SEO
- **Target:** 50k/month by Q4
- **Tracking:** Google Analytics
- **Segment by:**
  - Organic: 40% (20k)
  - Paid ads: 35% (17.5k)
  - Direct: 15% (7.5k)
  - Referral: 10% (5k)

### Website Conversion Rate (Visitor to Lead)
- **Owned by:** Director, Demand Gen
- **Target:** 2-3%
- **Calculation:** Leads / Visitors
- **Tracking:** Google Analytics + Marketo
- **Example:** 50k visitors × 2% = 1,000 leads
- **Improvement:** Better CTA, better landing pages, better offer

### Page Performance
- **Owned by:** Director, Content & SEO
- **Top pages tracked:**
  - Homepage (goal: 5-8% conversion to lead)
  - Pricing page (goal: 10%+ conversion)
  - Features page (goal: 5-8% conversion)
  - Blog posts (goal: 2-3% conversion to lead or social share)
- **Tracking:** Google Analytics, Hotjar
- **Optimization:** A/B test, redesign underperformers

### Bounce Rate
- **Owned by:** Director, Content & SEO
- **Target:** <50% (website), <40% (landing pages)
- **Calculation:** Visits with bounce / Total visits
- **Tracking:** Google Analytics
- **High bounce: Indicates poor fit between ad and page

### Time on Site
- **Owned by:** Director, Content & SEO
- **Target:** 2-3 minutes average
- **Tracking:** Google Analytics
- **Indicates:** Content quality, relevance

### Ranking Keywords
- **Owned by:** SEO Specialist
- **Target:** 50+ keywords ranking in top 10, 20+ in top 3
- **Tracking:** SEMrush, Ahrefs, Google Search Console
- **Top keywords:**
  - "HR analytics tool"
  - "People analytics"
  - "Employee turnover prediction"
  - "Hire success prediction"

---

## Tier 4: Email Metrics

### Email List Size
- **Owned by:** Email Manager
- **Target:** 10k subscribers by Q4
- **Tracking:** Email platform (Marketo)
- **Growth rate:** +200-300/week

### Email Open Rate
- **Owned by:** Email Manager
- **Target:** 25%+ (strong performer)
- **Benchmark:** 15-20% industry average
- **Calculation:** Opens / Sends
- **Tracking:** Email platform
- **Improvement:** Better subject lines, segment by interest

### Email Click-Through Rate (CTR)
- **Owned by:** Email Manager
- **Target:** 3%+ (strong performer)
- **Benchmark:** 2-3% industry average
- **Calculation:** Clicks / Opens
- **Tracking:** Email platform
- **Improvement:** Better content, clearer CTA

### Email Conversion Rate
- **Owned by:** Email Manager
- **Target:** 1%+ of emails lead to trial/demo signup
- **Calculation:** Conversions / Opens
- **Tracking:** UTM parameters + CRM
- **Example:** 10k emails × 25% open × 1% conversion = 25 conversions

### Email Unsubscribe Rate
- **Owned by:** Email Manager
- **Target:** <0.5% (low churn)
- **Benchmark:** 0.2-0.5% healthy
- **Tracking:** Email platform
- **High unsubscribe: Indicates poor segmentation, irrelevant content

---

## Tier 5: Social Media Metrics

### Followers/Followers Growth
- **Owned by:** Social Manager
- **Targets by platform:**
  - LinkedIn: 15k followers
  - Twitter: 5k followers
  - YouTube: 2k subscribers
  - Facebook: 2k followers
  - Instagram: 1k followers
  - Total: 25k

### Engagement Rate
- **Owned by:** Social Manager
- **Target:** 3%+ on LinkedIn/Twitter, 2%+ on Facebook
- **Calculation:** (Likes + Comments + Shares + Clicks) / Impressions
- **Tracking:** Platform analytics
- **High engagement: Indicates content resonance

### Reach
- **Owned by:** Social Manager
- **Calculation:** # Unique people who saw content
- **Tracking:** Platform analytics
- **Goal:** 500k reach/month

### Click-Through Rate (to Website)
- **Owned by:** Social Manager
- **Target:** 0.5-1%
- **Calculation:** Website clicks / Impressions
- **Tracking:** UTM parameters (utm_source=social)
- **Drives website traffic and leads

### Video Views
- **Owned by:** Social Manager
- **Target:** 50k monthly video views
- **Tracking:** YouTube, platform analytics
- **Note:** Count only views > 3 seconds (engaged)

---

## Tier 6: Paid Advertising Metrics

### Cost Per Click (CPC)
- **Owned by:** Director, Demand Gen
- **Targets by channel:**
  - LinkedIn: $2-5
  - Google Ads: $3-10
  - Facebook: $0.50-2.00
- **Calculation:** Ad spend / Clicks
- **Tracking:** Ad platform native analytics

### Click-Through Rate (CTR)
- **Owned by:** Director, Demand Gen
- **Target:** 1%+ for search, 0.5%+ for social
- **Calculation:** Clicks / Impressions
- **Tracking:** Ad platform
- **Indicates:** Ad relevance

### Cost Per Acquisition (CPA)
- **Owned by:** Director, Demand Gen
- **Target:** $200-500 per customer acquired
- **Calculation:** Ad spend / Customers acquired
- **Tracking:** UTM parameters + CRM
- **Example:** $250k spend / 500 customers = $500 CPA

### Return on Ad Spend (ROAS)
- **Owned by:** Director, Demand Gen
- **Target:** 3:1+ (spend $1, get $3 back)
- **Calculation:** Revenue / Ad spend
- **Tracking:** UTM parameters + CRM + Salesforce
- **Example:** $5M revenue / $250k spend = 20:1 ROAS (excellent)

### Quality Score (Google Ads)
- **Owned by:** Director, Demand Gen
- **Target:** 7+ out of 10
- **Factors:**
  - Expected CTR (7+: high)
  - Landing page experience (landing page quality)
  - Ad relevance (match between ad and keyword)
- **Tracking:** Google Ads interface
- **Improvement: Better keywords, better ads, faster landing pages

### Impression Share
- **Owned by:** Director, Demand Gen
- **Calculation:** # Impressions / # Eligible impressions
- **Target:** 50%+ impression share on key keywords
- **Tracking:** Google Ads
- **Low share: Need higher budget or better bidding

---

## Dashboards & Reporting

### Daily Dashboard (Updated automatically)
- MQLs generated today
- Website traffic today
- Email clicks today
- Ad spend today
- Pipeline change (won, lost)

### Weekly Dashboard (Monday recap)
- MQLs generated (target: 120)
- Website visitors (target: 11,500)
- Email subscribers added
- Social followers added
- Major wins/blockers

### Monthly Dashboard (Comprehensive)
- Revenue metrics (MRR, ARR, pipeline)
- Lead metrics (MQLs, SQL conversion, quality)
- Web metrics (traffic, conversion, ranking keywords)
- Email metrics (subscribers, open rate, CTR)
- Paid ad metrics (spend, CPA, ROAS, quality score)
- Social metrics (followers, engagement, reach)
- Budget vs. forecast vs. actual
- Top performing campaigns
- Underperforming areas

### Quarterly Review (Strategic)
- Full year-to-date performance vs. goals
- Progress against annual targets
- Channel performance (organic vs. paid)
- Customer segment performance
- Win/loss analysis
- Budget efficiency
- Team performance
- Recommendations for next quarter

---

## UTM Parameter Strategy

All campaigns must use UTM parameters for tracking.

**Format:**
```
https://website.com/page?utm_source=linkedin&utm_medium=cpc&utm_campaign=awareness_q1&utm_content=ad_v1&utm_term=hr_analytics
```

**Parameters:**
- `utm_source`: Where the traffic comes from (linkedin, google, facebook, newsletter)
- `utm_medium`: Type of marketing (cpc, organic, social, email)
- `utm_campaign`: Campaign name (awareness_q1, retention_feature, holiday_sale)
- `utm_content`: Ad variant (ad_v1, image_carousel, video_demo)
- `utm_term`: Keyword (for search ads) or targeting (for social)

**Tracking:**
- Visible in Google Analytics
- Import to CRM for lead source tracking
- Use for attribution modeling

---

## Attribution Modeling

### First-Touch Attribution
- Give credit to first touchpoint
- Good for: Awareness campaigns
- Example: LinkedIn awareness ad got them to site

### Last-Touch Attribution
- Give credit to last touchpoint
- Good for: Bottom-funnel campaigns
- Example: Google Ads search led to trial

### Multi-Touch Attribution
- Distribute credit across touchpoints
- Example: 40% to first touch, 20% to middle, 40% to last
- Good for: Understanding full customer journey

---

## Measurement Tools

| Tool | Purpose | Cost |
|------|---------|------|
| Google Analytics | Website traffic, behavior | Free |
| Marketo | Email, lead scoring, attribution | $5k/month |
| Salesforce | CRM, pipeline tracking | $3k/month |
| UTM Builder | Create UTM parameters | Free |
| Google Search Console | SEO, keywords, rankings | Free |
| SEMrush | Competitive analysis, backlinks | $500/month |
| Mixpanel | Funnel analysis, cohorts | $1k/month |
| Hotjar | User behavior, heatmaps | $300/month |

---

**Last Updated:** July 2026
**Next Review:** Monthly
**Owner:** Marketing Operations Manager
