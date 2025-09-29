# Microsoft Entra External ID

Microsoft Entra External ID is Microsoft’s customer- and partner-focused identity service, part of the broader Microsoft Entra identity suite. It unifies what used to be Azure AD External Identities (both B2B and B2C) into a single, scalable CIAM (Customer Identity and Access Management) platform that lets you securely onboard and manage “external” users—whether they’re business partners, consumers, citizens or contractors—using identities they already own. ([Microsoft Learn][1], [Microsoft Learn][2])

**Key scenarios**

* **B2B collaboration** (workforce tenant): Invite external partners or guests to access your corporate apps (e.g., Office 365, Teams, custom Line-of-Business applications) using their existing Azure AD, Microsoft Account, or social identities. You control what resources they see and how they authenticate. ([Microsoft Learn][1])
* **CIAM for consumers and customers** (external tenant): Build customer-facing apps with full-branding and self-service sign-up/sign-in experiences. Support social logins (Google, Facebook, Apple), custom OpenID Connect or SAML providers, and passwordless options—all managed in a dedicated Entra tenant separate from your employees. ([Microsoft Learn][1])

**Core features**

* **Multi-provider federation**: Out-of-the-box support for Azure AD, social IdPs, OIDC/SAML, and custom identity providers ([Microsoft Learn][3])
* **Customizable user journeys**: Fully branded sign-up, sign-in, password reset and profile editing flows with conditional access and MFA policies
* **Scalability & security**: Enterprise-grade SLAs, built-in DDoS protection, compliance with standards (ISO, GDPR, etc.)
* **Unified management**: Centralized administration in the Entra portal, plus APIs and PowerShell for automation

**Why “External ID”?**
Microsoft rebranded Azure AD B2B/B2C under the Entra umbrella to simplify choice: instead of separate B2B and B2C products, External ID converges both workloads in one service, streamlining licensing and developer experience. ([Microsoft Learn][2])

---

**Learn more**

* Overview: Microsoft Docs – “External Identities overview” ([Microsoft Learn][1])
* Getting started: Microsoft Docs – “Overview of Microsoft Entra External ID” ([Microsoft Learn][4])

[1]: https://learn.microsoft.com/en-us/entra/external-id/external-identities-overview?utm_source=chatgpt.com "Introduction to Microsoft Entra External ID"
[2]: https://learn.microsoft.com/en-us/answers/questions/1417764/difference-between-microsoft-entra-external-id-and?utm_source=chatgpt.com "Difference between Microsoft Entra External ID and Azure AD B2C"
[3]: https://learn.microsoft.com/en-us/entra/external-id/customers/faq-customers?utm_source=chatgpt.com "Microsoft Entra External ID frequently asked questions"
[4]: https://learn.microsoft.com/en-us/entra/external-id/?utm_source=chatgpt.com "Microsoft Entra External ID documentation"




Here’s a deep dive into the B2C (business-to-consumer) capabilities in Microsoft Entra External ID, Microsoft’s next-generation CIAM solution for customer-facing applications.

---
# Microsoft Entra External ID B2C 

## 1. CIAM Foundations

Microsoft Entra External ID B2C provides a fully managed, scale-out identity platform designed to handle millions of users and billions of logins per day. It takes care of high availability, threat protection (DDoS, brute-force, password spray), monitoring and automatic security updates—so you don’t have to build or operate your own identity infrastructure ([Microsoft Learn][1]).

---

## 2. Flexible Identity Providers

Your customers can sign up and sign in using:

* **Social providers** (e.g. Google, Facebook, Apple)
* **Enterprise IdPs** via SAML or OpenID Connect (e.g. ADFS, other Entra tenants)
* **Local accounts** (email + password or phone-based)
* **Custom OpenID Connect/SAML** (any compliant provider)
  All of these “bring-your-own-ID” options are configurable in a few clicks in the Entra portal ([Microsoft Learn][2], [Microsoft Learn][1]).

