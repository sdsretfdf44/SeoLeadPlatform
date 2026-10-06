#!/usr/bin/env bash
set -euo pipefail

API="${API:-http://localhost:5080}"
EMAIL="${EMAIL:-demo@example.com}"
PASSWORD="${PASSWORD:-demo-password-123}"

echo "==> Login (or register) $EMAIL"
LOGIN_RESPONSE=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}") || true

TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4 || true)

if [ -z "${TOKEN:-}" ]; then
  echo "==> Login failed, registering demo user"
  REG_RESPONSE=$(curl -s -X POST "$API/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"agencyName\":\"Demo Agency\",\"fullName\":\"Demo User\",\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")
  TOKEN=$(echo "$REG_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
fi

if [ -z "${TOKEN:-}" ]; then
  echo "Failed to obtain token" >&2
  exit 1
fi

AUTH="Authorization: Bearer $TOKEN"
JSON="Content-Type: application/json"
echo "Token acquired (${#TOKEN} chars)"

echo "==> Setting up agency profile and service offerings"
curl -s -X PUT "$API/api/agency" -H "$AUTH" -H "$JSON" -d '{
  "name": "Demo Agency",
  "websiteUrl": "https://demoagency.example",
  "description": "A digital marketing agency helping SMBs win with SEO.",
  "targetIndustries": "dental, legal, home services, e-commerce, SaaS",
  "targetLocations": "US, UK, Canada",
  "targetServices": "Local SEO, technical SEO, content marketing",
  "minimumProjectValue": 3000,
  "preferredProjectTypes": "monthly retainer"
}' > /dev/null

for SVC in \
  '{"name":"Technical SEO","description":"Audit + fixes for crawlability, speed and structure.","typicalProjectValue":4500,"typicalMonthlyRetainer":1800}' \
  '{"name":"Local SEO","description":"GBP, citations, local landing pages.","typicalProjectValue":3500,"typicalMonthlyRetainer":1500}' \
  '{"name":"Content marketing","description":"Editorial planning, briefs, and production.","typicalProjectValue":6000,"typicalMonthlyRetainer":2500}' \
  '{"name":"On-page SEO","description":"Metadata, IA, internal linking, schema.","typicalProjectValue":4000,"typicalMonthlyRetainer":1600}'
do
  curl -s -X POST "$API/api/agency/services" -H "$AUTH" -H "$JSON" -d "$SVC" > /dev/null
done

echo "==> Bulk-importing prospects"
IMPORT_BODY=$(cat <<'EOF'
{
  "rows": [
    { "companyName": "Smashing Magazine", "websiteUrl": "https://www.smashingmagazine.com", "industry": "Publishing", "country": "DE", "city": "Freiburg", "primaryEmail": "hello@smashingmagazine.com" },
    { "companyName": "CSS-Tricks", "websiteUrl": "https://css-tricks.com", "industry": "Publishing", "country": "US" },
    { "companyName": "Example Corp", "websiteUrl": "https://example.com", "industry": "SaaS", "country": "US", "city": "San Francisco" },
    { "companyName": "MDN Web Docs", "websiteUrl": "https://developer.mozilla.org", "industry": "Documentation", "country": "US" },
    { "companyName": "GitHub", "websiteUrl": "https://github.com", "industry": "SaaS", "country": "US", "city": "San Francisco" },
    { "companyName": "Hacker News", "websiteUrl": "https://news.ycombinator.com", "industry": "News", "country": "US" }
  ]
}
EOF
)
curl -s -X POST "$API/api/prospects/import" -H "$AUTH" -H "$JSON" -d "$IMPORT_BODY" | head -c 200
echo ""

