<#import "template.ftl" as layout>
<@layout.emailLayout preheader=msg("orderflowPasswordResetPreheader", realmName)>
    <@layout.heading text=msg("orderflowPasswordResetTitle") />
    <@layout.greeting />
    <p style="margin:0 0 16px 0;">${msg("orderflowPasswordResetIntro", realmName)}</p>
    <@layout.button href=link label=msg("orderflowPasswordResetAction") />
    <@layout.linkFooter href=link expiration=linkExpirationFormatter(linkExpiration) />
</@layout.emailLayout>
