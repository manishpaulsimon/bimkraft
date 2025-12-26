# Stripe Setup Guide for BIMKraft

**Complete step-by-step guide to set up payments**

---

## Prerequisites

- Business email address
- Bank account details (for payouts)
- Business/personal information
- Tax ID (if applicable)

---

## Step 1: Create Stripe Account

1. Go to https://stripe.com/register
2. Click **"Start now"**
3. Enter your **email** and create a **password**
4. Verify your email (check inbox)
5. Click verification link

---

## Step 2: Complete Business Profile

### Basic Information
1. Go to **Settings** → **Business settings**
2. Fill in:
   - **Business name**: BIMKraft.de (or your legal entity name)
   - **Business type**: Individual or Company
   - **Country**: Germany
   - **Industry**: Software/SaaS
   - **Website**: https://bimkraft.de

### Contact Information
1. **Support email**: support@bimkraft.de
2. **Support phone**: +49 [your phone]
3. **Business address**: Your registered address

### Tax Information
1. **VAT ID**: Your German VAT number (if registered)
2. **Tax classification**: Individual or Business
3. Upload documents if requested

---

## Step 3: Add Bank Account

1. Go to **Settings** → **Bank accounts and scheduling**
2. Click **+ Add bank account**
3. Enter:
   - **Account holder name**: Your name/business name
   - **IBAN**: Your German IBAN
   - **Bank name**: Auto-filled
4. Verify with micro-deposits (Stripe will send small amounts)
5. Confirm amounts within 2-3 business days

---

## Step 4: Create Products

### Product 1: Monthly Subscription

1. Go to **Products** → **+ Add product**
2. Fill in:
   - **Name**: BIMKraft Monthly Subscription
   - **Description**: Monthly access to all 6 BIMKraft tools for Revit
   - **Image**: Upload BIMKraft logo
   - **Pricing**:
     - **Model**: Recurring
     - **Price**: €12.00
     - **Billing period**: Monthly
     - **Currency**: EUR
   - **Tax behavior**:
     - Tax code: Software as a Service (SaaS)
     - ☑️ Prices are inclusive of tax
3. Click **Save product**

### Product 2: Annual Subscription

1. Click **+ Add product** again
2. Fill in:
   - **Name**: BIMKraft Annual Subscription
   - **Description**: Annual access to all 6 BIMKraft tools for Revit (Save 31%)
   - **Image**: Upload BIMKraft logo
   - **Pricing**:
     - **Model**: Recurring
     - **Price**: €99.00
     - **Billing period**: Yearly
     - **Currency**: EUR
   - **Tax behavior**:
     - Tax code: Software as a Service (SaaS)
     - ☑️ Prices are inclusive of tax
3. Click **Save product**

---

## Step 5: Configure Tax Settings

### Enable Tax Collection

1. Go to **Settings** → **Tax settings**
2. Click **Set up tax**
3. Choose **Automatic tax calculation**
4. Select regions:
   - ☑️ **European Union** (for VAT)
   - ☑️ **Germany** (domestic VAT)
5. Stripe will automatically calculate VAT based on customer location

### Tax Rates

Stripe automatically handles:
- **Germany VAT**: 19% (standard rate)
- **EU VAT**: Varies by country (handled automatically)
- **Reverse charge**: For B2B sales within EU
- **Digital services VAT**: Automatic compliance

---

## Step 6: Set Up Customer Portal

1. Go to **Settings** → **Customer portal**
2. Click **Activate**
3. Configure:
   - **Branding**:
     - Upload logo
     - Brand color: #2563eb (BIMKraft blue)
   - **Customer information**:
     - ☑️ Allow customers to update email
     - ☑️ Allow customers to update billing address
   - **Subscription management**:
     - ☑️ Allow customers to cancel subscriptions
     - **Cancel behavior**: At period end
     - ☑️ Allow customers to switch plans
     - ☑️ Allow customers to update payment method
   - **Invoice history**:
     - ☑️ Show invoice history
4. Click **Save**

**Portal URL**: https://billing.stripe.com/p/login/[your-id]

---

## Step 7: Configure Payment Methods

1. Go to **Settings** → **Payment methods**
2. Enable:
   - ☑️ **Cards** (Visa, Mastercard, Amex)
   - ☑️ **SEPA Direct Debit** (for EU customers)
   - ☑️ **Apple Pay** (optional)
   - ☑️ **Google Pay** (optional)
3. Click **Save**

---

## Step 8: Set Up Webhooks (For Phase 2 - Online Licensing)

