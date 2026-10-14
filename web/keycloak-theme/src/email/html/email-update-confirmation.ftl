<#import "template.ftl" as layout>
<@layout.emailLayout preheader=msg("orderflowEmailUpdateTitle")>
    <@layout.heading text=msg("orderflowEmailUpdateTitle") />
    <@layout.greeting />
    <p style="margin:0 0 16px 0;">${msg("orderflowEmailUpdateIntro", realmName, newEmail)}</p>
    <@layout.button href=link label=msg("orderflowEmailUpdateAction") />
    <@layout.linkFooter href=link expiration=linkExpirationFormatter(linkExpiration) />
</@layout.emailLayout>