---

## 3. Customizable User Journeys

You get two approaches to define flows for sign-up, sign-in, profile editing and password reset:

1. **Built-in user flows**: Quickstart, no code—choose from common scenarios and apply branding (logo, CSS).
2. **Custom policies**: For advanced needs (e.g. multi-step attribute collection, custom claims transformations, bespoke UI orchestration), you can extend the underlying policy engine—without reprovisioning infrastructure ([Microsoft Learn][3]).

---

## 4. White-Label Branding & UI Control

* **HTML/CSS/JavaScript customization**: Modify every page so it matches your application’s look and feel.
* **Localization**: Over 50 languages supported out-of-the-box, with easy overrides.
* **Progressive profiling**: Collect additional attributes over time rather than all at once.
* **Custom domains**: Serve auth pages from your own URLs (e.g. `auth.contoso.com`). ([Microsoft Learn][1]).

---

## 5. Security & Compliance

* **Adaptive access policies**: Risk-based MFA (e.g. challenge on unfamiliar location, device).
* **Conditional Access**: Apply granular rules (e.g. block legacy auth, require MFA when risk is high).
* **Passwordless**: Support for phone-based OTP, FIDO2/WebAuthn, Microsoft Authenticator.
* **Standards compliance**: ISO 27001, SOC 1/2, GDPR, etc. ([Microsoft Learn][3]).

---

## 6. Developer Experience & Extensibility

* **APIs & SDKs**: OIDC/OAuth endpoints, .NET, JavaScript, Java, Python libraries.
* **Graph API and PowerShell**: Automate tenant/user management, registrations, policies.
* **Built-in analytics**: Track sign-up conversion, sign-in success/failure rates, and custom events.
* **Webhooks & event grid**: React to user lifecycle events (e.g. new sign-ups) from your back end. ([Microsoft Learn][2]).

---

## 7. Integration with External Systems

* **External user stores**: Delegate to your own database or CRM for profile data, using custom policies.
* **API connectors**: Call external REST APIs during user journeys (e.g. to validate promo codes or enrich profiles).
* **Identity provisioning**: Automated provisioning/de-provisioning into downstream applications via SCIM. ([Microsoft Learn][1]).

---

## 8. Tenant & Licensing Model

* **External tenant**: Separate from your workforce (employee) tenant—ideal for customer-facing apps.
* **MAU billing**: Pay for monthly active users; tiered pricing to scale from thousands to tens of millions.
* **Migration path**: Existing Azure AD B2C tenants remain supported until at least May 2030; you can migrate to External ID at your pace ([Microsoft Learn][3]).

---

## 9. Roadmap Highlights

Microsoft plans full feature parity and deeper capabilities—such as richer custom policy constructs, enhanced analytics dashboards, and out-of-the-box CIAM templates—by end of 2025. For the latest updates, keep an eye on the Entra External ID release notes.

---

**Next Steps**

1. **Try it out**: Create an External tenant in the Entra portal and experiment with built-in user flows.
2. **Define your branding**: Use the customization blade to upload your logo, CSS and text.
3. **Configure providers**: Add social/enterprise IdPs and test sign-in experiences.
4. **Automate with Graph**: Write a small script to provision a test user via the Microsoft Graph API.

This architecture will give you a secure, scalable, and fully branded CIAM platform to delight your customers while reducing operational overhead.

[1]: https://learn.microsoft.com/en-us/azure/active-directory-b2c/overview?utm_source=chatgpt.com "What is Azure Active Directory B2C? | Microsoft Learn"
[2]: https://learn.microsoft.com/en-us/entra/external-id/external-identities-overview?utm_source=chatgpt.com "Introduction to Microsoft Entra External ID"
[3]: https://learn.microsoft.com/en-us/entra/external-id/customers/faq-customers?utm_source=chatgpt.com "Microsoft Entra External ID frequently asked questions"