### Create Webhook Endpoint

1. Go to **Developers** → **Webhooks**
2. Click **+ Add endpoint**
3. Enter:
   - **Endpoint URL**: https://[your-supabase-url].supabase.co/functions/v1/stripe-webhook
   - **Description**: BIMKraft license generation
4. Select events to listen for:
   - ☑️ `checkout.session.completed` - New purchase
   - ☑️ `invoice.paid` - Successful payment
   - ☑️ `invoice.payment_failed` - Failed payment
   - ☑️ `customer.subscription.created` - New subscription
   - ☑️ `customer.subscription.updated` - Subscription changed
   - ☑️ `customer.subscription.deleted` - Subscription cancelled
5. Click **Add endpoint**

### Get Webhook Secret

1. Copy **Signing secret** (starts with `whsec_...`)
2. Save this securely - you'll need it for backend verification
3. Add to environment variables in Supabase

---

## Step 9: Get API Keys

### Test Mode Keys (For Development)

1. Toggle to **Test mode** (top right)
2. Go to **Developers** → **API keys**
3. Copy:
   - **Publishable key**: `pk_test_...`
   - **Secret key**: `sk_test_...` (click "Reveal")
4. Save these securely

### Live Mode Keys (For Production)

1. Toggle to **Live mode**
2. Go to **Developers** → **API keys**
3. Copy:
   - **Publishable key**: `pk_live_...`
   - **Secret key**: `sk_live_...`
4. **IMPORTANT**: Never commit these to GitHub!

---

## Step 10: Create Checkout Session

### Test Checkout Flow

1. Go to **Products** → Select "BIMKraft Monthly"
2. Click **"Create payment link"**
3. Configure:
   - ☑️ Collect customer address
   - ☑️ Collect tax ID (for EU B2B)
   - **After payment**: Success URL
4. Copy the payment link
5. Test the checkout flow in a new incognito window

---

## Step 11: Enable Email Notifications

1. Go to **Settings** → **Emails**
2. Configure customer emails:
   - ☑️ Successful payment
   - ☑️ Failed payment
   - ☑️ Upcoming invoice (3 days before)
   - ☑️ Subscription canceled
   - ☑️ Subscription updated
3. Customize email templates:
   - Add BIMKraft branding
   - Include support contact
   - Add license key instructions (in custom message)

---

## Step 12: Set Up Fraud Prevention

1. Go to **Settings** → **Radar**
2. Stripe Radar is automatically enabled (free with Stripe)
3. Configure rules:
   - **Block**: High-risk payments automatically
   - **Review**: Medium-risk payments manually
   - **Allow**: Low-risk payments
4. Optional: Add custom rules
   - Block payments from high-risk countries
   - Require 3D Secure for amounts >€50

---

## Step 13: Testing Checklist

### Test Cards (Test Mode Only)

**Successful Payment:**
- Card: `4242 4242 4242 4242`
- Expiry: Any future date
- CVC: Any 3 digits
- ZIP: Any 5 digits

**Payment Declined:**
- Card: `4000 0000 0000 0002`

**3D Secure Required:**
- Card: `4000 0025 0000 3155`

### Test Scenarios

- [ ] Create monthly subscription
- [ ] Create annual subscription
- [ ] Cancel subscription (via customer portal)
- [ ] Update payment method
- [ ] Failed payment (card declined)
- [ ] Successful payment after failed
- [ ] Switch from monthly to annual
- [ ] Webhook receives events

---

## Step 14: Go Live Checklist

- [ ] Business profile complete
- [ ] Bank account verified
- [ ] Products created (Monthly & Annual)
- [ ] Tax settings configured
- [ ] Customer portal activated
- [ ] Payment methods enabled
- [ ] Webhooks configured (if using Phase 2)
- [ ] API keys secured
- [ ] Email notifications set up
- [ ] Test checkout flow successful
- [ ] Fraud prevention configured
- [ ] Terms of Service linked in checkout
- [ ] Privacy Policy linked in checkout

---

## Integration Code Snippets

### For Website (Astro)

**Create Checkout Session (Server-side):**