echo "==> Adding one manually with contact info"
MANUAL=$(curl -s -X POST "$API/api/prospects" -H "$AUTH" -H "$JSON" -d '{
  "companyName": "Acme Dental Group",
  "websiteUrl": "https://www.wikipedia.org",
  "industry": "Dental",
  "country": "US",
  "state": "TX",
  "city": "Austin",
  "primaryEmail": "hello@acmedental.example",
  "phoneNumber": "+1 555 0100"
}')
ACME_ID=$(echo "$MANUAL" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
echo "Manual prospect id=$ACME_ID"

if [ -n "$ACME_ID" ]; then
  curl -s -X POST "$API/api/prospects/$ACME_ID/contacts" -H "$AUTH" -H "$JSON" -d '{
    "fullName": "Dr. Jane Doe",
    "jobTitle": "Owner / Lead dentist",
    "email": "jane@acmedental.example",
    "phoneNumber": "+1 555 0100",
    "isDecisionMaker": true
  }' > /dev/null
fi

echo "==> Fetching prospect list to trigger audits"
PROSPECT_IDS=$(curl -s "$API/api/prospects?pageSize=50" -H "$AUTH" | grep -o '"id":"[^"]*"' | cut -d'"' -f4 | sort -u)

for PID in $PROSPECT_IDS; do
  curl -s -X POST "$API/api/prospects/$PID/audit" -H "$AUTH" > /dev/null
  echo "  queued audit for $PID"
done

echo "==> Waiting up to 90s for audits to complete"
for i in $(seq 1 30); do
  PENDING=$(curl -s "$API/api/prospects?pageSize=50" -H "$AUTH" | grep -o '"latestAuditStatus":"[^"]*"' | grep -cE 'Queued|Running' || true)
  echo "  pending audits: $PENDING (attempt $i)"
  if [ "$PENDING" = "0" ]; then break; fi
  sleep 3
done

echo "==> Qualifying top prospects and generating outreach + proposals"
COMPLETED=$(curl -s "$API/api/prospects?pageSize=50&sortBy=opportunityScore" -H "$AUTH")
TOP_IDS=$(echo "$COMPLETED" | tr ',' '\n' | grep -A1 '"latestAuditStatus":"Completed"' | grep -o '"id":"[^"]*"' | cut -d'"' -f4 | head -3 || true)

# fallback: pick first 3 ids
if [ -z "$TOP_IDS" ]; then
  TOP_IDS=$(echo "$PROSPECT_IDS" | head -3)
fi

for PID in $TOP_IDS; do
  echo "  qualifying $PID"
  curl -s -X POST "$API/api/prospects/$PID/qualify" -H "$AUTH" > /dev/null || true
  echo "  outreach $PID"
  curl -s -X POST "$API/api/prospects/$PID/outreach" -H "$AUTH" -H "$JSON" -d '{"type":"InitialEmail"}' > /dev/null || true
  echo "  proposal $PID"
  curl -s -X POST "$API/api/proposals/generate/$PID" -H "$AUTH" > /dev/null || true
done

# For the manual Acme prospect, also mark outreach sent and lead as MeetingScheduled to populate funnel
if [ -n "$ACME_ID" ]; then
  ACME_OUT=$(curl -s "$API/api/prospects/$ACME_ID/outreach" -H "$AUTH")
  ACME_MSG_ID=$(echo "$ACME_OUT" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
  if [ -n "$ACME_MSG_ID" ]; then
    curl -s -X POST "$API/api/outreach/$ACME_MSG_ID/sent" -H "$AUTH" > /dev/null || true
    echo "  marked outreach $ACME_MSG_ID sent"
  fi
  ACME_DETAIL=$(curl -s "$API/api/prospects/$ACME_ID" -H "$AUTH")
  ACME_LEAD_ID=$(echo "$ACME_DETAIL" | grep -o '"lead":{[^}]*}' | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
  if [ -n "$ACME_LEAD_ID" ]; then
    curl -s -X PATCH "$API/api/leads/$ACME_LEAD_ID/status" -H "$AUTH" -H "$JSON" -d '{"status":"MeetingScheduled"}' > /dev/null || true
    echo "  advanced lead $ACME_LEAD_ID to MeetingScheduled"
  fi
fi

echo "==> Dashboard summary"
curl -s "$API/api/dashboard/summary" -H "$AUTH" | head -c 600
echo ""
echo ""
echo "Done. Log in at http://localhost:5173 with $EMAIL / $PASSWORD"
