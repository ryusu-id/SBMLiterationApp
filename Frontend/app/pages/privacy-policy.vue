<script setup lang="ts">
useSeoMeta({
  title: 'Privacy Policy - SIGMA',
  description: 'Privacy Policy for SIGMA reading progress tracking application'
})

definePageMeta({
  layout: 'landing'
})
</script>

<template>
  <UContainer class="py-12">
    <UPageHeader
      title="Privacy Policy"
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
                1. Introduction
              </h2>
              <p>
                SIGMA ("we," "our," or "us") is committed to protecting your privacy. This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use our reading progress tracking application.
              </p>
              <p>
                By using SIGMA, you agree to the collection and use of information in accordance with this policy.
              </p>
            </section>

            <!-- Information We Collect -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                2. Information We Collect
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                2.1 Information You Provide to Us
              </h3>
              <p>When you register and use SIGMA, we collect the following personal information:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Full Name:</strong> Used to identify you within the app</li>
                <li><strong>NIM (Student Identification Number):</strong> Used for account verification and identification</li>
                <li><strong>Study Program:</strong> Used to organize and display reading progress by academic program</li>
                <li><strong>Campus:</strong> Used to organize users by campus location</li>
                <li><strong>Class:</strong> Used to group and display reading progress by class</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                2.2 Information from Third-Party Services
              </h3>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Profile Picture:</strong> We collect your profile picture through Google OAuth authentication. This image is used to display your identity on leaderboards and admin pages.</li>
                <li><strong>Email Address:</strong> Collected through Google Sign-In for account authentication and communication purposes.</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                2.3 Reading Progress Data
              </h3>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Reading Progress Information:</strong> We collect and store data about your reading activities, including books read, pages completed, and progress statistics.</li>
                <li><strong>Timestamps:</strong> We record when you update your reading progress.</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                2.4 Automatically Collected Information
              </h3>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Device Information:</strong> We may collect information about your device, including device type and operating system.</li>
                <li><strong>Usage Data:</strong> We may collect information about how you interact with the app.</li>
              </ul>
            </section>

            <!-- How We Use Your Information -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                3. How We Use Your Information
              </h2>
              <p>We use the collected information for the following purposes:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Authentication:</strong> To verify your identity using Google OAuth</li>
                <li><strong>Display Progress:</strong> To show your reading progress to yourself and other users on leaderboards</li>
                <li><strong>Academic Organization:</strong> To organize users by campus, study program, and class</li>
                <li><strong>Account Management:</strong> To manage your account and provide customer support</li>
                <li><strong>App Improvement:</strong> To analyze usage patterns and improve our services</li>
                <li><strong>Communication:</strong> To send you important updates about the app (if necessary)</li>
              </ul>
            </section>

            <!-- How We Share Your Information -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                4. How We Share Your Information
              </h2>

              <h3 class="text-xl font-semibold mb-3">
                4.1 With Other Users
              </h3>
              <p>The following information is visible to other SIGMA users:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Your profile picture</li>
                <li>Your full name</li>
                <li>Your reading progress statistics</li>
                <li>Your ranking on leaderboards</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                4.2 With Administrators
              </h3>
              <p>Administrators have access to:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li>All information provided during user onboarding (Full Name, NIM, Study Program, Campus, Class)</li>
                <li>Your profile picture</li>
                <li>Your reading progress data</li>
                <li>Your account activity</li>
              </ul>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                4.3 We Do Not Sell Your Data
              </h3>
              <p>We do not sell, trade, or rent your personal information to third parties.</p>

              <h3 class="text-xl font-semibold mb-3 mt-4">
                4.4 Legal Requirements
              </h3>
              <p>We may disclose your information if required to do so by law or in response to valid requests by public authorities.</p>
            </section>

            <!-- Google OAuth -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                5. Google OAuth and API Scopes
              </h2>
              <p>SIGMA uses Google OAuth for authentication. We request the following permissions:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Basic Profile Information:</strong> To access your name and email address</li>
                <li><strong>Profile Picture:</strong> To display your image on leaderboards and admin pages</li>
              </ul>
              <p class="mt-3">
                You can revoke SIGMA's access to your Google account at any time by visiting your
                <a
                  href="https://myaccount.google.com/permissions"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="text-primary hover:underline"
                >Google Account permissions page</a>.
              </p>
            </section>

            <!-- Data Security -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                6. Data Security
              </h2>
              <p>We implement appropriate technical and organizational security measures to protect your personal information, including:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Secure data transmission using encryption (HTTPS/TLS)</li>
                <li>Access controls limiting who can view your data</li>
                <li>Regular security reviews and updates</li>
                <li>Secure authentication through Google OAuth</li>
              </ul>
              <p class="mt-3">
                However, please note that no method of transmission over the internet or electronic storage is 100% secure, and we cannot guarantee absolute security.
              </p>
            </section>

            <!-- Your Rights -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                7. Your Rights and Choices
              </h2>
              <p>You have the following rights regarding your personal information:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li><strong>Access:</strong> You can request access to the personal information we hold about you</li>
                <li><strong>Correction:</strong> You can update your onboarding information by contacting us</li>
                <li><strong>Deletion:</strong> You can request deletion of your account and associated data</li>
                <li><strong>Revoke OAuth Access:</strong> You can revoke SIGMA's access to your Google profile at any time through your Google Account settings</li>
                <li><strong>Opt-out of Leaderboard:</strong> You can request to be hidden from public leaderboards by contacting us</li>
              </ul>
              <p class="mt-3">
                To exercise any of these rights, please contact us using the information provided in Section 11.
              </p>
            </section>

            <!-- Data Retention -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                8. Data Retention
              </h2>
              <p>
                We retain your personal information for as long as your account is active or as needed to provide you with our services. If you request account deletion, we will remove your personal information within 30 days, except where we are required to retain it for legal purposes.
              </p>
            </section>

            <!-- Third-Party Services -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                9. Third-Party Services
              </h2>
              <p>SIGMA uses the following third-party services:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li>
                  <strong>Google OAuth:</strong> For authentication and profile information. Please review
                  <a
                    href="https://policies.google.com/privacy"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="text-primary hover:underline"
                  >Google's Privacy Policy</a>
                </li>
              </ul>
              <p class="mt-3">
                We are not responsible for the privacy practices of these third-party services.
              </p>
            </section>

            <!-- Children's Privacy -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                10. Children's Privacy
              </h2>
              <p>
                SIGMA is intended for use by students and does not have a minimum age restriction for student users. However, we do not knowingly collect information from children under 13 without parental consent as required by applicable laws. If you believe we have collected information from a child under 13 without proper consent, please contact us immediately.
              </p>
            </section>

            <!-- International Data Transfers -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                11. International Data Transfers
              </h2>
              <p>
                Your information may be transferred to and maintained on servers located outside of your country, where data protection laws may differ. By using SIGMA, you consent to such transfers.
              </p>
            </section>

            <!-- Changes to Policy -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                12. Changes to This Privacy Policy
              </h2>
              <p>We may update this Privacy Policy from time to time to reflect changes in our practices or for legal, operational, or regulatory reasons. We will notify you of any material changes by:</p>
              <ul class="list-disc pl-6 space-y-2">
                <li>Posting the new Privacy Policy on this page</li>
                <li>Updating the "Last Updated" date at the top of this policy</li>
                <li>Sending an email notification (if applicable)</li>
              </ul>
              <p class="mt-3">
                We encourage you to review this Privacy Policy periodically for any changes. Your continued use of SIGMA after any modifications indicates your acceptance of the updated policy.
              </p>
            </section>

            <!-- Contact Us -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                13. Contact Us
              </h2>
              <p>If you have any questions, concerns, or requests regarding this Privacy Policy or our data practices, please contact us at:</p>
              <ul class="list-none space-y-2 mt-3">
                <li><strong>Email:</strong> info@pure-tco.com</li>
                <li><strong>Address:</strong> GREEN LAKE CITY., CIPONDOH, TANGERANG, 15147, INDONESIA</li>
                <li><strong>Phone:</strong> 0878-7719-8886</li>
              </ul>
            </section>

            <!-- Developer Info -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                Developer Information
              </h2>
              <ul class="list-none space-y-2">
                <li><strong>App Name:</strong> SIGMA</li>
                <li><strong>Developer:</strong> ryusu.id</li>
                <li><strong>Purpose:</strong> Reading progress tracking and leaderboard application for students</li>
              </ul>
            </section>

            <!-- Summary -->
            <section>
              <h2 class="text-2xl font-bold mb-4">
                Summary of Key Points
              </h2>
              <UAlert
                color="primary"
                variant="soft"
                icon="i-heroicons-information-circle"
                title="Key Highlights"
              >
                <template #description>
                  <ul class="list-none space-y-2 mt-2">
                    <li>✅ We collect your name, NIM, study program, campus, class, and profile picture</li>
                    <li>✅ We use Google OAuth for authentication</li>
                    <li>✅ Your reading progress and profile picture are visible to other users on leaderboards</li>
                    <li>✅ Administrators can see all your onboarding information</li>
                    <li>✅ We do not sell your personal data</li>
                    <li>✅ You can request data deletion or revoke access at any time</li>
                    <li>✅ We implement security measures to protect your information</li>
                  </ul>
                </template>
              </UAlert>
              <p class="mt-4 text-sm text-gray-600 dark:text-gray-400">
                By using SIGMA, you acknowledge that you have read and understood this Privacy Policy.
              </p>
            </section>
          </div>
        </template>
      </UCard>
    </div>
  </UContainer>
</template>
