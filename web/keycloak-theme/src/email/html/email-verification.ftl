<#import "template.ftl" as layout>
<@layout.emailLayout preheader=msg("orderflowEmailVerificationPreheader", realmName)>
    <@layout.heading text=msg("orderflowEmailVerificationTitle") />
    <@layout.greeting />
    <p style="margin:0 0 16px 0;">${msg("orderflowEmailVerificationIntro", realmName)}</p>
    <@layout.button href=link label=msg("orderflowEmailVerificationAction") />
    <@layout.linkFooter href=link expiration=linkExpirationFormatter(linkExpiration) />
</@layout.emailLayout>
