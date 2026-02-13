<script setup lang="ts">
useSeoMeta({
  title: 'Terms of Service - SIGMA',
  description: 'Terms of Service for SIGMA reading progress tracking application'
})

definePageMeta({
  layout: 'landing'
})
</script>

<template>
  <UContainer class="py-12">
    <UPageHeader
      title="Terms of Service"
      description="Last Updated: February 12, 2026"
      class="mb-8"
    />

    <div class="prose prose-gray dark:prose-invert max-w-none">
      <UCard>
        <template #default>
          <div class="space-y-8">
            <!-- Introduction -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                1. Acceptance of Terms
              </h2>
              <p>
                Welcome to SIGMA! These Terms of Service ("Terms") constitute a legally binding agreement between you and SIGMA ("we," "our," or "us") regarding your use of our reading progress tracking application.
              </p>
              <p>
                By creating an account, accessing, or using SIGMA, you agree to be bound by these Terms and our
                <NuxtLink
                  to="/privacy-policy"
                  class="text-primary hover:underline"
                >Privacy Policy</NuxtLink>.
                If you do not agree to these Terms, you must not use SIGMA.
              </p>
            </section>

            <!-- Description of Service -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                2. Description of Service
              </h2>
              <p>
                SIGMA is a reading progress tracking application designed for students to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Log and track reading progress for books and journals</li>
                <li>Report reading sessions with page progress and time spent</li>
                <li>View reading statistics and achievements</li>
                <li>Participate in leaderboards to compare progress with other students</li>
                <li>Receive reading recommendations</li>
                <li>Complete daily reading challenges and quizzes</li>
              </ul>
            </section>

            <!-- User Accounts -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                3. User Accounts and Registration
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                3.1 Account Creation
              </h3>
              <p>
                To use SIGMA, you must create an account by signing in with your Google account and completing the onboarding process. You must provide accurate and complete information, including:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Full Name</li>
                <li>NIM (Student Identification Number)</li>
                <li>Study Program</li>
                <li>Campus</li>
                <li>Class</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                3.2 Account Eligibility
              </h3>
              <p>
                SIGMA is intended for students. By creating an account, you represent that:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>You are a student or authorized user</li>
                <li>All information you provide is accurate and truthful</li>
                <li>You will maintain the accuracy of your information</li>
                <li>You have the right to use the email address provided through Google OAuth</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                3.3 Account Security
              </h3>
              <p>
                You are responsible for maintaining the confidentiality of your Google account credentials. You agree to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Keep your Google account credentials secure</li>
                <li>Not share your account with others</li>
                <li>Notify us immediately of any unauthorized use of your account</li>
                <li>Be responsible for all activities that occur under your account</li>
              </ul>
            </section>

            <!-- User Responsibilities -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                4. User Responsibilities and Conduct
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                4.1 Acceptable Use
              </h3>
              <p>You agree to use SIGMA only for lawful purposes and in accordance with these Terms. You agree not to:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Provide false, inaccurate, or misleading information</li>
                <li>Use another person's account without permission</li>
                <li>Impersonate any person or entity</li>
                <li>Manipulate your reading progress data to gain unfair advantages on leaderboards</li>
                <li>Attempt to circumvent any security features</li>
                <li>Use automated scripts or bots to interact with the service</li>
                <li>Interfere with or disrupt the service or servers</li>
                <li>Engage in any activity that could harm SIGMA or other users</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                4.2 Reading Progress Accuracy
              </h3>
              <p>
                You are expected to report your reading progress honestly and accurately. Deliberately falsifying progress data is a violation of these Terms and may result in account suspension or termination.
              </p>
            </section>

            <!-- Reading Progress Tracking -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                5. Reading Progress Tracking
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                5.1 Content Ownership
              </h3>
              <p>
                You retain ownership of all data you input into SIGMA, including your reading progress, reports, and notes. By using SIGMA, you grant us a license to use, store, and display this data as necessary to provide the service.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                5.2 Data Accuracy
              </h3>
              <p>
                We do not verify the accuracy of reading progress you report. You are solely responsible for the accuracy and integrity of your reading data.
              </p>
            </section>

            <!-- Leaderboards and Public Information -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                6. Leaderboards and Public Information
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                6.1 Public Visibility
              </h3>
              <p>
                By using SIGMA, you acknowledge and agree that certain information will be publicly visible to other users, including:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Your profile picture (from Google account)</li>
                <li>Your full name</li>
                <li>Your reading progress statistics</li>
                <li>Your ranking on leaderboards</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                6.2 Leaderboard Participation
              </h3>
              <p>
                Participation in leaderboards is automatic. If you wish to be removed from public leaderboards, please contact us through the information provided in Section 17.
              </p>
            </section>

            <!-- Intellectual Property -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                7. Intellectual Property Rights
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                7.1 SIGMA's Property
              </h3>
              <p>
                SIGMA and its original content, features, and functionality are owned by SIGMA and are protected by international copyright, trademark, patent, trade secret, and other intellectual property laws.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                7.2 Limited License
              </h3>
              <p>
                We grant you a limited, non-exclusive, non-transferable, revocable license to access and use SIGMA for its intended purpose. This license does not include the right to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Copy, modify, or create derivative works</li>
                <li>Reverse engineer or decompile the application</li>
                <li>Rent, lease, sell, or sublicense the service</li>
                <li>Remove any copyright or proprietary notices</li>
              </ul>
            </section>

            <!-- Prohibited Conduct -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                8. Prohibited Conduct
              </h2>
              <p>
                In addition to the restrictions outlined in Section 4, you expressly agree not to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Upload or transmit viruses, malware, or other malicious code</li>
                <li>Collect or harvest personal data of other users</li>
                <li>Engage in any data mining, scraping, or similar data gathering</li>
                <li>Attempt to gain unauthorized access to any part of the service</li>
                <li>Use the service for any commercial purpose without our express consent</li>
                <li>Violate any applicable laws or regulations</li>
                <li>Harass, abuse, or harm other users</li>
              </ul>
            </section>

            <!-- Administrator Rights -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                9. Administrator Rights and Access
              </h2>
              <p>
                You acknowledge and agree that administrators of SIGMA have access to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>All user information provided during onboarding</li>
                <li>User reading progress data</li>
                <li>User account activity and history</li>
              </ul>
              <p class="mt-3">
                Administrators may use this access to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Manage and moderate the platform</li>
                <li>Verify data accuracy and integrity</li>
                <li>Provide support and resolve issues</li>
                <li>Enforce these Terms of Service</li>
                <li>Generate reports and analytics</li>
              </ul>
            </section>

            <!-- Data and Privacy -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                10. Data and Privacy
              </h2>
              <p>
                Your privacy is important to us. Our collection, use, and disclosure of your personal information is governed by our
                <NuxtLink
                  to="/privacy-policy"
                  class="text-primary hover:underline"
                >Privacy Policy</NuxtLink>,
                which is incorporated into these Terms by reference.
              </p>
              <p class="mt-3">
                By using SIGMA, you consent to the collection and use of your information as described in the Privacy Policy, including:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Collection of personal information through Google OAuth</li>
                <li>Storage and display of your reading progress data</li>
                <li>Public display of your profile picture, name, and statistics</li>
                <li>Administrator access to your account information</li>
              </ul>
            </section>

            <!-- Third-Party Services -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                11. Third-Party Services
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                11.1 Google OAuth
              </h3>
              <p>
                SIGMA uses Google OAuth for authentication. By using SIGMA, you also agree to comply with
                <a
                  href="https://policies.google.com/terms"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="text-primary hover:underline"
                >Google's Terms of Service</a>
                and
                <a
                  href="https://policies.google.com/privacy"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="text-primary hover:underline"
                >Privacy Policy</a>.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                11.2 Third-Party Content
              </h3>
              <p>
                SIGMA may include links to third-party websites or services (such as book information sources). We are not responsible for the content, accuracy, or practices of these third-party services. Your use of third-party services is at your own risk.
              </p>
            </section>

            <!-- Disclaimers -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                12. Disclaimers and Warranties
              </h2>

              <UAlert
                variant="soft"
                icon="i-heroicons-exclamation-triangle"
                title="Important Notice"
                class="mb-4"
              >
                <template #description>
                  SIGMA is provided on an "AS IS" and "AS AVAILABLE" basis without warranties of any kind.
                </template>
              </UAlert>

              <p>
                TO THE FULLEST EXTENT PERMITTED BY APPLICABLE LAW, WE DISCLAIM ALL WARRANTIES, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Warranties of merchantability, fitness for a particular purpose, and non-infringement</li>
                <li>Warranties that the service will be uninterrupted, secure, or error-free</li>
                <li>Warranties regarding the accuracy, reliability, or completeness of content</li>
                <li>Warranties that defects will be corrected</li>
              </ul>
              <p class="mt-3">
                We do not warrant that:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>The service will meet your specific requirements</li>
                <li>Your reading progress data will always be accurate or available</li>
                <li>The service will be compatible with all devices or platforms</li>
                <li>Leaderboard rankings will be free from errors or manipulation</li>
              </ul>
            </section>

            <!-- Limitation of Liability -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                13. Limitation of Liability
              </h2>
              <p>
                TO THE FULLEST EXTENT PERMITTED BY APPLICABLE LAW, SIGMA SHALL NOT BE LIABLE FOR:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Any indirect, incidental, special, consequential, or punitive damages</li>
                <li>Loss of profits, revenue, data, or use</li>
                <li>Loss of reading progress data or account information</li>
                <li>Unauthorized access to or alteration of your data</li>
                <li>Statements or conduct of any third party on the service</li>
                <li>Any other matter relating to the service</li>
              </ul>
              <p class="mt-3">
                This limitation applies whether the alleged liability is based on contract, tort, negligence, strict liability, or any other basis, even if we have been advised of the possibility of such damage.
              </p>
              <p class="mt-3">
                In jurisdictions that do not allow the exclusion of certain warranties or the limitation of liability for consequential damages, our liability shall be limited to the maximum extent permitted by law.
              </p>
            </section>

            <!-- Indemnification -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                14. Indemnification
              </h2>
              <p>
                You agree to indemnify, defend, and hold harmless SIGMA, its affiliates, officers, directors, employees, and agents from and against any claims, liabilities, damages, losses, costs, or expenses (including reasonable attorneys' fees) arising out of or related to:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Your use or misuse of SIGMA</li>
                <li>Your violation of these Terms</li>
                <li>Your violation of any rights of another person or entity</li>
                <li>Your violation of any applicable laws or regulations</li>
                <li>Any false or misleading information you provide</li>
              </ul>
            </section>

            <!-- Termination -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                15. Termination
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                15.1 Termination by You
              </h3>
              <p>
                You may terminate your account at any time by:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Contacting us to request account deletion</li>
                <li>Revoking SIGMA's access to your Google account through your Google Account settings</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                15.2 Termination by Us
              </h3>
              <p>
                We reserve the right to suspend or terminate your account at any time, with or without cause, and with or without notice, including if:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>You violate these Terms</li>
                <li>You provide false or misleading information</li>
                <li>You manipulate reading progress data</li>
                <li>You engage in prohibited conduct</li>
                <li>We believe your account poses a security risk</li>
                <li>We discontinue the service</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                15.3 Effect of Termination
              </h3>
              <p>
                Upon termination:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Your access to SIGMA will be immediately revoked</li>
                <li>Your reading progress data may be deleted after 30 days</li>
                <li>Your public profile information will be removed from leaderboards</li>
                <li>Sections of these Terms that by their nature should survive termination will remain in effect</li>
              </ul>
            </section>

            <!-- Changes to Terms -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                16. Changes to These Terms
              </h2>
              <p>
                We reserve the right to modify these Terms at any time. If we make material changes, we will notify you by:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Posting the updated Terms on this page</li>
                <li>Updating the "Last Updated" date</li>
                <li>Sending an email notification (if applicable)</li>
                <li>Displaying a notification within the app</li>
              </ul>
              <p class="mt-3">
                Your continued use of SIGMA after any changes indicates your acceptance of the new Terms. If you do not agree to the modified Terms, you must stop using SIGMA.
              </p>
            </section>

            <!-- Governing Law -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                17. Governing Law and Dispute Resolution
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                17.1 Governing Law
              </h3>
              <p>
                These Terms shall be governed by and construed in accordance with the laws of [Your Jurisdiction], without regard to its conflict of law provisions.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                17.2 Dispute Resolution
              </h3>
              <p>
                Any disputes arising out of or relating to these Terms or the service shall be resolved through:
              </p>
              <ul class="list-disc pl-6 space-y-2">
                <li>First, informal negotiation between you and SIGMA</li>
                <li>If unresolved, through binding arbitration or litigation in the courts of [Your Jurisdiction]</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                17.3 Waiver of Class Actions
              </h3>
              <p>
                You agree that any proceedings to resolve disputes will be conducted on an individual basis and not as a class action or other representative proceeding.
              </p>
            </section>

            <!-- Miscellaneous -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                18. Miscellaneous Provisions
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                18.1 Entire Agreement
              </h3>
              <p>
                These Terms, together with our Privacy Policy, constitute the entire agreement between you and SIGMA regarding the service.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                18.2 Severability
              </h3>
              <p>
                If any provision of these Terms is found to be invalid or unenforceable, the remaining provisions shall remain in full force and effect.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                18.3 Waiver
              </h3>
              <p>
                Our failure to enforce any right or provision of these Terms shall not constitute a waiver of such right or provision.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                18.4 Assignment
              </h3>
              <p>
                You may not assign or transfer these Terms or your account without our prior written consent. We may assign or transfer these Terms without restriction.
              </p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                18.5 Language
              </h3>
              <p>
                These Terms are provided in English. In the event of any conflict between an English version and a translated version, the English version shall prevail.
              </p>
            </section>

            <!-- Contact Information -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                19. Contact Information
              </h2>
              <p>
                If you have any questions about these Terms, please contact us at:
              </p>
              <ul class="list-none space-y-2 mt-3">
                <li><strong>Email:</strong> info@pure-tco.com</li>
                <li><strong>Address:</strong> GREEN LAKE CITY., CIPONDOH, TANGERANG, 15147, INDONESIA</li>
                <li><strong>Phone:</strong> 0878-7719-8886</li>
              </ul>
            </section>

            <!-- Acknowledgment -->
            <section>
              <UAlert
                color="primary"
                variant="soft"
                icon="i-heroicons-check-circle"
                title="Acknowledgment"
              >
                <template #description>
                  <p class="mt-2">
                    By using SIGMA, you acknowledge that:
                  </p>
                  <ul class="list-none space-y-2 mt-2">
                    <li>✅ You have read and understood these Terms of Service</li>
                    <li>✅ You agree to be bound by these Terms</li>
                    <li>✅ You have read and understood our Privacy Policy</li>
                    <li>✅ You are eligible to use the service</li>
                    <li>✅ You will use the service responsibly and honestly</li>
                  </ul>
                </template>
              </UAlert>
            </section>
          </div>
        </template>
      </UCard>
    </div>
  </UContainer>
</template>