```typescript
// website/src/pages/api/create-checkout.ts
import Stripe from 'stripe';

const stripe = new Stripe(import.meta.env.STRIPE_SECRET_KEY);

export async function post({ request }) {
  const { priceId } = await request.json();

  const session = await stripe.checkout.sessions.create({
    mode: 'subscription',
    payment_method_types: ['card', 'sepa_debit'],
    line_items: [{
      price: priceId,
      quantity: 1,
    }],
    success_url: 'https://bimkraft.de/success?session_id={CHECKOUT_SESSION_ID}',
    cancel_url: 'https://bimkraft.de/pricing',
    automatic_tax: { enabled: true },
    customer_email: undefined, // Let customer enter
    billing_address_collection: 'required',
    tax_id_collection: { enabled: true }, // For EU B2B
  });

  return new Response(JSON.stringify({ url: session.url }), {
    status: 200,
    headers: { 'Content-Type': 'application/json' }
  });
}
```

**Pricing Page Button (Client-side):**

```astro
---
// In pricing.astro
const monthlyPriceId = 'price_xxxxxxxxxxxxx'; // From Stripe dashboard
const annualPriceId = 'price_xxxxxxxxxxxxx';
---

<script>
  async function checkout(priceId) {
    const response = await fetch('/api/create-checkout', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ priceId })
    });

    const { url } = await response.json();
    window.location.href = url;
  }
</script>

<button onclick={`checkout('${monthlyPriceId}')`}>
  Get Monthly - €12/month
</button>

<button onclick={`checkout('${annualPriceId}')`}>
  Get Annual - €99/year
</button>
```

---

## Price IDs (Save These)

After creating products, save these IDs:

**Monthly Subscription:**
- Price ID: `price_xxxxxxxxxxxxx`
- Product ID: `prod_xxxxxxxxxxxxx`

**Annual Subscription:**
- Price ID: `price_xxxxxxxxxxxxx`
- Product ID: `prod_xxxxxxxxxxxxx`

---

## Stripe Dashboard Overview

**Key Pages:**

- **Home**: Overview of revenue, customers, charges
- **Payments**: All payment transactions
- **Customers**: Customer list and details
- **Subscriptions**: Active/cancelled subscriptions
- **Products**: Manage products and pricing
- **Billing**: Invoices and billing settings
- **Reports**: Revenue reports, tax reports, exports
- **Developers**: API keys, webhooks, logs

---

## Monitoring & Alerts

### Set Up Email Alerts

1. Go to **Settings** → **Notifications**
2. Enable alerts for:
   - ☑️ Failed payments
   - ☑️ Large transactions (>€500)
   - ☑️ Disputes/chargebacks
   - ☑️ Radar fraud alerts
3. Add email: contact@bimkraft.de

### Daily Review

Check dashboard daily for:
- New subscriptions
- Failed payments (retry or contact customer)
- Cancelled subscriptions (ask for feedback)
- Disputes (respond within 7 days)

---

## Fees & Costs

**Stripe Pricing (Europe):**
- **EU cards**: 1.5% + €0.25 per transaction
- **Non-EU cards**: 2.9% + €0.25 per transaction
- **SEPA Direct Debit**: 0.8%, capped at €5
- **Disputes**: €15 per dispute

**Example Calculation:**
- Monthly subscription: €12.00
- Stripe fee (EU card): €0.43
- Your revenue: €11.57
- Annual subscription: €99.00
- Stripe fee (EU card): €1.74
- Your revenue: €97.26

---

## Tax Reporting

Stripe provides:
- **Monthly invoices** with fee breakdown
- **Annual 1099-K** (if US-based, not applicable for Germany)
- **VAT reports** for EU compliance
- **Export to CSV** for accounting software

**Download Reports:**
1. Go to **Reports** → **Revenue**
2. Select date range
3. Download CSV
4. Import into accounting software

---

## Support & Resources

**Stripe Support:**
- Email: support@stripe.com
- Chat: Available in dashboard
- Documentation: https://stripe.com/docs
- API reference: https://stripe.com/docs/api

**BIMKraft Integration Help:**
- Email: support@bimkraft.de
- Documentation: This guide

---

## Security Best Practices

1. **Never commit API keys** to version control
2. **Use environment variables** for secrets
3. **Verify webhook signatures** in backend
4. **Use HTTPS only** for all requests
5. **Enable 2FA** on Stripe account
6. **Review Radar alerts** regularly
7. **Keep API libraries updated**
8. **Log all transactions** for auditing

---

## Next Steps

After completing this setup:

1. ✅ **Test thoroughly** in test mode
2. ✅ **Update pricing page** with real Stripe checkout buttons
3. ✅ **Implement webhook handler** (Phase 2)
4. ✅ **Go live** when ready
5. ✅ **Monitor dashboard** daily

---

**Questions?** Contact support@bimkraft.de

*Last Updated: December 26, 2025*
